/**
 * Editor Fixes for BlogApp
 * Fixes various issues with the blog editor for post creation and editing
 */

document.addEventListener('DOMContentLoaded', function() {
    // Check if we're on a Create or Edit page
    const isCreatePage = window.location.pathname.includes('/Posts/Create');
    const isEditPage = window.location.pathname.includes('/Posts/Edit');
    
    if (!isCreatePage && !isEditPage) return;
    
    // Apply fixes once the editor is fully initialized
    setTimeout(() => {
        // 1. Fix Image Upload and Drag-Drop
        fixImageUpload();
        
        // 2. Fix Video Width Validation
        fixVideoWidthValidation();
        
        // 3. Fix Formula Cursor Position
        fixFormulaCursorPosition();
        
        // 4. Fix Link Insertion
        fixLinkInsertion();
        
        // 5. Remove Dark Mode
        removeDarkMode();
        
        // 6. Fix Duplicate Video on Save
        fixDuplicateVideoOnSave();
        
        // 7. Fix Edit Page Content Loading
        fixEditPageContentLoading();
        
        // 8. Fix Formulas Save Issue
        fixFormulasSaveIssue();
        
        // 9. Fix Video Button Infinite Loop
        fixVideoButtonIssue();
        
        // 10. Fix Image Toolbars and Resizing
        fixImageToolbarsAndResizing();
        
        // 11. Fix Bootstrap Modal Issues
        fixBootstrapModalIssues();
        
        // 12. Fix Video Style Issues
        fixVideoStyleIssues();
        
        // 13. Fix Video Toolbar Controls
        fixVideoToolbarControls();
        
        // 14. Fix CORS and Cross-Site Cookie Issues
        fixCorsAndCookieIssues();
        
        console.log('All editor fixes applied successfully, including CORS and cookie protections');
        
        // Update the global editorFixes object
        if (window.editorFixes) {
            window.editorFixes.fixVideoButtonIssue = fixVideoButtonIssue;
            window.editorFixes.fixFormulaCursorPosition = fixFormulaCursorPosition;
            window.editorFixes.fixLinkInsertion = fixLinkInsertion;
            window.editorFixes.fixEditPageContentLoading = fixEditPageContentLoading;
            window.editorFixes.processImages = processImages;
            window.editorFixes.fixImageToolbarsAndResizing = fixImageToolbarsAndResizing;
            window.editorFixes.fixImageUpload = fixImageUpload;
            window.editorFixes.fixBootstrapModalIssues = fixBootstrapModalIssues;
            window.editorFixes.fixVideoStyleIssues = fixVideoStyleIssues;
            window.editorFixes.fixCorsAndCookieIssues = fixCorsAndCookieIssues;
        }
    }, 500);
});

// Expose editor fixes for other modules to use
window.editorFixes = {
    fixVideoButtonIssue: function() {},
    fixFormulaCursorPosition: function() {},
    fixLinkInsertion: function() {},
    fixEditPageContentLoading: function() {},
    processImages: function() {
        processImages();
    },
    fixImageToolbarsAndResizing: function() {},
    fixImageUpload: function() {},
    fixBootstrapModalIssues: function() {},
    fixVideoStyleIssues: function() {},
    fixCorsAndCookieIssues: function() {}
};

/**
 * Process images in the editor
 * Makes sure all images have proper containers and toolbars
 */
function processImages() {
    const editor = document.getElementById('blogEditor');
    if (!editor) return;
    
    if (window.imageHandler && typeof window.imageHandler.processImages === 'function') {
        window.imageHandler.processImages();
    } else {
        console.warn('ImageHandler not available for processing images');
        
        // Basic fallback for when imageHandler is not available
        const images = editor.querySelectorAll('img:not([data-processed="true"])');
        images.forEach(img => {
            // Mark as processed to avoid duplication
            img.setAttribute('data-processed', 'true');
            
            // Check if image is already in a container
            let container = img.closest('.image-container');
            if (!container) {
                // Create container
                container = document.createElement('div');
                container.className = 'image-container';
                
                // Insert container before image
                img.parentNode.insertBefore(container, img);
                container.appendChild(img);
                
                // Add toolbar
                const toolbar = document.createElement('div');
                toolbar.className = 'image-toolbar';
                toolbar.innerHTML = `
                    <button type="button" title="Sola hizala"><i class="bi bi-align-start"></i></button>
                    <button type="button" title="Ortaya hizala"><i class="bi bi-align-center"></i></button>
                    <button type="button" title="Sağa hizala"><i class="bi bi-align-end"></i></button>
                    <button type="button" title="Resmi sil"><i class="bi bi-trash"></i></button>
                `;
                
                container.appendChild(toolbar);
                
                // Add event listeners for toolbar buttons
                const buttons = toolbar.querySelectorAll('button');
                buttons[0].addEventListener('click', () => alignImage(buttons[0], 'left'));
                buttons[1].addEventListener('click', () => alignImage(buttons[1], 'center'));
                buttons[2].addEventListener('click', () => alignImage(buttons[2], 'right'));
                buttons[3].addEventListener('click', () => {
                    if (confirm('Bu resmi silmek istediğinizden emin misiniz?')) {
                        container.remove();
                    }
                });
            }
        });
    }
}

/**
 * Fix Image Upload and Drag-Drop
 * Simple, effective image handling system
 */
function fixImageUpload() {
    // Find editor instance
    const editor = document.getElementById('blogEditor');
    if (!editor) return;
    
    // Fix Insert Image button
    const imageBtn = document.querySelector('.editor-tool[data-tool="image"]');
    if (imageBtn) {
        imageBtn.addEventListener('click', () => {
            // Create a file input
            const fileInput = document.createElement('input');
            fileInput.type = 'file';
            fileInput.accept = 'image/*';
            fileInput.style.display = 'none';
            
            // Add to body and trigger click
            document.body.appendChild(fileInput);
            fileInput.click();
            
            // Handle file selection
            fileInput.addEventListener('change', () => {
                if (fileInput.files && fileInput.files[0]) {
                    const file = fileInput.files[0];
                    simpleImageUpload(file, editor);
                }
                // Remove the input
                document.body.removeChild(fileInput);
            });
        });
    }
    
    // Fix drag and drop
    editor.addEventListener('dragover', (e) => {
        e.preventDefault();
        editor.classList.add('dragover');
    });
    
    editor.addEventListener('dragleave', () => {
        editor.classList.remove('dragover');
    });
    
    editor.addEventListener('drop', (e) => {
        e.preventDefault();
        editor.classList.remove('dragover');
        
        // Get the dropped files
        const files = e.dataTransfer.files;
        if (files && files.length > 0) {
            // Filter only image files
            for (let i = 0; i < files.length; i++) {
                if (files[i].type.startsWith('image/')) {
                    // Save selection before uploading
                    const selection = window.getSelection();
                    const range = selection.getRangeAt(0);
                    
                    // Upload and insert the image
                    simpleImageUpload(files[i], editor, range);
                    break; // Only process the first image
                }
            }
        }
    });
    
    // Add global key handler for image deletion
    editor.addEventListener('keydown', (e) => {
        if (e.key === 'Backspace' || e.key === 'Delete') {
            const selection = window.getSelection();
            if (selection.rangeCount === 0) return;
            
            // Check if selection is right before an image
            const range = selection.getRangeAt(0);
            if (!range.collapsed) return;
            
            // Find the next element after the cursor
            const node = range.startContainer;
            let nextNode = null;
            
            if (node.nodeType === Node.TEXT_NODE) {
                // If we're at the end of a text node, check the next sibling
                if (range.startOffset === node.textContent.length) {
                    let current = node;
                    while (current && !nextNode) {
                        nextNode = current.nextSibling;
                        if (!nextNode && current.parentNode !== editor) {
                            current = current.parentNode;
                        } else {
                            break;
                        }
                    }
                }
            } else if (node.nodeType === Node.ELEMENT_NODE) {
                // If we're in an element, check children at the offset
                if (range.startOffset < node.childNodes.length) {
                    nextNode = node.childNodes[range.startOffset];
                } else {
                    // At the end of the element, check next sibling
                    let current = node;
                    while (current && !nextNode) {
                        nextNode = current.nextSibling;
                        if (!nextNode && current.parentNode !== editor) {
                            current = current.parentNode;
                        } else {
                            break;
                        }
                    }
                }
            }
            
            // Check if next node is or contains an image
            if (nextNode) {
                const isOrContainsImage = (
                    (nextNode.nodeName === 'IMG') || 
                    (nextNode.nodeName === 'DIV' && nextNode.classList.contains('image-container')) ||
                    (nextNode.querySelector && nextNode.querySelector('img'))
                );
                
                if (isOrContainsImage) {
                    e.preventDefault();
                    
                    // Find the image container
                    const container = nextNode.classList?.contains('image-container') ? 
                        nextNode : nextNode.closest('.image-container') || nextNode.parentElement;
                    
                    // Remove the container or image
                    if (container) {
                        container.parentNode.removeChild(container);
                    } else if (nextNode.nodeName === 'IMG') {
                        nextNode.parentNode.removeChild(nextNode);
                    }
                    
                    // Update content
                    if (window.editor && window.editor.updateContent) {
                        window.editor.updateContent();
                    }
                }
            }
        }
    });
}

/**
 * Simple Image Upload - Basic but effective
 */
function simpleImageUpload(file, editor, savedRange = null) {
    // Create placeholder
    const placeholder = document.createElement('img');
    placeholder.src = URL.createObjectURL(file);
    placeholder.alt = 'Uploading...';
    placeholder.style.maxWidth = '100%';
    placeholder.style.opacity = '0.5';
    
    // Insert placeholder at cursor position
    if (savedRange) {
        const selection = window.getSelection();
        selection.removeAllRanges();
        selection.addRange(savedRange);
    }
    
    const selection = window.getSelection();
    if (selection.rangeCount > 0) {
        const range = selection.getRangeAt(0);
        range.deleteContents();
        range.insertNode(placeholder);
    } else {
        editor.appendChild(placeholder);
    }
    
    // Create form data for upload
    const formData = new FormData();
    formData.append('upload', file);
    
    // Get CSRF token
    const csrfToken = document.querySelector('input[name="__RequestVerificationToken"]')?.value || 
                      document.querySelector('meta[name="csrf-token"]')?.getAttribute('content');
    
    // Upload the image
    fetch('/Posts/UploadImage', {
        method: 'POST',
        body: formData,
        headers: {
            'X-CSRF-TOKEN': csrfToken,
            'X-Requested-With': 'XMLHttpRequest'
        },
        credentials: 'same-origin'
    })
    .then(response => response.json())
    .then(data => {
        if (data.location || data.url) {
            // Create image container
            const container = document.createElement('div');
            container.className = 'image-container';
            
            // Create the image
            const img = document.createElement('img');
            img.src = data.location || data.url;
            img.alt = file.name.split('.')[0] || 'Blog image';
            
            // Create toolbar
            const toolbar = document.createElement('div');
            toolbar.className = 'image-toolbar';
            
            // Left align button
            const leftBtn = document.createElement('button');
            leftBtn.type = 'button';
            leftBtn.title = 'Sola hizala';
            leftBtn.innerHTML = '<i class="bi bi-align-start"></i>';
            leftBtn.addEventListener('click', () => alignImage(leftBtn, 'left'));
            
            // Center align button
            const centerBtn = document.createElement('button');
            centerBtn.type = 'button';
            centerBtn.title = 'Ortaya hizala';
            centerBtn.innerHTML = '<i class="bi bi-align-center"></i>';
            centerBtn.addEventListener('click', () => alignImage(centerBtn, 'center'));
            
            // Right align button
            const rightBtn = document.createElement('button');
            rightBtn.type = 'button';
            rightBtn.title = 'Sağa hizala';
            rightBtn.innerHTML = '<i class="bi bi-align-end"></i>';
            rightBtn.addEventListener('click', () => alignImage(rightBtn, 'right'));
            
            // Remove button
            const removeBtn = document.createElement('button');
            removeBtn.type = 'button';
            removeBtn.title = 'Resmi sil';
            removeBtn.innerHTML = '<i class="bi bi-trash"></i>';
            removeBtn.addEventListener('click', () => {
                container.remove();
                // Update content
                if (window.editor && window.editor.updateContent) {
                    window.editor.updateContent();
                }
            });
            
            // Add buttons to toolbar
            toolbar.appendChild(leftBtn);
            toolbar.appendChild(centerBtn);
            toolbar.appendChild(rightBtn);
            toolbar.appendChild(removeBtn);
            
            // Add toolbar and image to container
            container.appendChild(toolbar);
            container.appendChild(img);
            
            // Replace placeholder with container
            placeholder.replaceWith(container);
            
            // Add line break for text to continue below
            const br = document.createElement('br');
            br.className = 'clearfix';
            container.after(br);
            
            // Update editor content
            if (window.editor && window.editor.updateContent) {
                setTimeout(() => window.editor.updateContent(), 100);
            }
        } else {
            console.error('Image upload failed:', data.message || 'Unknown error');
            alert('Image upload failed: ' + (data.message || 'Unknown error'));
            placeholder.remove();
        }
    })
    .catch(error => {
        console.error('Error uploading image:', error);
        alert('Error uploading image. Please try again.');
        placeholder.remove();
    });
}

/**
 * Align image container
 */
function alignImage(button, alignment) {
    const container = button.closest('.image-container');
    if (!container) return;
    
    // Remove all alignment classes
    container.classList.remove('align-left', 'align-center', 'align-right');
    
    // Add selected alignment class
    container.classList.add(`align-${alignment}`);
    
    // Highlight the active button
    const toolbar = button.closest('.image-toolbar');
    if (toolbar) {
        const buttons = toolbar.querySelectorAll('button');
        buttons.forEach(btn => btn.classList.remove('active'));
        button.classList.add('active');
    }
    
    // Update editor content
    if (window.editor && window.editor.updateContent) {
        window.editor.updateContent();
    }
}

/**
 * Fix Video Width Validation
 */
function fixVideoWidthValidation() {
    // Find editor instance
    const editorInstance = window.editor;
    if (!editorInstance) return;
    
    // Find insert video modal
    const videoModalId = 'videoInsertModal';
    
    // Wait for DOM to ensure modal is created
    const setupVideoValidation = () => {
        // Find the modal
        const videoModal = document.getElementById(videoModalId);
        if (!videoModal) {
            setTimeout(setupVideoValidation, 500);
            return;
        }
        
        // Find width input and insert button in the modal
        const widthInput = videoModal.querySelector('input[name="video-width"]');
        const insertBtn = videoModal.querySelector('#insertVideoBtn');
        
        if (!widthInput || !insertBtn) return;
        
        // Add event listener to validate width
        widthInput.addEventListener('input', function() {
            const width = parseFloat(this.value);
            
            if (isNaN(width) || width < 50 || width > 100) {
                // Show error for invalid width
                widthInput.classList.add('is-invalid');
                
                // Add or update validation message
                let feedbackEl = widthInput.nextElementSibling;
                if (!feedbackEl || !feedbackEl.classList.contains('invalid-feedback')) {
                    feedbackEl = document.createElement('div');
                    feedbackEl.className = 'invalid-feedback';
                    widthInput.parentNode.appendChild(feedbackEl);
                }
                
                // Set message based on the error type
                if (isNaN(width)) {
                    feedbackEl.textContent = 'Lütfen geçerli bir genişlik değeri girin.';
                } else if (width < 50) {
                    feedbackEl.textContent = 'Genişlik en az 50% olmalıdır.';
                } else if (width > 100) {
                    feedbackEl.textContent = 'Genişlik en fazla 100% olabilir.';
                }
                
                // Disable the insert button
                insertBtn.disabled = true;
            } else {
                // Valid width, remove error
                widthInput.classList.remove('is-invalid');
                
                // Remove validation message if exists
                const feedbackEl = widthInput.nextElementSibling;
                if (feedbackEl && feedbackEl.classList.contains('invalid-feedback')) {
                    feedbackEl.textContent = '';
                }
                
                // Enable the insert button
                insertBtn.disabled = false;
            }
        });
        
        // Set default value when modal is opened
        $(videoModal).on('shown.bs.modal', function() {
            // Set default width to 100% if not already set
            if (!widthInput.value) {
                widthInput.value = '100';
            }
            
            // Trigger validation
            widthInput.dispatchEvent(new Event('input'));
        });
        
        // Fix the original insert button handler to include timeout and loading state
        const originalInsertBtnClick = insertBtn.onclick;
        if (originalInsertBtnClick) {
            insertBtn.onclick = null;
            
            insertBtn.addEventListener('click', function(e) {
                e.preventDefault();
                
                // Check if width is valid
                const width = parseFloat(widthInput.value);
                if (isNaN(width) || width < 50 || width > 100) {
                    widthInput.focus();
                    return false;
                }
                
                // Add loading state to button
                const originalText = insertBtn.innerHTML;
                insertBtn.disabled = true;
                insertBtn.innerHTML = '<span class="spinner-border spinner-border-sm" role="status" aria-hidden="true"></span> Ekleniyor...';
                
                // Backup URL input element values
                const urlInputs = videoModal.querySelectorAll('input[type="url"], input[type="text"]');
                const urls = Array.from(urlInputs).map(input => input.value);
                
                // Call original handler with a small delay
                setTimeout(() => {
                    // Check if any input has a value
                    if (urls.some(url => url.trim())) {
                        try {
                            // Call original handler
                            originalInsertBtnClick.call(this, e);
                            
                            // Close modal after a successful insertion
                            setTimeout(() => {
                                try {
                                    bootstrap.Modal.getInstance(videoModal).hide();
                                } catch (modalError) {
                                    console.error('Error closing modal:', modalError);
                                }
                            }, 500);
                        } catch (error) {
                            console.error('Error inserting video:', error);
                            alert('Video eklenirken bir hata oluştu. Lütfen geçerli bir URL girdiğinizden emin olun.');
                        }
                    } else {
                        alert('Lütfen geçerli bir video URL\'si girin.');
                    }
                    
                    // Reset button state after a delay
                    setTimeout(() => {
                        insertBtn.disabled = false;
                        insertBtn.innerHTML = originalText;
                    }, 1000);
                }, 300);
            });
        }
    };
    
    // Start setup with a small delay to ensure editor is fully initialized
    setTimeout(setupVideoValidation, 500);
}

/**
 * Fix Formula Cursor Position
 */
function fixFormulaCursorPosition() {
    // Find blog editor
    const editor = document.querySelector('.blog-editor');
    if (!editor) return;
    
    // Check if we already patched the editor
    if (window.formulaCursorFixed) return;
    
    // Find LaTeX button
    const latexButton = document.querySelector('button[title="LaTeX Formula"]');
    if (latexButton) {
        const originalClickHandler = latexButton.onclick;
        latexButton.onclick = null;
        
        latexButton.addEventListener('click', function(e) {
            e.preventDefault();
            e.stopPropagation();
            
            // Store selection position for later
            const selection = window.getSelection();
            if (selection.rangeCount === 0) return;
            
            const range = selection.getRangeAt(0);
            const isInline = !range.collapsed || 
                (range.startContainer.nodeType === Node.TEXT_NODE && 
                 range.startContainer.textContent.trim().length > 0);
            
            // Note the parent node where we're inserting the formula
            const parentNode = range.startContainer.nodeType === Node.TEXT_NODE ? 
                range.startContainer.parentNode : range.startContainer;
            
            // Call original handler
            if (typeof originalClickHandler === 'function') {
                originalClickHandler.call(this, e);
            } else if (window.editor && window.editor.insertLaTeX) {
                window.editor.insertLaTeX();
            }
            
            // Observer to fix cursor position after formula insertion
            const formulaObserver = new MutationObserver((mutations) => {
                for (const mutation of mutations) {
                    if (mutation.type === 'childList' && mutation.addedNodes.length) {
                        for (const node of mutation.addedNodes) {
                            if (node.nodeType === 1 && node.classList.contains('latex-formula')) {
                                // Remove existing text nodes that might be causes issues
                                if (node.nextSibling && node.nextSibling.nodeType === 3 && node.nextSibling.textContent.trim() === '') {
                                    parentNode.removeChild(node.nextSibling);
                                }
                                
                                // Create a text node with zero-width space to place the cursor after formula
                                const textNode = document.createTextNode('\u200B');
                                
                                // Ensure we're not breaking flow - very important for inline formulas
                                if (node.nextSibling) {
                                    parentNode.insertBefore(textNode, node.nextSibling);
                                } else {
                                    parentNode.appendChild(textNode);
                                }
                                
                                // Position cursor after formula
                                const sel = window.getSelection();
                                const newRange = document.createRange();
                                newRange.setStartAfter(node);
                                newRange.collapse(true);
                                sel.removeAllRanges();
                                sel.addRange(newRange);
                                
                                // For inline formulas, ensure we don't create a new paragraph
                                if (isInline && node.parentNode.tagName === 'P') {
                                    // If a new paragraph was created, we need to fix it
                                    const paragraph = node.parentNode;
                                    if (paragraph.nextSibling && paragraph.nextSibling.nodeName === 'P') {
                                        // Remove empty paragraphs
                                        if (paragraph.nextSibling.textContent.trim() === '') {
                                            paragraph.parentNode.removeChild(paragraph.nextSibling);
                                        }
                                    }
                                }
                                
                                // Set display style for formula based on context
                                if (isInline) {
                                    // Make sure the formula is displayed inline
                                    node.style.display = 'inline';
                                    
                                    // Also ensure the LaTeX starts with $ not $$
                                    const content = node.textContent;
                                    if (content.startsWith('$$') && content.endsWith('$$')) {
                                        node.textContent = content.replace(/^\$\$(.*)\$\$$/, '$1');
                                        node.setAttribute('data-mode', 'inline');
                                        
                                        // Force MathJax to re-render
                                        if (typeof MathJax !== 'undefined') {
                                            MathJax.typesetPromise([node]).catch(err => {
                                                console.error('MathJax error:', err);
                                            });
                                        }
                                    }
                                }
                                
                                // Disconnect the observer after fixing
                                formulaObserver.disconnect();
                                
                                // Wait a bit to make sure any other post-processing is done
                                setTimeout(() => {
                                    // Force cursor position again after a small delay
                                    const finalSel = window.getSelection();
                                    const finalRange = document.createRange();
                                    finalRange.setStartAfter(node);
                                    finalRange.collapse(true);
                                    finalSel.removeAllRanges();
                                    finalSel.addRange(finalRange);
                                }, 50);
                                
                                break;
                            }
                        }
                    }
                }
            });
            
            // Observe editor for formula insertion
            formulaObserver.observe(editor, { childList: true, subtree: true });
            
            // Disconnect observer after a timeout (safety)
            setTimeout(() => formulaObserver.disconnect(), 2000);
        });
        
        // Mark as fixed
        window.formulaCursorFixed = true;
    }
    
    // Add styles to fix formula display
    const formulaStyles = document.createElement('style');
    formulaStyles.textContent = `
        .blog-editor .latex-formula {
            display: inline-block;
            margin: 0 3px;
            vertical-align: middle;
        }
        .blog-editor .latex-formula[data-mode="inline"] {
            display: inline;
        }
    `;
    document.head.appendChild(formulaStyles);
}

/**
 * Fix Link Insertion
 */
function fixLinkInsertion() {
    // Find blog editor
    const editor = document.querySelector('.blog-editor');
    if (!editor) return;
    
    // Check if we already patched the button
    if (window.linkInsertionFixed) return;
    
    // Fix link button
    const linkButton = document.querySelector('button[title="Insert Link"]');
    if (linkButton) {
        const originalClickHandler = linkButton.onclick;
        linkButton.onclick = null;
        
        linkButton.addEventListener('click', function(e) {
            e.preventDefault();
            e.stopPropagation();
            
            // Get the selection
            const selection = window.getSelection();
            const selectedText = selection.toString();
            
            // Create a modal for link insertion
            const modalId = 'linkInsertModal';
            let modal = document.getElementById(modalId);
            
            if (!modal) {
                modal = document.createElement('div');
                modal.id = modalId;
                modal.className = 'modal fade';
                modal.tabIndex = -1;
                modal.setAttribute('aria-labelledby', `${modalId}Label`);
                modal.setAttribute('aria-hidden', 'true');
                
                modal.innerHTML = `
                    <div class="modal-dialog">
                        <div class="modal-content">
                            <div class="modal-header">
                                <h5 class="modal-title" id="${modalId}Label">Insert Link</h5>
                                <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>
                            </div>
                            <div class="modal-body">
                                <div class="mb-3">
                                    <label for="linkText" class="form-label">Link Text</label>
                                    <input type="text" class="form-control" id="linkText" placeholder="Link display text">
                                </div>
                                <div class="mb-3">
                                    <label for="linkUrl" class="form-label">URL</label>
                                    <input type="text" class="form-control" id="linkUrl" placeholder="https://example.com">
                                </div>
                                <div class="form-check mb-3">
                                    <input class="form-check-input" type="checkbox" id="linkNewTab" checked>
                                    <label class="form-check-label" for="linkNewTab">
                                        Open in new tab
                                    </label>
                                </div>
                            </div>
                            <div class="modal-footer">
                                <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">Cancel</button>
                                <button type="button" class="btn btn-primary" id="insertLinkBtn">Insert Link</button>
                            </div>
                        </div>
                    </div>
                `;
                
                document.body.appendChild(modal);
                
                // Store selection when the modal is shown
                let savedSelection = null;
                modal.addEventListener('show.bs.modal', function() {
                    savedSelection = saveSelection();
                    
                    // Fill link text with selected text
                    const linkTextInput = document.getElementById('linkText');
                    if (linkTextInput && selectedText) {
                        linkTextInput.value = selectedText;
                    }
                });
                
                // Handle insert button click
                document.getElementById('insertLinkBtn').addEventListener('click', function() {
                    const linkText = document.getElementById('linkText').value.trim();
                    const linkUrl = document.getElementById('linkUrl').value.trim();
                    const newTab = document.getElementById('linkNewTab').checked;
                    
                    if (linkUrl) {
                        // Restore selection
                        if (savedSelection) {
                            restoreSelection(savedSelection);
                        }
                        
                        // Create link HTML
                        let linkHtml = `<a href="${linkUrl}"`;
                        if (newTab) {
                            linkHtml += ` target="_blank" rel="noopener noreferrer"`;
                        }
                        linkHtml += `>${linkText || linkUrl}</a>`;
                        
                        // Insert the link
                        document.execCommand('insertHTML', false, linkHtml);
                        
                        // Close modal
                        bootstrap.Modal.getInstance(modal).hide();
                        
                        // Reset form
                        document.getElementById('linkText').value = '';
                        document.getElementById('linkUrl').value = '';
                    }
                });
            } else {
                // Reset form fields if modal already exists
                const linkTextInput = document.getElementById('linkText');
                const linkUrlInput = document.getElementById('linkUrl');
                if (linkTextInput) linkTextInput.value = selectedText || '';
                if (linkUrlInput) linkUrlInput.value = '';
            }
            
            // Show the modal
            const bsModal = new bootstrap.Modal(modal);
            bsModal.show();
        });
        
        // Mark as fixed
        window.linkInsertionFixed = true;
    }
    
    // Enable auto-linking of pasted URLs
    if (!editor.dataset.autoLinkEnabled) {
        editor.addEventListener('paste', function(e) {
            // Let the paste happen normally
            setTimeout(() => {
                // Get the selection and newly pasted content
                const selection = window.getSelection();
                if (!selection.rangeCount) return;
                
                const range = selection.getRangeAt(0);
                const startNode = range.startContainer;
                
                // Check for text nodes with URLs
                if (startNode.nodeType === 3) { // Text node
                    const text = startNode.textContent;
                    
                    // URL regex pattern
                    const urlPattern = /https?:\/\/(www\.)?[-a-zA-Z0-9@:%._\+~#=]{1,256}\.[a-zA-Z0-9()]{1,6}\b([-a-zA-Z0-9()@:%_\+.~#?&//=]*)/gi;
                    
                    // Find URLs in the text
                    let match;
                    let lastIndex = 0;
                    let hasUrls = false;
                    
                    // Create a document fragment for the replacement
                    const fragment = document.createDocumentFragment();
                    
                    while ((match = urlPattern.exec(text)) !== null) {
                        hasUrls = true;
                        
                        // Add text before URL
                        if (match.index > lastIndex) {
                            fragment.appendChild(document.createTextNode(text.substring(lastIndex, match.index)));
                        }
                        
                        // Create link for URL
                        const link = document.createElement('a');
                        link.href = match[0];
                        link.textContent = match[0];
                        link.target = '_blank';
                        link.rel = 'noopener noreferrer';
                        fragment.appendChild(link);
                        
                        lastIndex = match.index + match[0].length;
                    }
                    
                    // If URLs were found, replace the node
                    if (hasUrls) {
                        // Add remaining text
                        if (lastIndex < text.length) {
                            fragment.appendChild(document.createTextNode(text.substring(lastIndex)));
                        }
                        
                        // Replace the text node with the fragment
                        startNode.parentNode.replaceChild(fragment, startNode);
                        
                        // Update editor content
                        if (window.editor && window.editor.updateContent) {
                            window.editor.updateContent();
                        }
                    }
                }
            }, 0);
        });
        
        // Mark as processed
        editor.dataset.autoLinkEnabled = 'true';
    }
}

/**
 * Remove Dark Mode
 */
function removeDarkMode() {
    // Remove dark mode toggle button
    const darkModeBtn = document.querySelector('.btn[title="Toggle Dark Mode"]');
    if (darkModeBtn) {
        darkModeBtn.remove();
    }
    
    // Remove dark mode from editor if applied
    const editor = document.querySelector('.blog-editor');
    if (editor) {
        editor.classList.remove('dark-mode');
    }
    
    // Remove dark mode from toolbar if applied
    const toolbar = document.querySelector('.blog-editor-toolbar');
    if (toolbar) {
        toolbar.classList.remove('dark-mode');
    }
    
    // Add style to ensure dark mode can't be applied
    const style = document.createElement('style');
    style.textContent = `
        .blog-editor.dark-mode, 
        .blog-editor-toolbar.dark-mode {
            background-color: #fff !important;
            color: #212529 !important;
            border-color: #dee2e6 !important;
        }
    `;
    document.head.appendChild(style);
    
    // Override toggleDarkMode method if it exists
    const editorInstance = window.editor;
    if (editorInstance && editorInstance.toggleDarkMode) {
        editorInstance.toggleDarkMode = function() {
            // Do nothing
            return false;
        };
    }
}

/**
 * Fix Duplicate Video on Save
 */
function fixDuplicateVideoOnSave() {
    // Find blog form
    const form = document.getElementById('blogPostForm');
    if (!form) return;
    
    // Intercept form submission
    form.addEventListener('submit', function(e) {
        // Find editor content and content input
        const editor = document.querySelector('.blog-editor');
        const contentInput = document.getElementById('Content');
        
        if (!editor || !contentInput) return;
        
        // Create a clone to process
        const clone = editor.cloneNode(true);
        
        // Find all videos in the clone
        const videoContainers = clone.querySelectorAll('.video-container');
        const processedVideoIds = new Set();
        
        // First pass: identify all unique video IDs
        videoContainers.forEach(container => {
            const iframe = container.querySelector('iframe');
            if (!iframe) return;
            
            const src = iframe.src;
            const videoId = src.includes('embed/') ? 
                src.split('embed/')[1].split('?')[0] : '';
            
            if (videoId) {
                // Store with its position to keep the first occurrence
                if (!container.hasAttribute('data-processed')) {
                    container.setAttribute('data-processed', 'true');
                    processedVideoIds.add(videoId);
                }
            }
        });
        
        // Second pass: remove duplicate videos
        videoContainers.forEach(container => {
            const iframe = container.querySelector('iframe');
            if (!iframe) return;
            
            const src = iframe.src;
            const videoId = src.includes('embed/') ? 
                src.split('embed/')[1].split('?')[0] : '';
            
            if (!videoId) return;
            
            // If already processed and this is a duplicate, remove it
            if (processedVideoIds.has(videoId) && !container.hasAttribute('data-processed')) {
                container.remove();
            }
        });
        
        // Update content input with the processed HTML
        contentInput.value = clone.innerHTML;
    }, true); // Use capturing to ensure our handler runs before the form's handler
    
    // Also add a cleanup function to the editor instance
    const editorInstance = window.editor;
    if (editorInstance) {
        editorInstance.cleanupVideos = function() {
            const editor = document.querySelector('.blog-editor');
            if (!editor) return;
            
            // Find all videos in the editor
            const videoContainers = editor.querySelectorAll('.video-container');
            const processedVideoIds = new Set();
            
            // First pass: identify all unique video IDs
            videoContainers.forEach(container => {
                const iframe = container.querySelector('iframe');
                if (!iframe) return;
                
                const src = iframe.src;
                const videoId = src.includes('embed/') ? 
                    src.split('embed/')[1].split('?')[0] : '';
                
                if (videoId) {
                    // Store with its position to keep the first occurrence
                    if (!container.hasAttribute('data-processed')) {
                        container.setAttribute('data-processed', 'true');
                        processedVideoIds.add(videoId);
                    }
                }
            });
            
            // Second pass: remove duplicate videos
            videoContainers.forEach(container => {
                const iframe = container.querySelector('iframe');
                if (!iframe) return;
                
                const src = iframe.src;
                const videoId = src.includes('embed/') ? 
                    src.split('embed/')[1].split('?')[0] : '';
                
                if (!videoId) return;
                
                // If already processed and this is a duplicate, remove it
                if (processedVideoIds.has(videoId) && !container.hasAttribute('data-processed')) {
                    container.remove();
                }
            });
            
            // Clean up attributes
            videoContainers.forEach(container => {
                container.removeAttribute('data-processed');
            });
            
            // Update content
            this.updateContent();
        };
        
        // Add cleanup to editor initialization
        setTimeout(() => {
            if (editorInstance.cleanupVideos) {
                editorInstance.cleanupVideos();
            }
        }, 1000);
    }
}

/**
 * Fix Edit Page Content Loading
 */
function fixEditPageContentLoading() {
    // Only apply this fix to edit pages
    if (!window.location.pathname.includes('/Posts/Edit/')) return;
    
    // Find editor and content input
    const editor = document.querySelector('.blog-editor');
    const contentInput = document.getElementById('Content');
    
    if (editor && contentInput && contentInput.value && !editor.innerHTML.trim()) {
        // Editor is empty but content input has value - set the content
        editor.innerHTML = contentInput.value;
        
        // Notify editor instance of the content change
        const editorInstance = window.editor;
        if (editorInstance && editorInstance.updateContent) {
            editorInstance.updateContent();
        }
        
        // Make images resizable
        const imageHandler = window.imageHandler;
        if (imageHandler && imageHandler.makeImagesResizable) {
            imageHandler.makeImagesResizable();
        }
        
        // Add toolbars to videos
        if (editorInstance && editorInstance.addVideoToolbar) {
            const videoContainers = editor.querySelectorAll('.video-container');
            videoContainers.forEach(container => {
                editorInstance.addVideoToolbar(container);
            });
        }
        
        // Render LaTeX formulas
        if (typeof MathJax !== 'undefined') {
            MathJax.typesetPromise([editor]).catch(err => {
                console.error('MathJax error:', err);
            });
        }
    }
}

/**
 * Fix Formulas Save Issue
 */
function fixFormulasSaveIssue() {
    // Find blog form
    const form = document.getElementById('blogPostForm');
    if (!form) return;
    
    // Add attribute to ensure LaTeX content is preserved 
    form.addEventListener('submit', function(e) {
        // Find editor and content input
        const editor = document.querySelector('.blog-editor');
        const contentInput = document.getElementById('Content');
        
        if (editor && contentInput) {
            // Process LaTeX formulas to ensure they are properly preserved
            const formulas = editor.querySelectorAll('.latex-formula');
            formulas.forEach(formula => {
                // Ensure data-preserve-content attribute is set
                formula.setAttribute('data-preserve-content', 'true');
                
                // Add an additional class to make formulas more identifiable
                formula.classList.add('latex-preserved');
                
                // Make sure the raw LaTeX content is preserved
                if (!formula.getAttribute('data-raw-formula')) {
                    // Extract formula content
                    const content = formula.textContent || '';
                    
                    // Store raw formula
                    const rawFormula = content.replace(/^\$\$(.*)\$\$$/, '$1');
                    formula.setAttribute('data-raw-formula', rawFormula);
                    
                    // Ensure the formula HTML structure is correct
                    if (!content.startsWith('$$') || !content.endsWith('$$')) {
                        formula.textContent = `$$${rawFormula}$$`;
                    }
                }
            });
            
            // Update content input with the processed HTML
            contentInput.value = editor.innerHTML;
        }
    }, true); // Use capturing to ensure our handler runs before the form's handler
}

/**
 * Fix Video Button Infinite Loop
 */
function fixVideoButtonIssue() {
    // Find blog editor
    const editor = document.querySelector('.blog-editor');
    if (!editor) return;
    
    // Check if we already patched the button (use a flag to prevent multiple execution)
    if (window.videoButtonFixed) return;
    
    // Patch the video button if it exists
    const videoButton = document.querySelector('button[title="Insert Video"]');
    if (videoButton) {
        // Remove existing handler and add a new one
        const originalClickHandler = videoButton.onclick;
        videoButton.onclick = null;
        
        videoButton.addEventListener('click', function(e) {
            e.preventDefault();
            e.stopPropagation();
            
            // Prevent multiple executions by setting a cooldown
            if (videoButton.dataset.processing === 'true') return;
            videoButton.dataset.processing = 'true';
            
            // Call the original handler directly if available
            if (typeof originalClickHandler === 'function') {
                originalClickHandler.call(this, e);
            } else if (window.editor && window.editor.insertVideo) {
                window.editor.insertVideo();
            }
            
            // Reset the processing state after a delay
            setTimeout(() => {
                videoButton.dataset.processing = 'false';
            }, 1000);
        });
        
        // Mark as fixed
        window.videoButtonFixed = true;
    }
}

/**
 * Helper: Save Selection
 */
function saveSelection() {
    const selection = window.getSelection();
    if (selection.rangeCount > 0) {
        return selection.getRangeAt(0).cloneRange();
    }
    return null;
}

/**
 * Helper: Restore Selection
 */
function restoreSelection(range) {
    if (range) {
        const selection = window.getSelection();
        selection.removeAllRanges();
        selection.addRange(range);
    }
}

// Add debugging capabilities
function addEditorDebugTools() {
    const editor = document.querySelector('.blog-editor');
    const form = document.getElementById('blogPostForm');
    if (!editor || !form) return;
    
    // Create debug button
    const debugBtn = document.createElement('button');
    debugBtn.type = 'button';
    debugBtn.className = 'btn btn-sm btn-warning btn-debug';
    debugBtn.textContent = 'Debug Editor';
    debugBtn.style.position = 'fixed';
    debugBtn.style.bottom = '20px';
    debugBtn.style.right = '20px';
    debugBtn.style.zIndex = '9999';
    
    // Add debug functionality
    debugBtn.addEventListener('click', () => {
        // Create debug info modal
        const modal = document.createElement('div');
        modal.className = 'modal fade';
        modal.id = 'editorDebugModal';
        modal.tabIndex = '-1';
        
        // Get editor state info
        const editorInstance = window.editor;
        const debugInfo = {
            'Editor Content Length': editor.innerHTML.length,
            'Hidden Input Value Length': document.getElementById('Content')?.value.length || 0,
            'Images': editor.querySelectorAll('img').length,
            'Videos': editor.querySelectorAll('.video-container').length,
            'Formulas': editor.querySelectorAll('.latex-formula').length,
            'Links': editor.querySelectorAll('a').length,
            'Editor Instance': editorInstance ? 'Available' : 'Not available',
            'Image Handler': window.imageHandler ? 'Available' : 'Not available',
            'LaTeX Handler': window.latexHandler ? 'Available' : 'Not available',
            'Dark Mode': editor.classList.contains('dark-mode') ? 'Enabled' : 'Disabled',
            'Browser': navigator.userAgent
        };
        
        // Create modal content
        let debugInfoHtml = '';
        for (const [key, value] of Object.entries(debugInfo)) {
            debugInfoHtml += `<tr><td><strong>${key}</strong></td><td>${value}</td></tr>`;
        }
        
        modal.innerHTML = `
            <div class="modal-dialog modal-lg">
                <div class="modal-content">
                    <div class="modal-header">
                        <h5 class="modal-title">Editor Debug Information</h5>
                        <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>
                    </div>
                    <div class="modal-body">
                        <div class="alert alert-info">
                            This information can help troubleshoot editor issues.
                        </div>
                        <table class="table table-striped">
                            <tbody>
                                ${debugInfoHtml}
                            </tbody>
                        </table>
                        <div class="mb-3">
                            <label class="form-label">View Editor HTML</label>
                            <textarea class="form-control" rows="10" readonly>${editor.innerHTML}</textarea>
                        </div>
                    </div>
                    <div class="modal-footer">
                        <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">Close</button>
                        <button type="button" class="btn btn-warning" id="forceSaveContentBtn">Force Update Content</button>
                    </div>
                </div>
            </div>
        `;
        
        document.body.appendChild(modal);
        
        // Show modal
        const bsModal = new bootstrap.Modal(modal);
        bsModal.show();
        
        // Add force update handler
        document.getElementById('forceSaveContentBtn').addEventListener('click', () => {
            const contentInput = document.getElementById('Content');
            if (contentInput && editor) {
                contentInput.value = editor.innerHTML;
                alert('Content updated! You can now submit the form to save changes.');
            }
        });
        
        // Remove modal when hidden
        modal.addEventListener('hidden.bs.modal', () => {
            document.body.removeChild(modal);
        });
    });
    
    // Add button to document body
    document.body.appendChild(debugBtn);
}

// Initialize debug tools on both Create and Edit pages
document.addEventListener('DOMContentLoaded', function() {
    // Check if we're on a Create or Edit page
    const isCreatePage = window.location.pathname.includes('/Posts/Create');
    const isEditPage = window.location.pathname.includes('/Posts/Edit');
    
    if (isCreatePage || isEditPage) {
        setTimeout(addEditorDebugTools, 1000);
    }
});

/**
 * Fix image toolbars and resizing
 * Ensures images are properly set up with toolbars and resize handles
 */
function fixImageToolbarsAndResizing() {
    const editor = document.getElementById('blogEditor');
    if (!editor) return;
    
    // Make sure imageHandler is available
    if (!window.imageHandler) {
        // Create image handler if it doesn't exist
        window.imageHandler = new ImageHandler('#blogEditor');
    }
    
    // Process existing images
    // Use the global processImages function instead of redefining it here
    
    // Fix all nested image containers
    function fixNestedContainers() {
        // Find any nested .image-container elements and fix them
        const nestedContainers = editor.querySelectorAll('.image-container .image-container');
        nestedContainers.forEach(nestedContainer => {
            const parentContainer = nestedContainer.closest('.image-container');
            if (parentContainer !== nestedContainer) {
                // Get the image from the nested container
                const img = nestedContainer.querySelector('img');
                if (img) {
                    // Move the image to the parent container
                    parentContainer.appendChild(img);
                }
                // Remove the nested container
                nestedContainer.remove();
            }
        });
        
        // Remove empty containers
        const emptyContainers = editor.querySelectorAll('.image-container');
        emptyContainers.forEach(container => {
            if (!container.querySelector('img')) {
                container.remove();
            }
        });
        
        // Fix duplicate toolbars
        const containers = editor.querySelectorAll('.image-container');
        containers.forEach(container => {
            const toolbars = container.querySelectorAll('.image-toolbar');
            if (toolbars.length > 1) {
                // Keep only the first toolbar
                for (let i = 1; i < toolbars.length; i++) {
                    toolbars[i].remove();
                }
            }
        });
    }
    
    // Process images immediately
    processImages();
    
    // Fix any nested containers
    fixNestedContainers();
    
    // Set up a MutationObserver to handle dynamically added images
    const observer = new MutationObserver((mutations) => {
        let shouldProcess = false;
        
        mutations.forEach(mutation => {
            if (mutation.type === 'childList') {
                mutation.addedNodes.forEach(node => {
                    if (node.nodeName === 'IMG' || 
                        (node.nodeType === 1 && node.querySelector('img'))) {
                        shouldProcess = true;
                    }
                });
            }
        });
        
        if (shouldProcess) {
            processImages();
            fixNestedContainers();
        }
    });
    
    // Start observing the editor
    observer.observe(editor, { 
        childList: true, 
        subtree: true 
    });
}

/**
 * Fix image editing buttons functionality
 * Corrects issues with image toolbar buttons and ensures they work properly
 */
function fixImageEditingButtons() {
    const editor = document.getElementById('blogEditor');
    if (!editor) return;
    
    // Find all image containers
    const imageContainers = editor.querySelectorAll('.image-container');
    if (!imageContainers.length) return;
    
    imageContainers.forEach(container => {
        // Find toolbar and buttons
        const toolbar = container.querySelector('.image-toolbar');
        if (!toolbar) return;
        
        // Get the image associated with this container
        const image = container.querySelector('img');
        if (!image) return;
        
        // Add proper event listeners to align buttons
        const alignButtons = toolbar.querySelectorAll('.align-btn');
        alignButtons.forEach(button => {
            // Remove any existing event listeners by cloning
            const newButton = button.cloneNode(true);
            button.parentNode.replaceChild(newButton, button);
            
            // Add correct event listener with alignment value
            const alignment = newButton.getAttribute('data-align');
            if (alignment) {
                newButton.addEventListener('click', function(e) {
                    e.preventDefault();
                    e.stopPropagation();
                    alignImage(this, alignment);
                });
            }
        });
        
        // Fix resize handles
        const resizeHandle = container.querySelector('.resize-handle');
        if (resizeHandle) {
            // Ensure resize handle is properly positioned
            resizeHandle.style.display = 'block';
            resizeHandle.style.position = 'absolute';
            resizeHandle.style.bottom = '0';
            resizeHandle.style.right = '0';
            
            // Add proper cursor style
            resizeHandle.style.cursor = 'nwse-resize';
        }
    });
    
    console.log('Image editing buttons fixed');
}

/**
 * Fix styling issues with video toolbars and modals
 */
function fixVideoStyleIssues() {
    // Create style element if it doesn't exist
    let styleEl = document.getElementById('editor-fix-styles');
    if (!styleEl) {
        styleEl = document.createElement('style');
        styleEl.id = 'editor-fix-styles';
        document.head.appendChild(styleEl);
    }
    
    // Add fixes for video styles
    styleEl.textContent += `
        /* Fix video toolbar position */
        .video-container .video-toolbar {
            top: -35px !important;
            box-shadow: 0 0 5px rgba(0,0,0,0.2);
            z-index: 1000;
        }
        
        /* Fix video modal styles */
        #videoInsertModal .modal-dialog {
            max-width: 600px;
        }
        
        #videoInsertModal .form-label {
            display: block;
            margin-bottom: 0.5rem;
        }
        
        #videoInsertModal .input-group {
            z-index: 1; /* Fix z-index issues in input groups */
        }
        
        #videoInsertModal #videoModalPreview {
            margin-top: 10px;
            overflow: hidden;
            border: 1px solid #dee2e6;
        }
        
        /* Fix bootstrap form validation */
        .was-validated .form-control:invalid,
        .form-control.is-invalid {
            background-position: right calc(0.375em + 0.1875rem) center !important;
        }
        
        /* Fix modal backdrop issues */
        .modal-backdrop {
            opacity: 0.5 !important;
        }
        
        /* Add focus styles for accessibility */
        .form-control:focus, 
        .btn:focus {
            box-shadow: 0 0 0 0.25rem rgba(13, 110, 253, 0.25);
            outline: none;
        }
        
        /* Fix video container responsive behavior */
        .video-container {
            position: relative;
            clear: both;
            margin-bottom: 1.5rem;
        }
        
        .video-container .video-width-control input {
            min-width: 50px;
            font-size: 12px;
            padding: 2px 5px;
            height: 24px;
        }
        
        /* Fix YouTube iframe behavior */
        iframe[src*="youtube.com"] {
            border: none;
        }
        
        /* Emergency modal close button */
        #emergency-modal-close {
            opacity: 0.8;
            transition: opacity 0.2s;
        }
        
        #emergency-modal-close:hover {
            opacity: 1;
        }
    `;
    
    // Fix video modal fields
    function fixVideoModalFields() {
        const modal = document.getElementById('videoInsertModal');
        if (!modal) return;
        
        // Fix any missing IDs or labels
        const urlInput = modal.querySelector('input[type="url"], input[type="text"]');
        if (urlInput && !urlInput.id) {
            urlInput.id = 'videoUrl';
            urlInput.name = 'videoUrl';
            
            // Find corresponding label
            const label = modal.querySelector('label:first-child');
            if (label) {
                label.setAttribute('for', 'videoUrl');
            }
        }
        
        // Fix width input
        const widthInput = modal.querySelector('input[type="number"]');
        if (widthInput && !widthInput.id) {
            widthInput.id = 'videoWidth';
            widthInput.name = 'videoWidth';
            
            // Find corresponding label
            const inputGroup = widthInput.closest('.input-group');
            if (inputGroup) {
                const label = inputGroup.previousElementSibling;
                if (label && label.tagName === 'LABEL') {
                    label.setAttribute('for', 'videoWidth');
                }
            }
        }
        
        // Fix buttons
        const buttons = modal.querySelectorAll('.modal-footer button');
        buttons.forEach((button, index) => {
            if (!button.id) {
                const type = index === 0 ? 'cancel' : 'insert';
                button.id = `video${type.charAt(0).toUpperCase() + type.slice(1)}Btn`;
                button.name = button.id;
            }
        });
    }
    
    // Add observer for video modal
    const observer = new MutationObserver(mutations => {
        mutations.forEach(mutation => {
            if (mutation.type === 'childList' && mutation.addedNodes.length) {
                for (const node of mutation.addedNodes) {
                    if (node.id === 'videoInsertModal' || 
                        (node.querySelector && node.querySelector('#videoInsertModal'))) {
                        fixVideoModalFields();
                    }
                }
            }
        });
    });
    
    // Start observing
    observer.observe(document.body, { childList: true, subtree: true });
    
    // Fix existing video modal if present
    fixVideoModalFields();
}

/**
 * Fix Bootstrap Modal issues
 * Addresses issues with modals in the editor
 */
function fixBootstrapModalIssues() {
    // Fix modal close issues
    window.cleanupModalState = function() {
        // Remove any remaining backdrops
        const backdrops = document.querySelectorAll('.modal-backdrop');
        backdrops.forEach(backdrop => backdrop.remove());
        
        // Fix body scrolling
        document.body.classList.remove('modal-open');
        document.body.style.removeProperty('overflow');
        document.body.style.removeProperty('padding-right');
        
        // Hide any visible modals
        const visibleModals = document.querySelectorAll('.modal.show');
        visibleModals.forEach(modal => {
            modal.classList.remove('show');
            modal.style.display = 'none';
        });
    };
    
    // Fix modal close button issues
    const addEmergencyCloseButton = () => {
        // Check if the button already exists
        if (document.getElementById('emergency-modal-close')) return;
        
        // Create emergency close button for modals
        const closeBtn = document.createElement('button');
        closeBtn.id = 'emergency-modal-close';
        closeBtn.textContent = 'X';
        closeBtn.style.position = 'fixed';
        closeBtn.style.bottom = '20px';
        closeBtn.style.left = '20px';
        closeBtn.style.zIndex = '9999';
        closeBtn.style.backgroundColor = 'red';
        closeBtn.style.color = 'white';
        closeBtn.style.borderRadius = '50%';
        closeBtn.style.width = '40px';
        closeBtn.style.height = '40px';
        closeBtn.style.fontSize = '16px';
        closeBtn.style.fontWeight = 'bold';
        closeBtn.style.border = 'none';
        closeBtn.style.display = 'none';
        closeBtn.style.cursor = 'pointer';
        closeBtn.title = 'Emergency Modal Close';
        
        closeBtn.addEventListener('click', window.cleanupModalState);
        
        // When a modal is shown, show the emergency button
        document.addEventListener('shown.bs.modal', () => {
            closeBtn.style.display = 'block';
        });
        
        // When all modals are hidden, hide the emergency button
        document.addEventListener('hidden.bs.modal', () => {
            // Check if any modal is still visible
            const visibleModals = document.querySelectorAll('.modal.show');
            if (visibleModals.length === 0) {
                closeBtn.style.display = 'none';
            }
        });
        
        // Add to document
        document.body.appendChild(closeBtn);
    };
    
    // Add emergency close button
    setTimeout(addEmergencyCloseButton, 1000);
    
    // Add form field fixes to all modals
    function fixModalFormFields() {
        const modals = document.querySelectorAll('.modal');
        
        modals.forEach(modal => {
            // Fix form fields without proper ID or name attributes
            const formFields = modal.querySelectorAll('input, select, textarea');
            formFields.forEach(field => {
                // Ensure field has an ID
                if (!field.id) {
                    const fieldType = field.type || 'input';
                    field.id = `${fieldType}-${Math.random().toString(36).substring(2, 9)}`;
                }
                
                // Ensure field has a name attribute
                if (!field.name) {
                    field.name = field.id;
                }
                
                // Find labels without 'for' attribute
                const parentLabel = field.closest('label');
                if (parentLabel) {
                    // Field is inside a label, no need for 'for' attribute
                    // This is valid HTML5
                } else {
                    // Find preceding label without 'for' attribute
                    const prevSibling = field.previousElementSibling;
                    if (prevSibling && prevSibling.tagName === 'LABEL' && !prevSibling.getAttribute('for')) {
                        prevSibling.setAttribute('for', field.id);
                    }
                }
            });
            
            // Fix buttons without proper ID or name
            const buttons = modal.querySelectorAll('button');
            buttons.forEach(button => {
                // Ensure button has an ID
                if (!button.id) {
                    const btnType = button.type || 'button';
                    const btnText = button.textContent.trim().toLowerCase().replace(/\s+/g, '-');
                    button.id = `${btnType}-${btnText}-${Math.random().toString(36).substring(2, 9)}`;
                }
                
                // Ensure button has a name attribute
                if (!button.name) {
                    button.name = button.id;
                }
            });
        });
    }
    
    // Fix third-party cookie warnings
    function fixCookieIssues() {
        // Add SameSite attribute to cookies if supported by the browser
        if (document.cookie && navigator.cookieEnabled) {
            // Create a function to easily set SameSite cookies
            window.setSameSiteCookie = function(name, value, days) {
                let expires = "";
                if (days) {
                    const date = new Date();
                    date.setTime(date.getTime() + (days * 24 * 60 * 60 * 1000));
                    expires = "; expires=" + date.toUTCString();
                }
                
                // Adding SameSite=Lax attribute
                document.cookie = name + "=" + (value || "") + expires + "; path=/; SameSite=Lax";
            };
            
            // Update existing cookies with SameSite attribute
            const cookieList = document.cookie.split('; ');
            for (const cookie of cookieList) {
                const [name, value] = cookie.split('=');
                window.setSameSiteCookie(name, value, 30); // 30 days expiration
            }
            
            // Update session cookies
            if (window._aspSessionId) {
                window.setSameSiteCookie("ASP.NET_SessionId", window._aspSessionId, null);
            }
        }
    }
    
    // Monitor and fix modals when they appear
    const modalObserver = new MutationObserver((mutations) => {
        mutations.forEach(mutation => {
            if (mutation.type === 'childList' && mutation.addedNodes.length) {
                for (const node of mutation.addedNodes) {
                    if (node.nodeType === 1 && node.classList && node.classList.contains('modal')) {
                        // Fix form fields in the newly added modal
                        fixModalFormFields();
                    }
                }
            }
        });
    });
    
    // Start observing
    modalObserver.observe(document.body, { childList: true, subtree: true });
    
    // Initial fixes
    fixModalFormFields();
    fixCookieIssues();
    
    // Re-apply fixes when document is fully loaded
    window.addEventListener('load', () => {
        fixModalFormFields();
        fixCookieIssues();
    });
}

// Initialize fixes
document.addEventListener('DOMContentLoaded', function() {
    setTimeout(fixBootstrapModalIssues, 500);
});

/**
 * Fix video toolbar controls
 * Ensures all video toolbar inputs have proper IDs and labels
 */
function fixVideoToolbarControls() {
    const editor = document.querySelector('.blog-editor');
    if (!editor) return;
    
    // Find all video containers
    const videoContainers = editor.querySelectorAll('.video-container');
    videoContainers.forEach(container => {
        // Find width control input
        const widthControl = container.querySelector('.video-width-control');
        if (!widthControl) return;
        
        // Fix the input element
        const input = widthControl.querySelector('input');
        if (input && !input.id) {
            // Generate unique ID
            const uniqueId = 'video-width-' + Math.random().toString(36).substring(2, 9);
            input.id = uniqueId;
            input.name = uniqueId;
            
            // Add accessibility attributes
            input.setAttribute('aria-label', 'Video width percentage');
            input.setAttribute('role', 'spinbutton');
            
            // Make sure span has associated label
            const labelSpan = widthControl.querySelector('span:first-child');
            if (labelSpan) {
                labelSpan.setAttribute('id', `${uniqueId}-label`);
                input.setAttribute('aria-labelledby', `${uniqueId}-label`);
            }
        }
        
        // Fix the delete button
        const deleteBtn = container.querySelector('.video-delete-btn');
        if (deleteBtn && !deleteBtn.id) {
            const uniqueId = 'video-delete-' + Math.random().toString(36).substring(2, 9);
            deleteBtn.id = uniqueId;
            deleteBtn.name = uniqueId;
            deleteBtn.setAttribute('aria-label', 'Delete video');
        }
    });
}

// Add periodic monitoring for video toolbar controls
document.addEventListener('DOMContentLoaded', function() {
    // Initial fix
    setTimeout(fixVideoToolbarControls, 1000);
    
    // Set up observer to watch for new video containers
    const observer = new MutationObserver((mutations) => {
        let hasNewVideos = false;
        
        mutations.forEach(mutation => {
            if (mutation.type === 'childList') {
                mutation.addedNodes.forEach(node => {
                    if (node.classList && node.classList.contains('video-container') ||
                        (node.nodeType === 1 && node.querySelector && node.querySelector('.video-container'))) {
                        hasNewVideos = true;
                    }
                });
            }
        });
        
        if (hasNewVideos) {
            fixVideoToolbarControls();
        }
    });
    
    // Start observing
        const editor = document.querySelector('.blog-editor');
        if (editor) {
        observer.observe(editor, { 
            childList: true,
            subtree: true
        });
    }
    
    // Add to the fix list in the initial setup
    const originalSetup = window.editorFixes.processImages;
    window.editorFixes.processImages = function() {
        if (typeof originalSetup === 'function') {
            originalSetup();
        }
        
        // Also fix video toolbars
        fixVideoToolbarControls();
    };
});

/**
 * Fix CORS and cross-site cookie issues
 */
function fixCorsAndCookieIssues() {
    // Fix CORS issues with YouTube embeds
    function fixYoutubeEmbeds() {
        // Find all YouTube iframes
        const iframes = document.querySelectorAll('iframe[src*="youtube.com"]');
        
        iframes.forEach(iframe => {
            const src = iframe.src;
            
            if (src) {
                // Add nocookie domain for YouTube
                if (src.includes('youtube.com') && !src.includes('youtube-nocookie.com')) {
                    const newSrc = src.replace('youtube.com', 'youtube-nocookie.com');
                    iframe.src = newSrc;
                }
                
                // Ensure iframe has title for accessibility
                if (!iframe.hasAttribute('title')) {
                    iframe.setAttribute('title', 'YouTube video player');
                }
                
                // Add loading="lazy" for performance
                if (!iframe.hasAttribute('loading')) {
                    iframe.setAttribute('loading', 'lazy');
                }
            }
        });
    }
    
    // Fix cross-site cookie warnings
    function fixCookieWarnings() {
        // Use SameSite=Lax for cookies in document
        if (document.cookie && navigator.cookieEnabled) {
            try {
                // Save current cookies
                const cookieList = document.cookie.split('; ');
                
                // Create a function to set cookies with SameSite attribute
                const setSameSiteCookie = (name, value, days) => {
                    let expires = "";
                    if (days) {
                        const date = new Date();
                        date.setTime(date.getTime() + (days * 24 * 60 * 60 * 1000));
                        expires = "; expires=" + date.toUTCString();
                    }
                    
                    // Adding SameSite=Lax attribute and Secure flag
                    document.cookie = name + "=" + (value || "") + expires + "; path=/; SameSite=Lax; Secure";
                };
                
                // Apply to all cookies
                for (const cookie of cookieList) {
                    const [name, value] = cookie.split('=');
                    if (name && value) {
                        setSameSiteCookie(name, value, 30); // 30 days expiration
                    }
                }
            } catch (error) {
                console.error('Error updating cookie attributes:', error);
            }
        }
    }
    
    // Add a meta tag to inform browsers about cookie usage
    function addCookieConsentMeta() {
        if (!document.querySelector('meta[name="cookie-consent"]')) {
            const meta = document.createElement('meta');
            meta.name = 'cookie-consent';
            meta.content = 'Same-Site';
            document.head.appendChild(meta);
        }
    }
    
    // Apply CORS related fixes
    function addCorsHeaders() {
        // Add a meta tag to control CORS policy
        if (!document.querySelector('meta[name="referrer"]')) {
            const meta = document.createElement('meta');
            meta.name = 'referrer';
            meta.content = 'same-origin';
            document.head.appendChild(meta);
        }
    }
    
    // Fix YouTube video inserts to use nocookie domain
    function patchVideoInsertion() {
        const editorInstance = window.editor;
        if (!editorInstance || !editorInstance.insertVideo) return;
        
        // Save reference to the original method
        const originalExtractYouTubeId = editorInstance.extractYouTubeId;
        
        // Patch the extractYouTubeId method to support nocookie domain
        editorInstance.extractYouTubeId = function(url) {
            if (!url) return null;
            
            // First convert youtube-nocookie.com to youtube.com for ID extraction
            if (url.includes('youtube-nocookie.com')) {
                url = url.replace('youtube-nocookie.com', 'youtube.com');
            }
            
            // Then call the original method
            return originalExtractYouTubeId.call(this, url);
        };
        
        // Add a hook to modify iframe creation in insertVideo
        const originalInsertVideo = editorInstance.insertVideo;
        
        editorInstance.insertVideo = function(url = null) {
            // Call original method
            const result = originalInsertVideo.call(this, url);
            
            // Find any newly created YouTube iframes
            setTimeout(() => {
                fixYoutubeEmbeds();
            }, 100);
            
            return result;
        };
    }
    
    // Initial fixes
    fixYoutubeEmbeds();
    fixCookieWarnings();
    addCookieConsentMeta();
    addCorsHeaders();
    patchVideoInsertion();
    
    // Set up observer for future iframe additions
    const observer = new MutationObserver((mutations) => {
        let hasNewIframes = false;
        
        mutations.forEach(mutation => {
            if (mutation.type === 'childList') {
                mutation.addedNodes.forEach(node => {
                    if (node.nodeName === 'IFRAME' || 
                        (node.nodeType === 1 && node.querySelector && node.querySelector('iframe'))) {
                        hasNewIframes = true;
                    }
                });
            }
        });
        
        if (hasNewIframes) {
            fixYoutubeEmbeds();
        }
    });
    
    // Start observing
    observer.observe(document.body, { 
                childList: true, 
        subtree: true
    });
    
    // Run fixes again when page is fully loaded
    window.addEventListener('load', () => {
        fixYoutubeEmbeds();
        fixCookieWarnings();
    });
}

// Initialize CORS and cookie fixes
document.addEventListener('DOMContentLoaded', function() {
    setTimeout(fixCorsAndCookieIssues, 300);
    
    // Add to the initialization in the main DOMContentLoaded handler
            setTimeout(() => {
        // Apply existing fixes
        fixImageUpload();
        fixVideoWidthValidation();
        fixFormulaCursorPosition();
        fixLinkInsertion();
        removeDarkMode();
        fixDuplicateVideoOnSave();
        fixEditPageContentLoading();
        fixFormulasSaveIssue();
        fixVideoButtonIssue();
        fixImageToolbarsAndResizing();
        fixBootstrapModalIssues();
        fixVideoStyleIssues();
        fixVideoToolbarControls();
        fixCorsAndCookieIssues();
        
        console.log('All editor fixes applied successfully, including CORS and cookie protections');
        
        // Update the global editorFixes object
        if (window.editorFixes) {
            window.editorFixes.fixCorsAndCookieIssues = fixCorsAndCookieIssues;
        }
    }, 500);
});

/**
 * Fix favicon 404 errors
 */
function fixFaviconError() {
    // Check if favicon exists
    const favicon = document.querySelector('link[rel="icon"], link[rel="shortcut icon"]');
    
    if (!favicon) {
        // Create a simple favicon if none exists
        const newFavicon = document.createElement('link');
        newFavicon.rel = 'icon';
        newFavicon.type = 'image/png';
        newFavicon.href = 'data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAACAAAAAgCAMAAABEpIrGAAAABGdBTUEAALGPC/xhBQAAAAFzUkdCAK7OHOkAAAAJcEhZcwAACxMAAAsTAQCanBgAAABjUExURUdwTAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAF8yqVEAAAAgdFJOUwD1CsX5Ko7iFWdZ3jMJhhHRsdp+LSB3BpwEQ1E4azdxR3+hxgAAALlJREFUOMvNkVcOwzAMBKXuvffm/S/JD8KyI1EG9puxNMPhLkAgJMf1AzJr+TY3o/XN7cif/BS/TXbOO3YJVjsHFJnrHBHk7jkgyFwV5oFBxFATHCUYyqCaIDsEObFvMJImBTkKij0gK8I8QJGdreDYn8DD/c4IvEp1CJ6wZwWuKVoEGlM0ZhWZYYYGTXaYYNMEmyZYnv30WCAoRkJQjLhWC3TfjwpEuQCJKBfQXUBLqwXa93+CL9syGyvyIrPKAAAAAElFTkSuQmCC';
        document.head.appendChild(newFavicon);
    }
}

// Execute favicon fix early
document.addEventListener('DOMContentLoaded', function() {
    // Fix favicon immediately
    fixFaviconError();
}); 