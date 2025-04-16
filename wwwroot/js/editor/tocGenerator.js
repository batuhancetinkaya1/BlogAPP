/**
 * TOC Generator for BlogEditor
 * Creates and manages table of contents for blog posts
 */

class TOCGenerator {
    constructor(options = {}) {
        this.config = {
            editorSelector: options.editorSelector || '.blog-editor',
            headingSelectors: options.headingSelectors || 'h1, h2, h3, h4, h5, h6',
            tocTitle: options.tocTitle || 'Table of Contents',
            insertPosition: options.insertPosition || 'start',
            minHeadings: options.minHeadings || 3, // Minimum headings to show TOC
            maxLevel: options.maxLevel || 4 // Maximum heading level to include (h1-h4)
        };
        
        this.editor = document.querySelector(this.config.editorSelector);
        
        if (!this.editor) {
            console.error('TOC Generator: Editor element not found');
            return;
        }
        
        // Initialize
        this.init();
    }
    
    /**
     * Initialize TOC generator
     */
    init() {
        // Create preview button for TOC
        this.createPreviewButton();
        
        // Monitor content changes
        this.monitorContentChanges();
    }
    
    /**
     * Create TOC preview button
     */
    createPreviewButton() {
        const toolbar = document.querySelector('#editorToolbar');
        if (!toolbar) return;
        
        // Check if button already exists
        if (toolbar.querySelector('.toc-button')) return;
        
        // Create button
        const tocButton = document.createElement('button');
        tocButton.type = 'button';
        tocButton.className = 'btn btn-light btn-sm toc-button';
        tocButton.innerHTML = '<i class="bi bi-list-nested"></i>';
        tocButton.title = 'Preview Table of Contents';
        
        // Add click handler
        tocButton.addEventListener('click', () => {
            this.previewTOC();
        });
        
        // Add to toolbar
        toolbar.appendChild(tocButton);
    }
    
    /**
     * Monitor content changes to update headings
     */
    monitorContentChanges() {
        // Initialize MutationObserver to watch for heading changes
        const observer = new MutationObserver(() => {
            this.processHeadings();
        });
        
        // Start observing
        observer.observe(this.editor, {
            childList: true,
            subtree: true,
            characterData: true
        });
        
        // Initial processing
        this.processHeadings();
    }
    
    /**
     * Process headings in the editor
     */
    processHeadings() {
        const headings = this.editor.querySelectorAll(this.config.headingSelectors);
        
        // Add IDs to headings if they don't have one
        headings.forEach((heading, index) => {
            if (!heading.id) {
                const headingText = heading.textContent.trim();
                const headingId = this.generateHeadingId(headingText, index);
                heading.id = headingId;
            }
        });
    }
    
    /**
     * Generate heading ID from text
     */
    generateHeadingId(text, index) {
        // Sanitize text for use as ID
        let id = text.toLowerCase()
            .replace(/[^\w\s-]/g, '') // Remove special characters
            .replace(/\s+/g, '-')     // Replace spaces with hyphens
            .replace(/-+/g, '-');     // Replace multiple hyphens with single hyphen
        
        // Add index to ensure uniqueness
        return `heading-${id}-${index}`;
    }
    
    /**
     * Generate TOC HTML
     */
    generateTOC() {
        const headings = this.editor.querySelectorAll(this.config.headingSelectors);
        
        // If not enough headings, don't generate TOC
        if (headings.length < this.config.minHeadings) {
            return '';
        }
        
        let tocHtml = `<div class="blog-toc">
            <h4>${this.config.tocTitle}</h4>
            <ul class="blog-toc-list">`;
        
        headings.forEach(heading => {
            // Get heading level (1-6)
            const level = parseInt(heading.tagName.substring(1));
            
            // Skip if level exceeds maxLevel
            if (level > this.config.maxLevel) return;
            
            const headingText = heading.textContent.trim();
            const headingId = heading.id;
            
            tocHtml += `<li class="toc-level-${level}">
                <a href="#${headingId}">${headingText}</a>
            </li>`;
        });
        
        tocHtml += `</ul></div>`;
        
        return tocHtml;
    }
    
    /**
     * Preview TOC in the editor
     */
    previewTOC() {
        const headings = this.editor.querySelectorAll(this.config.headingSelectors);
        
        if (headings.length < this.config.minHeadings) {
            alert(`You need at least ${this.config.minHeadings} headings to generate a table of contents.`);
            return;
        }
        
        // Generate TOC HTML
        const tocHtml = this.generateTOC();
        
        // Preview modal
        const modalId = 'tocPreviewModal';
        
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
                <div class="modal-dialog">
                    <div class="modal-content">
                        <div class="modal-header">
                            <h5 class="modal-title" id="${modalId}Label">Table of Contents Preview</h5>
                            <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>
                        </div>
                        <div class="modal-body" id="${modalId}Body">
                        </div>
                        <div class="modal-footer">
                            <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">Close</button>
                            <button type="button" class="btn btn-primary" id="insertTocBtn">Insert TOC</button>
                        </div>
                    </div>
                </div>
            `;
            
            document.body.appendChild(modal);
        }
        
        // Update modal content
        const modalBody = document.getElementById(`${modalId}Body`);
        modalBody.innerHTML = tocHtml;
        
        // Initialize modal
        const bsModal = new bootstrap.Modal(modal);
        bsModal.show();
        
        // Insert button handler
        document.getElementById('insertTocBtn').addEventListener('click', () => {
            this.insertTOC();
            bsModal.hide();
        });
    }
    
    /**
     * Insert TOC into the editor
     */
    insertTOC() {
        const tocHtml = this.generateTOC();
        
        // Check if a TOC already exists
        const existingToc = this.editor.querySelector('.blog-toc');
        if (existingToc) {
            // Replace existing TOC
            existingToc.outerHTML = tocHtml;
        } else {
            // Insert new TOC at specified position
            if (this.config.insertPosition === 'start') {
                // Insert at beginning of editor
                this.editor.innerHTML = tocHtml + this.editor.innerHTML;
            } else if (this.config.insertPosition === 'cursor') {
                // Insert at cursor position
                document.execCommand('insertHTML', false, tocHtml);
            }
        }
        
        // Notify of content change
        document.dispatchEvent(new CustomEvent('blogEditorContentChange'));
        
        // Force update of hidden input
        const contentInput = document.querySelector('#Content');
        if (contentInput) {
            contentInput.value = this.editor.innerHTML;
        }
    }
    
    /**
     * Get TOC HTML for saving with the post
     */
    getTOC() {
        return this.generateTOC();
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
        window.tocGenerator = new TOCGenerator();
        
        // Add TOC to form submission
        const form = document.getElementById('blogPostForm');
        if (form) {
            form.addEventListener('submit', (e) => {
                // Only insert TOC automatically if there are enough headings
                const editor = document.querySelector('.blog-editor');
                const headings = editor.querySelectorAll('h1, h2, h3, h4, h5, h6');
                
                if (headings.length >= 3) {
                    // Check if TOC already exists
                    const existingToc = editor.querySelector('.blog-toc');
                    if (!existingToc) {
                        // Insert TOC at the beginning
                        window.tocGenerator.insertTOC();
                    }
                }
            });
        }
    }, 1000);
}); 