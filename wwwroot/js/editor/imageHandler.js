/**
 * Simple Image Handler for BlogEditor
 * Provides basic image handling capabilities
 */

class ImageHandler {
    constructor(editorSelector = '.blog-editor') {
        this.editor = document.querySelector(editorSelector);
        if (!this.editor) {
            console.error('Editor element not found for ImageHandler');
            return;
        }
        
        this.init();
    }
    
    /**
     * Initialize image handler
     */
    init() {
        // Add styles for image handling
        this.addStyles();
        
        // Clean up any existing images
        this.cleanupExistingImages();
        
        // Make existing images floating properly
        this.processImages();
        
        // Set up mutation observer to handle new images
        this.setupMutationObserver();
        
        // Add paste handler for images
        this.setupPasteHandler();
    }
    
    /**
     * Add necessary styles for image handling
     */
    addStyles() {
        if (document.getElementById('image-handler-styles')) return;
        
        const styleElement = document.createElement('style');
        styleElement.id = 'image-handler-styles';
        styleElement.textContent = `
            .blog-editor img {
                max-width: 100%;
                height: auto;
                margin: 10px 0;
                clear: both;
                opacity: 1 !important;
            }
            
            .blog-editor .image-container {
                margin: 10px 0;
                position: relative;
                display: block;
                clear: both;
                width: 100%;
                max-width: 100%;
            }
            
            .blog-editor .image-container.align-left {
                float: left;
                margin-right: 20px;
                margin-bottom: 10px;
                width: auto;
                max-width: 50%;
            }
            
            .blog-editor .image-container.align-right {
                float: right;
                margin-left: 20px;
                margin-bottom: 10px;
                width: auto;
                max-width: 50%;
            }
            
            .blog-editor .image-container.align-center {
                display: block;
                margin-left: auto;
                margin-right: auto;
                text-align: center;
                width: 100%;
            }
            
            .blog-editor .image-container img {
                margin: 0;
                max-width: 100%;
                display: block;
                opacity: 1 !important;
            }
            
            .blog-editor .image-toolbar {
                position: absolute;
                top: -30px;
                left: 0;
                right: 0;
                height: 30px;
                background-color: #f8f9fa;
                border-radius: 3px;
                display: none;
                align-items: center;
                justify-content: center;
                gap: 5px;
                box-shadow: 0 1px 3px rgba(0,0,0,0.1);
                z-index: 99;
            }
            
            .blog-editor .image-container:hover .image-toolbar {
                display: flex;
            }
            
            .blog-editor .image-toolbar button {
                background: none;
                border: none;
                padding: 3px 6px;
                cursor: pointer;
                font-size: 14px;
                color: #495057;
            }
            
            .blog-editor .image-toolbar button:hover {
                background-color: rgba(0,0,0,0.1);
                border-radius: 3px;
            }
            
            .blog-editor .image-toolbar button.active {
                background-color: rgba(0,0,0,0.1);
                border-radius: 3px;
            }
            
            .blog-editor p {
                clear: both;
            }
            
            .blog-editor br {
                clear: both;
            }
            
            .blog-editor .clearfix::after {
                content: "";
                display: table;
                clear: both;
            }
            
            /* Hide old resize handles */
            .blog-editor .resize-handle {
                display: none !important;
            }
            
            /* Hide old image toolbars outside containers */
            .blog-editor > .image-toolbar {
                display: none !important;
            }
            
            /* Hide old image-wrapper that might still be present */
            .blog-editor .image-wrapper {
                border: none !important;
                position: relative !important;
                display: block !important;
                margin: 10px 0 !important;
                padding: 0 !important;
            }
            
            /* Hide old dimensions display */
            .blog-editor .image-dimensions {
                display: none !important;
            }
        `;
        
        document.head.appendChild(styleElement);
    }
    
    /**
     * Clean up any existing images with duplicate or old structures
     */
    cleanupExistingImages() {
        try {
            if (!this.editor) return;
            
            // Remove any stray resize handles
            const resizeHandles = this.editor.querySelectorAll('.resize-handle');
            resizeHandles.forEach(handle => handle.remove());
            
            // Remove any stray toolbars
            const toolbars = this.editor.querySelectorAll(':not(.image-container) > .image-toolbar');
            toolbars.forEach(toolbar => toolbar.remove());
            
            // Remove any dimensions displays
            const dimensions = this.editor.querySelectorAll('.image-dimensions');
            dimensions.forEach(dim => dim.remove());
            
            // Fix any images in image-wrapper but not in image-container
            const imageWrappers = this.editor.querySelectorAll('.image-wrapper:not(.image-container)');
            imageWrappers.forEach(wrapper => {
                const img = wrapper.querySelector('img');
                if (img) {
                    wrapper.parentNode.insertBefore(img, wrapper);
                }
                wrapper.remove();
            });
        } catch (error) {
            console.error('Error cleaning up existing images:', error);
        }
    }
    
    /**
     * Set up mutation observer to handle new images
     */
    setupMutationObserver() {
        const observer = new MutationObserver(mutations => {
            let shouldProcess = false;
            
            // Check if any mutation added images
            mutations.forEach(mutation => {
                if (mutation.type === 'childList') {
                    mutation.addedNodes.forEach(node => {
                        if (node.nodeName === 'IMG' || (node.nodeType === 1 && node.querySelector('img'))) {
                            shouldProcess = true;
                        }
                    });
                }
            });
            
            if (shouldProcess) {
                // Clean up first to prevent duplicates
                this.cleanupExistingImages();
                // Then process images
                this.processImages();
            }
        });
        
        // Start observing the editor
        observer.observe(this.editor, { 
            childList: true, 
            subtree: true 
        });
    }
    
    /**
     * Set up paste handler for images
     */
    setupPasteHandler() {
        this.editor.addEventListener('paste', e => {
            // Check if there are image files in the clipboard
            const items = (e.clipboardData || e.originalEvent.clipboardData).items;
            
            for (const item of items) {
                if (item.type.indexOf('image') === 0) {
                    e.preventDefault();
                    
                    // Get the file
                    const blob = item.getAsFile();
                    
                    // Upload the image
                    this.uploadImage(blob);
                    return;
                }
            }
        });
    }
    
    /**
     * Upload a pasted image
     */
    uploadImage(blob) {
        // Create a placeholder while uploading
        const placeholder = document.createElement('img');
        placeholder.src = URL.createObjectURL(blob);
        placeholder.alt = 'Uploading...';
        placeholder.style.maxWidth = '100%';
        placeholder.style.opacity = '0.5';
        placeholder.setAttribute('data-uploading', 'true');
        
        // Insert placeholder at cursor position
        const selection = window.getSelection();
        if (selection.rangeCount > 0) {
            const range = selection.getRangeAt(0);
            range.deleteContents();
            range.insertNode(placeholder);
            
            // Move cursor after the placeholder
            range.setStartAfter(placeholder);
            range.setEndAfter(placeholder);
            selection.removeAllRanges();
            selection.addRange(range);
        } else {
            this.editor.appendChild(placeholder);
        }
        
        // Create form data for upload
        const formData = new FormData();
        formData.append('upload', blob, 'pasted-image.png');
        
        // Get CSRF token
        const csrfToken = document.querySelector('input[name="__RequestVerificationToken"]')?.value || 
                        document.querySelector('meta[name="csrf-token"]')?.getAttribute('content');
        
        // Upload the image
        fetch('/Posts/UploadImage', {
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
            if (data.success && data.url) {
                // Create image container
                const container = document.createElement('div');
                container.className = 'image-container';
                
                // Create the image
                const img = document.createElement('img');
                img.src = data.url;
                img.alt = 'Uploaded image';
                
                // Create toolbar
                const toolbar = this.createImageToolbar();
                
                // Add image and toolbar to container
                container.appendChild(toolbar);
                container.appendChild(img);
                
                // Replace placeholder with the container
                placeholder.replaceWith(container);
                
                // Add line break for text to continue below
                const br = document.createElement('br');
                br.className = 'clearfix';
                container.after(br);
                
                // Move cursor after the line break
                const selection = window.getSelection();
                const range = document.createRange();
                range.setStartAfter(br);
                range.setEndAfter(br);
                selection.removeAllRanges();
                selection.addRange(range);
                
                // Trigger content change event
                document.dispatchEvent(new CustomEvent('blogEditorContentChange'));
            } else {
                console.error('Image upload failed:', data.message || 'Unknown error');
                alert('Image upload failed: ' + (data.message || 'Unknown error'));
                
                // Remove placeholder on error
                placeholder.remove();
            }
        })
        .catch(error => {
            console.error('Error uploading image:', error);
            alert('Error uploading image. Please try again.');
            
            // Remove placeholder on error
            placeholder.remove();
        });
    }
    
    /**
     * Create toolbar for image alignment options
     */
    createImageToolbar() {
        const toolbar = document.createElement('div');
        toolbar.className = 'image-toolbar';
        
        // Left align button
        const leftBtn = document.createElement('button');
        leftBtn.type = 'button';
        leftBtn.title = 'Sola hizala';
        leftBtn.innerHTML = '<i class="bi bi-align-start"></i>';
        leftBtn.addEventListener('click', () => this.alignImage(leftBtn, 'left'));
        
        // Center align button
        const centerBtn = document.createElement('button');
        centerBtn.type = 'button';
        centerBtn.title = 'Ortaya hizala';
        centerBtn.innerHTML = '<i class="bi bi-align-center"></i>';
        centerBtn.addEventListener('click', () => this.alignImage(centerBtn, 'center'));
        
        // Right align button
        const rightBtn = document.createElement('button');
        rightBtn.type = 'button';
        rightBtn.title = 'Sağa hizala';
        rightBtn.innerHTML = '<i class="bi bi-align-end"></i>';
        rightBtn.addEventListener('click', () => this.alignImage(rightBtn, 'right'));
        
        // Remove button
        const removeBtn = document.createElement('button');
        removeBtn.type = 'button';
        removeBtn.title = 'Resmi sil';
        removeBtn.innerHTML = '<i class="bi bi-trash"></i>';
        removeBtn.addEventListener('click', e => {
            const container = e.target.closest('.image-container');
            if (container) {
                container.remove();
                // Trigger content change event
                document.dispatchEvent(new CustomEvent('blogEditorContentChange'));
            }
        });
        
        // Add buttons to toolbar
        toolbar.appendChild(leftBtn);
        toolbar.appendChild(centerBtn);
        toolbar.appendChild(rightBtn);
        toolbar.appendChild(removeBtn);
        
        return toolbar;
    }
    
    /**
     * Align image container
     */
    alignImage(button, alignment) {
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
        
        // Trigger content change event
        document.dispatchEvent(new CustomEvent('blogEditorContentChange'));
    }
    
    /**
     * Process images to ensure proper formatting
     */
    processImages() {
        if (!this.editor) return;
        
        // First clean up any existing image wrappers or old structures
        this.cleanupExistingImages();
        
        const images = this.editor.querySelectorAll('img:not([data-uploading])');
        
        images.forEach(img => {
            // Remove any old attributes that might cause issues
            if (img.hasAttribute('data-resizable')) {
                img.removeAttribute('data-resizable');
            }
            
            if (img.classList.contains('blog-image')) {
                img.classList.remove('blog-image');
            }
            
            // Skip already processed images
            if (img.closest('.image-container')) return;
            
            // Get parent and check if it's already a container or wrapper
            const parent = img.parentNode;
            if (parent && (parent.classList.contains('image-wrapper') || parent.classList.contains('image-container'))) {
                // If it's an old wrapper but not a container, fix it
                if (parent.classList.contains('image-wrapper') && !parent.classList.contains('image-container')) {
                    parent.classList.add('image-container');
                    
                    // Add toolbar if missing
                    if (!parent.querySelector('.image-toolbar')) {
                        const toolbar = this.createImageToolbar();
                        parent.prepend(toolbar);
                    }
                    
                    // Make sure image is the last child
                    parent.appendChild(img);
                    
                    // Add line break after image container if missing
                    if (!parent.nextSibling || parent.nextSibling.nodeName !== 'BR') {
                        const br = document.createElement('br');
                        br.className = 'clearfix';
                        parent.after(br);
                    }
                }
                return;
            }
            
            // Create image container
            const container = document.createElement('div');
            container.className = 'image-container';
            
            // Create toolbar
            const toolbar = this.createImageToolbar();
            
            // Wrap image in container with toolbar
            img.parentNode.insertBefore(container, img);
            container.appendChild(toolbar);
            container.appendChild(img);
            
            // Add line break after image for proper text flow
            const br = document.createElement('br');
            br.className = 'clearfix';
            if (!container.nextSibling || container.nextSibling.nodeName !== 'BR') {
                container.after(br);
            }
        });
    }
}

// Initialize image handler
window.addEventListener('DOMContentLoaded', () => {
    window.imageHandler = new ImageHandler();
}); 