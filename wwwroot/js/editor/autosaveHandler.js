/**
 * Autosave Handler for BlogEditor
 * Manages automatic saving of post drafts
 */

class AutosaveHandler {
    constructor(options = {}) {
        this.config = {
            interval: options.interval || 30000, // Default: 30 seconds
            editorSelector: options.editorSelector || '.blog-editor',
            contentSelector: options.contentSelector || '#Content',
            titleSelector: options.titleSelector || '#Title',
            descriptionSelector: options.descriptionSelector || '#Description',
            storageKeyPrefix: options.storageKeyPrefix || 'blogpost_draft_',
            maxDrafts: options.maxDrafts || 10
        };
        
        // State
        this.state = {
            lastSaved: null,
            isDirty: false,
            draftKey: '',
            isNew: true,
            contentChanged: false
        };
        
        // Elements
        this.editor = document.querySelector(this.config.editorSelector);
        this.contentInput = document.querySelector(this.config.contentSelector);
        this.titleInput = document.querySelector(this.config.titleSelector);
        this.descriptionInput = document.querySelector(this.config.descriptionSelector);
        
        // Check for post ID from URL
        this.postId = this.getPostIdFromUrl();
        this.state.draftKey = this.config.storageKeyPrefix + (this.postId || 'new');
        this.state.isNew = !this.postId;
        
        // Initialize
        this.init();
    }
    
    /**
     * Initialize autosave handler
     */
    init() {
        if (!this.editor || !this.contentInput) {
            console.error('Autosave Handler: Required elements not found');
            return;
        }
        
        // Check for existing draft
        this.checkForDrafts();
        
        // Set up event listeners
        this.setupEventListeners();
        
        // Start autosave interval
        this.startAutosaveInterval();
        
        // Add save indicator to the UI
        this.addSaveIndicator();
        
        console.log('AutosaveHandler initialized successfully');
    }
    
    /**
     * Set up event listeners for content changes
     */
    setupEventListeners() {
        // Track content changes
        this.editor.addEventListener('input', () => {
            this.state.isDirty = true;
            this.state.contentChanged = true;
        });
        
        // Track title and description changes
        if (this.titleInput) {
            this.titleInput.addEventListener('input', () => {
                this.state.isDirty = true;
            });
        }
        
        if (this.descriptionInput) {
            this.descriptionInput.addEventListener('input', () => {
                this.state.isDirty = true;
            });
        }
        
        // Listen for editor content changes
        document.addEventListener('blogEditorContentChange', () => {
            this.state.isDirty = true;
            this.state.contentChanged = true;
        });
        
        // Handle form submission - clear draft
        const form = document.getElementById('blogPostForm');
        if (form) {
            form.addEventListener('submit', () => {
                this.clearDraft();
            });
        }
        
        // Listen for page unload to save
        window.addEventListener('beforeunload', (e) => {
            if (this.state.isDirty) {
                this.saveDraft();
                
                // Show warning if there are unsaved changes
                e.preventDefault();
                e.returnValue = 'You have unsaved changes. Are you sure you want to leave?';
                return e.returnValue;
            }
        });
        
        // Listen for content clear event
        document.addEventListener('blogEditorContentCleared', () => {
            if (confirm('Do you want to clear draft data as well?')) {
                this.clearDraft();
                this.showSaveIndicator('Draft cleared');
            }
        });
    }
    
    /**
     * Start autosave interval
     */
    startAutosaveInterval() {
        setInterval(() => {
            if (this.state.isDirty) {
                this.saveDraft();
            }
        }, this.config.interval);
    }
    
    /**
     * Add save indicator to the UI
     */
    addSaveIndicator() {
        // Create indicator container
        const indicator = document.createElement('div');
        indicator.className = 'autosave-indicator';
        indicator.style.position = 'fixed';
        indicator.style.bottom = '20px';
        indicator.style.right = '20px';
        indicator.style.padding = '8px 15px';
        indicator.style.backgroundColor = 'rgba(25, 135, 84, 0.8)';
        indicator.style.color = 'white';
        indicator.style.borderRadius = '4px';
        indicator.style.fontSize = '14px';
        indicator.style.zIndex = '1050';
        indicator.style.display = 'none';
        indicator.style.transition = 'opacity 0.5s ease-in-out';
        indicator.innerHTML = 'Draft saved <i class="bi bi-check-circle"></i>';
        
        // Add clear button
        const clearBtn = document.createElement('button');
        clearBtn.innerHTML = '<i class="bi bi-trash"></i>';
        clearBtn.style.marginLeft = '10px';
        clearBtn.style.background = 'none';
        clearBtn.style.border = 'none';
        clearBtn.style.color = 'white';
        clearBtn.style.cursor = 'pointer';
        clearBtn.title = 'Clear draft';
        
        clearBtn.addEventListener('click', (e) => {
            e.stopPropagation();
            if (confirm('Are you sure you want to clear the current draft?')) {
                this.clearDraft();
                this.showSaveIndicator('Draft cleared');
                
                // Reset editor if needed
                if (this.state.contentChanged && confirm('Do you want to clear the editor content as well?')) {
                    if (window.blogEditor && typeof window.blogEditor.clearContent === 'function') {
                        window.blogEditor.clearContent();
                    } else {
                        // Fallback if the blogEditor instance is not accessible
                        if (this.editor) {
                            this.editor.innerHTML = '<p><br></p>';
                        }
                        if (this.contentInput) {
                            this.contentInput.value = '';
                        }
                    }
                }
            }
        });
        
        indicator.appendChild(clearBtn);
        document.body.appendChild(indicator);
        this.saveIndicator = indicator;
    }
    
    /**
     * Check for existing drafts
     */
    checkForDrafts() {
        const draft = this.loadDraft();
        
        if (draft) {
            // Show draft recovery prompt
            this.showDraftRecoveryPrompt(draft);
        }
    }
    
    /**
     * Show draft recovery prompt
     */
    showDraftRecoveryPrompt(draft) {
        // Create modal for draft recovery
        const modalId = 'draftRecoveryModal';
        
        // Check if modal already exists
        let modal = document.getElementById(modalId);
        
        if (!modal) {
            // Format date for display
            const savedDate = new Date(draft.timestamp);
            const formattedDate = savedDate.toLocaleString();
            
            // Create modal
            modal = document.createElement('div');
            modal.className = 'modal fade';
            modal.id = modalId;
            modal.tabIndex = -1;
            modal.setAttribute('aria-labelledby', `${modalId}Label`);
            modal.setAttribute('aria-hidden', 'true');
            
            modal.innerHTML = `
                <div class="modal-dialog">
                    <div class="modal-content">
                        <div class="modal-header">
                            <h5 class="modal-title" id="${modalId}Label">Recover Draft</h5>
                            <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>
                        </div>
                        <div class="modal-body">
                            <p>We found a saved draft from ${formattedDate}. Would you like to recover it?</p>
                            <div class="card mb-3">
                                <div class="card-header">Draft Preview</div>
                                <div class="card-body">
                                    <h6>${draft.title || '(No title)'}</h6>
                                    <p class="text-muted small">${draft.description || '(No description)'}</p>
                                </div>
                            </div>
                        </div>
                        <div class="modal-footer">
                            <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">Discard Draft</button>
                            <button type="button" class="btn btn-primary" id="recoverDraftBtn">Recover Draft</button>
                        </div>
                    </div>
                </div>
            `;
            
            document.body.appendChild(modal);
        }
        
        // Initialize modal
        const bsModal = new bootstrap.Modal(modal);
        bsModal.show();
        
        // Recover button handler
        document.getElementById('recoverDraftBtn').addEventListener('click', () => {
            this.restoreDraft(draft);
            bsModal.hide();
        });
        
        // If modal is dismissed, clear the draft
        modal.addEventListener('hidden.bs.modal', () => {
            this.clearDraft();
        });
    }
    
    /**
     * Get post ID from URL
     */
    getPostIdFromUrl() {
        const url = window.location.pathname;
        const matches = url.match(/\/Posts\/Edit\/(\d+)/);
        
        if (matches && matches.length > 1) {
            return matches[1];
        }
        
        return null;
    }
    
    /**
     * Save current draft
     */
    saveDraft() {
        if (!this.contentInput) return;
        
        // Get current content
        const content = this.contentInput.value;
        const title = this.titleInput ? this.titleInput.value : '';
        const description = this.descriptionInput ? this.descriptionInput.value : '';
        
        // Create draft object
        const draft = {
            content: content,
            title: title,
            description: description,
            timestamp: new Date().toISOString(),
            postId: this.postId
        };
        
        // Save to localStorage
        try {
            localStorage.setItem(this.state.draftKey, JSON.stringify(draft));
            
            // Also save to server if this is an edit of an existing post
            if (this.postId) {
                this.saveDraftToServer(draft);
            }
            
            // Update state
            this.state.lastSaved = new Date();
            this.state.isDirty = false;
            
            // Show save indicator
            this.showSaveIndicator();
            
            // Manage older drafts
            this.manageOldDrafts();
            
            // Trigger autosave event
            document.dispatchEvent(new CustomEvent('blogEditorAutosave', { 
                detail: { 
                    draft: draft, 
                    timestamp: this.state.lastSaved 
                }
            }));
        } catch (error) {
            console.error('Error saving draft:', error);
        }
    }
    
    /**
     * Save draft to server via AJAX
     * This ensures drafts are saved to the database, not just localStorage
     */
    saveDraftToServer(draft) {
        // Find form elements to get needed data
        const form = document.getElementById('blogPostForm');
        if (!form) return;
        
        // Get CSRF token
        const csrfToken = document.querySelector('input[name="__RequestVerificationToken"]')?.value;
        if (!csrfToken) {
            console.error('CSRF token not found, cannot save draft to server');
            return;
        }
        
        // Get editor content safely
        const getEditorContent = () => {
            const editor = document.querySelector('.blog-editor');
            const contentInput = document.getElementById('Content');
            
            if (editor && contentInput) {
                // Clone the editor content to avoid modifying the actual editor
                const editorClone = editor.cloneNode(true);
                
                // Process content for better storage 
                processEditorContent(editorClone);
                
                // Return the processed HTML content
                return editorClone.innerHTML;
            }
            
            return null;
        };
        
        // Create form data for submission
        const formData = new FormData(form);
        
        // Set content from the processed editor content
        const editorContent = getEditorContent();
        if (editorContent !== null) {
            formData.set('Content', editorContent);
        }
        
        // Ensure the status is set to draft
        formData.set('Status', '0'); // 0 = Draft in PostStatus enum
        
        // Use fetch API to send the draft to server using the dedicated SaveDraft endpoint
        fetch('/Posts/SaveDraft', {
            method: 'POST',
            body: formData,
            headers: {
                'X-Requested-With': 'XMLHttpRequest',
                'X-CSRF-TOKEN': csrfToken
            },
            credentials: 'same-origin'
        })
        .then(response => {
            if (!response.ok) {
                throw new Error(`HTTP error! Status: ${response.status}`);
            }
            return response.json();
        })
        .then(data => {
            if (data.success) {
                console.log('Draft saved to server successfully:', data.timestamp);
                // Show save notification to user
                this.showSaveIndicator(`Draft saved at ${new Date().toLocaleTimeString()}`);
            } else {
                console.error('Server reported error saving draft:', data.message);
                alert('Error saving draft: ' + data.message);
            }
        })
        .catch(error => {
            console.error('Error saving draft to server:', error);
            alert('Error saving draft. Please try again.');
        });
        
        // Helper function to prepare content for storage
        function processEditorContent(editor) {
            // Mark special content to preserve formatting
            markSpecialContent(editor);
            
            // Ensure videos have proper wrappers
            const iframes = editor.querySelectorAll('iframe');
            iframes.forEach(iframe => {
                if (!iframe.closest('.video-container')) {
                    const videoWrapper = document.createElement('div');
                    videoWrapper.className = 'video-container';
                    videoWrapper.setAttribute('contenteditable', 'false');
                    videoWrapper.style.position = 'relative';
                    videoWrapper.style.width = '100%';
                    videoWrapper.style.marginBottom = '20px';
                    videoWrapper.style.clear = 'both';
                    
                    // Create responsive wrapper
                    const wrapper = document.createElement('div');
                    wrapper.style.position = 'relative';
                    wrapper.style.paddingBottom = '56.25%'; // 16:9 aspect ratio
                    wrapper.style.height = '0';
                    wrapper.style.overflow = 'hidden';
                    wrapper.style.maxWidth = '100%';
                    
                    // Position iframe properly
                    iframe.style.position = 'absolute';
                    iframe.style.top = '0';
                    iframe.style.left = '0';
                    iframe.style.width = '100%';
                    iframe.style.height = '100%';
                    
                    // Wrap iframe
                    iframe.parentNode.insertBefore(videoWrapper, iframe);
                    wrapper.appendChild(iframe);
                    videoWrapper.appendChild(wrapper);
                }
            });
            
            // Ensure images have proper wrappers
            const images = editor.querySelectorAll('img:not([data-uploading="true"])');
            images.forEach(img => {
                // Make sure image has blog-image class
                img.classList.add('blog-image');
                
                if (!img.closest('.image-wrapper')) {
                    const wrapper = document.createElement('div');
                    wrapper.className = 'image-wrapper';
                    wrapper.style.display = 'inline-block';
                    wrapper.style.position = 'relative';
                    wrapper.style.margin = '5px 0';
                    wrapper.style.maxWidth = '100%';
                    
                    img.parentNode.insertBefore(wrapper, img);
                    wrapper.appendChild(img);
                }
            });
            
            // Clean up any temporary elements
            const tempElements = editor.querySelectorAll('.resize-handle, .image-dimensions, .video-toolbar, .latex-edit-btn');
            tempElements.forEach(el => el.remove());
        }
        
        // Mark special content with preservation attributes
        function markSpecialContent(editor) {
            // Process code blocks
            const codeBlocks = editor.querySelectorAll('pre code');
            codeBlocks.forEach(code => {
                code.setAttribute('data-preserve-content', 'true');
                if (code.parentElement && code.parentElement.tagName === 'PRE') {
                    code.parentElement.setAttribute('data-preserve-content', 'true');
                }
            });
            
            // Process LaTeX formulas
            const latexElements = editor.querySelectorAll('.latex-formula');
            latexElements.forEach(latex => {
                latex.setAttribute('data-preserve-content', 'true');
            });
            
            // Preserve table structures
            const tables = editor.querySelectorAll('table');
            tables.forEach(table => {
                table.setAttribute('data-preserve-structure', 'true');
            });
        }
    }
    
    /**
     * Load saved draft
     */
    loadDraft() {
        try {
            const savedDraft = localStorage.getItem(this.state.draftKey);
            
            if (savedDraft) {
                return JSON.parse(savedDraft);
            }
        } catch (error) {
            console.error('Error loading draft:', error);
        }
        
        return null;
    }
    
    /**
     * Restore draft to editor
     */
    restoreDraft(draft) {
        if (!draft) return;
        
        // Restore content
        if (this.contentInput && draft.content) {
            this.contentInput.value = draft.content;
            
            // Also update editor if it exists
            if (this.editor) {
                this.editor.innerHTML = draft.content;
                
                // Initialize the editor content
                if (window.blogEditor) {
                    window.blogEditor.processSpecialElements();
                    window.blogEditor.makeImagesResizable();
                    window.blogEditor.renderMathFormulas();
                }
            }
        }
        
        // Restore title
        if (this.titleInput && draft.title) {
            this.titleInput.value = draft.title;
        }
        
        // Restore description
        if (this.descriptionInput && draft.description) {
            this.descriptionInput.value = draft.description;
        }
        
        // Trigger content change event
        document.dispatchEvent(new CustomEvent('blogEditorContentChange'));
        
        // Reset state
        this.state.isDirty = false;
        this.state.lastSaved = new Date();
        this.state.contentChanged = true;
    }
    
    /**
     * Clear current draft
     */
    clearDraft() {
        try {
            localStorage.removeItem(this.state.draftKey);
            this.state.isDirty = false;
            this.state.contentChanged = false;
            
            // Trigger draft cleared event
            document.dispatchEvent(new CustomEvent('blogEditorDraftCleared'));
        } catch (error) {
            console.error('Error clearing draft:', error);
        }
    }
    
    /**
     * Show save indicator
     */
    showSaveIndicator(message) {
        if (!this.saveIndicator) return;
        
        // Set custom message if provided
        if (message) {
            // Store the original innerHTML
            const originalHTML = this.saveIndicator.innerHTML;
            
            // Set the new message
            this.saveIndicator.innerHTML = message;
            
            // Store the original background color
            const originalBgColor = this.saveIndicator.style.backgroundColor;
            
            // Change background color for "cleared" message
            if (message.includes('cleared')) {
                this.saveIndicator.style.backgroundColor = 'rgba(13, 110, 253, 0.8)'; // Blue for cleared
            }
            
            // Show indicator
            this.saveIndicator.style.display = 'block';
            this.saveIndicator.style.opacity = '1';
            
            // Hide after 3 seconds and restore original
            setTimeout(() => {
                this.saveIndicator.style.opacity = '0';
                setTimeout(() => {
                    this.saveIndicator.style.display = 'none';
                    this.saveIndicator.innerHTML = originalHTML;
                    this.saveIndicator.style.backgroundColor = originalBgColor;
                }, 500); // match transition duration
            }, 3000);
            
            return;
        }
        
        // Show indicator
        this.saveIndicator.style.display = 'block';
        this.saveIndicator.style.opacity = '1';
        
        // Hide after 3 seconds
        setTimeout(() => {
            this.saveIndicator.style.opacity = '0';
            setTimeout(() => {
                this.saveIndicator.style.display = 'none';
            }, 500); // match transition duration
        }, 3000);
    }
    
    /**
     * Manage older drafts (clean up)
     */
    manageOldDrafts() {
        try {
            // Get all keys
            const draftKeys = [];
            for (let i = 0; i < localStorage.length; i++) {
                const key = localStorage.key(i);
                if (key.startsWith(this.config.storageKeyPrefix)) {
                    draftKeys.push(key);
                }
            }
            
            // If number of drafts exceeds maxDrafts, remove oldest
            if (draftKeys.length > this.config.maxDrafts) {
                // Sort draft keys by timestamp
                const draftItems = draftKeys.map(key => {
                    try {
                        const draft = JSON.parse(localStorage.getItem(key));
                        return { key, timestamp: draft.timestamp };
                    } catch (e) {
                        return { key, timestamp: '0' };
                    }
                });
                
                // Sort by timestamp (oldest first)
                draftItems.sort((a, b) => {
                    return new Date(a.timestamp) - new Date(b.timestamp);
                });
                
                // Remove oldest drafts
                const toRemove = draftItems.slice(0, draftItems.length - this.config.maxDrafts);
                toRemove.forEach(item => {
                    localStorage.removeItem(item.key);
                });
            }
        } catch (error) {
            console.error('Error managing old drafts:', error);
        }
    }
}

// Initialize when DOM is loaded
document.addEventListener('DOMContentLoaded', () => {
    // Check if we're on a Create or Edit page
    const isCreatePage = window.location.pathname.includes('/Posts/Create');
    const isEditPage = window.location.pathname.includes('/Posts/Edit');
    
    if (!isCreatePage && !isEditPage) return;
    
    // Initialize after short delay to ensure editor is ready
    setTimeout(() => {
        window.autosaveHandler = new AutosaveHandler();
    }, 1000);
}); 