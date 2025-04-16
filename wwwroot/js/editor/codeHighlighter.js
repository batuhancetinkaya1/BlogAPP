/**
 * Code Highlighter for BlogEditor
 * Implements syntax highlighting for code blocks
 */

class CodeHighlighter {
    constructor(options = {}) {
        this.config = {
            editorSelector: options.editorSelector || '.blog-editor',
            languages: options.languages || [
                'html', 'css', 'javascript', 'typescript', 'csharp', 'c', 'cpp', 
                'java', 'python', 'ruby', 'php', 'bash', 'sql', 'json', 'markdown',
                'yaml', 'plaintext'
            ]
        };
        
        // Initialize
        this.init();
    }
    
    /**
     * Initialize code highlighter
     */
    init() {
        // Load Prism.js if not already loaded
        this.loadPrism();
        
        // Initialize observer for code blocks
        this.initObserver();
        
        // Set up event handlers
        this.setupEventHandlers();
    }
    
    /**
     * Load Prism.js for syntax highlighting
     */
    loadPrism() {
        if (typeof Prism !== 'undefined') {
            // Already loaded
            return;
        }
        
        // Load Prism CSS
        const linkElement = document.createElement('link');
        linkElement.rel = 'stylesheet';
        linkElement.href = 'https://cdn.jsdelivr.net/npm/prismjs@1.29.0/themes/prism.min.css';
        document.head.appendChild(linkElement);
        
        // Main Prism.js
        const script = document.createElement('script');
        script.src = 'https://cdn.jsdelivr.net/npm/prismjs@1.29.0/prism.min.js';
        document.head.appendChild(script);
        
        // Load additional languages
        this.config.languages.forEach(lang => {
            if (lang === 'plaintext' || lang === 'html' || lang === 'javascript' || lang === 'css') {
                return; // These are included in the core
            }
            
            const langScript = document.createElement('script');
            langScript.src = `https://cdn.jsdelivr.net/npm/prismjs@1.29.0/components/prism-${lang}.min.js`;
            document.head.appendChild(langScript);
        });
    }
    
    /**
     * Initialize observer for code blocks
     */
    initObserver() {
        // Create mutation observer to detect when code blocks are added
        const observer = new MutationObserver((mutations) => {
            let shouldHighlight = false;
            
            mutations.forEach((mutation) => {
                if (mutation.type === 'childList' && mutation.addedNodes.length > 0) {
                    mutation.addedNodes.forEach((node) => {
                        // Check if node contains code blocks
                        if (node.nodeType === Node.ELEMENT_NODE) {
                            if (node.tagName === 'PRE' || node.tagName === 'CODE' || 
                                node.querySelector('pre') || node.querySelector('code')) {
                                shouldHighlight = true;
                            }
                        }
                    });
                }
            });
            
            if (shouldHighlight) {
                this.highlightCodeBlocks();
            }
        });
        
        // Start observing the editor
        const editor = document.querySelector(this.config.editorSelector);
        if (editor) {
            observer.observe(editor, { childList: true, subtree: true });
        }
    }
    
    /**
     * Set up event handlers for editor
     */
    setupEventHandlers() {
        document.addEventListener('blogEditorContentChange', () => {
            this.highlightCodeBlocks();
        });
        
        // Listen for code block insertion requests
        document.addEventListener('blogEditorCodeInsert', (e) => {
            const language = e.detail?.language || 'javascript';
            this.insertCodeBlock(language);
        });
    }
    
    /**
     * Highlight all code blocks in the editor
     */
    highlightCodeBlocks() {
        // Wait for Prism to load
        if (typeof Prism === 'undefined') {
            setTimeout(() => this.highlightCodeBlocks(), 500);
            return;
        }
        
        const editor = document.querySelector(this.config.editorSelector);
        if (!editor) return;
        
        // Find all code blocks
        const codeBlocks = editor.querySelectorAll('pre code');
        
        codeBlocks.forEach(codeBlock => {
            // Skip if already highlighted by Prism
            if (codeBlock.classList.contains('prism-highlighted')) return;
            
            // Get language class
            let language = 'plaintext';
            codeBlock.classList.forEach(className => {
                if (className.startsWith('language-')) {
                    language = className.replace('language-', '');
                }
            });
            
            // Ensure the block has the language class
            if (!codeBlock.classList.contains(`language-${language}`)) {
                codeBlock.classList.add(`language-${language}`);
            }
            
            // Highlight code
            try {
                Prism.highlightElement(codeBlock);
                
                // Mark as highlighted
                codeBlock.classList.add('prism-highlighted');
                
                // Add edit control if not already added
                this.addCodeBlockEditControls(codeBlock.parentElement);
            } catch (error) {
                console.error('Error highlighting code block:', error);
            }
        });
    }
    
    /**
     * Add edit controls to code blocks
     */
    addCodeBlockEditControls(preElement) {
        if (!preElement || preElement.tagName !== 'PRE') return;
        
        // Check if controls already exist
        if (preElement.querySelector('.code-block-controls')) return;
        
        // Make positioning work
        preElement.style.position = 'relative';
        
        // Create controls container
        const controls = document.createElement('div');
        controls.className = 'code-block-controls';
        controls.style.position = 'absolute';
        controls.style.top = '5px';
        controls.style.right = '5px';
        controls.style.display = 'flex';
        controls.style.gap = '5px';
        controls.style.opacity = '0';
        controls.style.transition = 'opacity 0.2s ease-in-out';
        
        // Get language
        const codeElement = preElement.querySelector('code');
        let language = 'plaintext';
        
        if (codeElement) {
            codeElement.classList.forEach(className => {
                if (className.startsWith('language-')) {
                    language = className.replace('language-', '');
                }
            });
        }
        
        // Create language selector
        const languageSelector = document.createElement('select');
        languageSelector.className = 'form-select form-select-sm';
        languageSelector.style.width = 'auto';
        languageSelector.title = 'Change language';
        
        // Add language options
        this.config.languages.forEach(lang => {
            const option = document.createElement('option');
            option.value = lang;
            option.textContent = lang;
            option.selected = lang === language;
            languageSelector.appendChild(option);
        });
        
        // Language change handler
        languageSelector.addEventListener('change', () => {
            const newLanguage = languageSelector.value;
            if (codeElement) {
                // Remove existing language classes
                codeElement.className = '';
                codeElement.classList.add(`language-${newLanguage}`);
                
                // Highlight with new language
                Prism.highlightElement(codeElement);
                
                // Notify of content change
                document.dispatchEvent(new CustomEvent('blogEditorContentChange'));
            }
        });
        
        // Create edit button
        const editButton = document.createElement('button');
        editButton.className = 'btn btn-sm btn-light';
        editButton.innerHTML = '<i class="bi bi-pencil"></i>';
        editButton.title = 'Edit code';
        
        // Edit button handler
        editButton.addEventListener('click', () => {
            if (codeElement) {
                this.editCodeBlock(codeElement);
            }
        });
        
        // Create copy button
        const copyButton = document.createElement('button');
        copyButton.className = 'btn btn-sm btn-light';
        copyButton.innerHTML = '<i class="bi bi-clipboard"></i>';
        copyButton.title = 'Copy code';
        
        // Copy button handler
        copyButton.addEventListener('click', () => {
            if (codeElement) {
                const text = codeElement.textContent;
                navigator.clipboard.writeText(text)
                    .then(() => {
                        // Show copied indicator
                        copyButton.innerHTML = '<i class="bi bi-check"></i>';
                        setTimeout(() => {
                            copyButton.innerHTML = '<i class="bi bi-clipboard"></i>';
                        }, 2000);
                    })
                    .catch(err => {
                        console.error('Error copying code to clipboard:', err);
                    });
            }
        });
        
        // Create delete button
        const deleteButton = document.createElement('button');
        deleteButton.className = 'btn btn-sm btn-light';
        deleteButton.innerHTML = '<i class="bi bi-trash"></i>';
        deleteButton.title = 'Delete code block';
        deleteButton.style.color = '#dc3545';
        
        // Delete button handler
        deleteButton.addEventListener('click', () => {
            if (confirm('Are you sure you want to delete this code block?')) {
                preElement.remove();
                
                // Notify of content change
                document.dispatchEvent(new CustomEvent('blogEditorContentChange'));
            }
        });
        
        // Add controls
        controls.appendChild(languageSelector);
        controls.appendChild(editButton);
        controls.appendChild(copyButton);
        controls.appendChild(deleteButton);
        
        // Add to pre element
        preElement.appendChild(controls);
        
        // Show controls on hover
        preElement.addEventListener('mouseenter', () => {
            controls.style.opacity = '1';
        });
        
        preElement.addEventListener('mouseleave', () => {
            controls.style.opacity = '0';
        });
    }
    
    /**
     * Edit a code block
     */
    editCodeBlock(codeElement) {
        if (!codeElement) return;
        
        // Get current content and language
        const content = codeElement.textContent;
        let language = 'plaintext';
        
        codeElement.classList.forEach(className => {
            if (className.startsWith('language-')) {
                language = className.replace('language-', '');
            }
        });
        
        // Create modal for editing
        const modalId = 'codeEditModal';
        
        // Check if modal already exists
        let modal = document.getElementById(modalId);
        
        if (!modal) {
            // Create modal
            modal = document.createElement('div');
            modal.className = 'modal fade';
            modal.id = modalId;
            modal.tabIndex = -1;
            modal.setAttribute('aria-labelledby', `${modalId}Label`);
            modal.setAttribute('aria-hidden', 'true');
            
            modal.innerHTML = `
                <div class="modal-dialog modal-lg">
                    <div class="modal-content">
                        <div class="modal-header">
                            <h5 class="modal-title" id="${modalId}Label">Edit Code</h5>
                            <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>
                        </div>
                        <div class="modal-body">
                            <div class="mb-3">
                                <label for="codeLanguage" class="form-label">Language</label>
                                <select id="codeLanguage" class="form-select"></select>
                            </div>
                            <div class="mb-3">
                                <label for="codeContent" class="form-label">Code</label>
                                <textarea id="codeContent" class="form-control font-monospace" rows="10" style="tab-size: 4;"></textarea>
                            </div>
                        </div>
                        <div class="modal-footer">
                            <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">Cancel</button>
                            <button type="button" class="btn btn-primary" id="saveCodeBtn">Save Changes</button>
                        </div>
                    </div>
                </div>
            `;
            
            document.body.appendChild(modal);
            
            // Add language options
            const languageSelect = modal.querySelector('#codeLanguage');
            this.config.languages.forEach(lang => {
                const option = document.createElement('option');
                option.value = lang;
                option.textContent = lang;
                languageSelect.appendChild(option);
            });
        }
        
        // Set current values
        const languageSelect = modal.querySelector('#codeLanguage');
        const codeTextarea = modal.querySelector('#codeContent');
        
        languageSelect.value = language;
        codeTextarea.value = content;
        
        // Add tab key support for indentation
        codeTextarea.addEventListener('keydown', (e) => {
            if (e.key === 'Tab') {
                e.preventDefault();
                
                const start = codeTextarea.selectionStart;
                const end = codeTextarea.selectionEnd;
                
                // Set textarea value to: text before caret + tab + text after caret
                codeTextarea.value = codeTextarea.value.substring(0, start) + 
                    '    ' + codeTextarea.value.substring(end);
                
                // Put caret at right position again
                codeTextarea.selectionStart = codeTextarea.selectionEnd = start + 4;
            }
        });
        
        // Initialize modal
        const bsModal = new bootstrap.Modal(modal);
        bsModal.show();
        
        // Save button handler
        document.getElementById('saveCodeBtn').addEventListener('click', () => {
            const newLanguage = languageSelect.value;
            const newContent = codeTextarea.value;
            
            // Update code element
            codeElement.textContent = newContent;
            
            // Update language class
            codeElement.className = '';
            codeElement.classList.add(`language-${newLanguage}`);
            
            // Highlight with new content and language
            Prism.highlightElement(codeElement);
            
            // Notify of content change
            document.dispatchEvent(new CustomEvent('blogEditorContentChange'));
            
            // Close modal
            bsModal.hide();
        });
    }
    
    /**
     * Insert a new code block at cursor position
     */
    insertCodeBlock(language = 'javascript') {
        const editor = document.querySelector(this.config.editorSelector);
        if (!editor) return;
        
        // Create code block HTML
        const codeBlockHtml = `<pre><code class="language-${language}">// Your code here
function example() {
    console.log('Hello, world!');
}
</code></pre>`;
        
        // Insert at cursor position
        document.execCommand('insertHTML', false, codeBlockHtml);
        
        // Highlight new code block
        setTimeout(() => {
            this.highlightCodeBlocks();
            
            // Notify of content change
            document.dispatchEvent(new CustomEvent('blogEditorContentChange'));
        }, 100);
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
        window.codeHighlighter = new CodeHighlighter();
    }, 1000);
}); 