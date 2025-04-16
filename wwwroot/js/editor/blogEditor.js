/**
 * BlogEditor - Custom Editor for BlogApp
 * A lightweight, modern content editor with rich text formatting, image handling, and more
 */

class BlogEditor {
    constructor(options = {}) {
        // Default configuration
        this.config = {
            selector: options.selector || '#editor',
            contentSelector: options.contentSelector || '#Content',
            toolbarSelector: options.toolbarSelector || '#editorToolbar',
            previewSelector: options.previewSelector || '#contentPreview',
            charCountSelector: options.charCountSelector || '#charCount',
            wordCountSelector: options.wordCountSelector || '#wordCount',
            uploadEndpoint: options.uploadEndpoint || '/Posts/UploadImage',
            placeholder: options.placeholder || 'Start writing your blog post...',
            autosaveInterval: options.autosaveInterval || 30000, // 30 seconds
            darkMode: options.darkMode || false,
            tools: options.tools || [
                'heading', 'bold', 'italic', 'underline', 'strikethrough',
                'link', 'image', 'video', 'code', 'quote', 'list', 'align',
                'table', 'latex'
            ]
        };

        // Editor state
        this.state = {
            content: '',
            selection: null,
            images: [],
            isDirty: false,
            lastSaved: new Date(),
            history: [],
            historyIndex: -1,
            darkMode: this.config.darkMode
        };

        // Initialization
        this.init();
    }

    /**
     * Initialize the editor
     */
    init() {
        this.editor = document.querySelector(this.config.selector);
        this.contentInput = document.querySelector(this.config.contentSelector);
        this.toolbar = document.querySelector(this.config.toolbarSelector);
        this.preview = document.querySelector(this.config.previewSelector);
        this.charCount = document.querySelector(this.config.charCountSelector);
        this.wordCount = document.querySelector(this.config.wordCountSelector);

        if (!this.editor || !this.contentInput) {
            console.error('Editor elements not found. Check your selectors.');
            return;
        }

        // Set up editor container
        this.setupEditor();
        
        // Initialize toolbar
        this.setupToolbar();
        
        // Set up events
        this.setupEvents();
        
        // Initialize with existing content if any
        if (this.contentInput.value) {
            this.setContent(this.contentInput.value);
        }
        
        // Set up autosave
        if (this.config.autosaveInterval > 0) {
            this.setupAutosave();
        }

        // Set up word/char counter
        this.updateCounters();
        
        // Ensure MathJax is loaded if needed
        this.ensureMathJaxLoaded();
        
        console.log('BlogEditor initialized successfully');
    }
    
    /**
     * Set the editor content
     * @param {string} html - The HTML content to set
     */
    setContent(html) {
        try {
            if (!this.editor) return;
            
            // Set the content
            this.editor.innerHTML = html || '';
            
            // Update state
            this.state.content = html || '';
            
            // Update counters
            this.updateCounters();
            
            // Process images and other special elements
            setTimeout(() => {
                if (window.imageHandler && typeof window.imageHandler.processImages === 'function') {
                    window.imageHandler.processImages();
                }
                this.processSpecialElements();
            }, 100);
            
            console.log('Content set successfully');
        } catch (error) {
            console.error('Error setting content:', error);
        }
    }

    /**
     * Check and ensure MathJax is loaded
     */
    ensureMathJaxLoaded() {
        if (typeof MathJax === 'undefined') {
            // Add MathJax script if not present
            const script = document.createElement('script');
            script.src = 'https://cdn.jsdelivr.net/npm/mathjax@3/es5/tex-mml-chtml.js';
            script.async = true;
            script.id = 'mathjax-script';
            
            // Configure MathJax
            window.MathJax = {
                tex: {
                    inlineMath: [['$', '$'], ['\\(', '\\)']],
                    displayMath: [['$$', '$$'], ['\\[', '\\]']],
                    processEscapes: true,
                    processEnvironments: true
                },
                options: {
                    skipHtmlTags: ['script', 'noscript', 'style', 'textarea', 'pre']
                }
            };
            
            document.head.appendChild(script);
            
            console.log('MathJax script added to document');
        }
    }

    /**
     * Set up editor container
     */
    setupEditor() {
        this.editor.classList.add('blog-editor');
        this.editor.setAttribute('contenteditable', 'true');
        this.editor.setAttribute('spellcheck', 'true');
        this.editor.setAttribute('placeholder', this.config.placeholder);
        
        // Ensure the editor has a minimum height
        this.editor.style.minHeight = '300px';
        
        // Apply dark mode if enabled
        if (this.state.darkMode) {
            this.editor.classList.add('dark-mode');
        }
        
        // Add editor styles
        this.addEditorStyles();
    }

    /**
     * Set up toolbar buttons and functionality
     */
    setupToolbar() {
        if (!this.toolbar) return;
        
        const tools = {
            heading: {
                icon: '<i class="bi bi-type"></i>',
                tooltip: 'Heading',
                dropdown: [
                    { 
                        text: '<span style="font-size: 2em; font-weight: bold; line-height: 1.2;">Heading 1</span>', 
                        action: () => this.execCommand('formatBlock', '<h1>'),
                        html: true 
                    },
                    { 
                        text: '<span style="font-size: 1.5em; font-weight: bold; line-height: 1.2;">Heading 2</span>', 
                        action: () => this.execCommand('formatBlock', '<h2>'),
                        html: true 
                    },
                    { 
                        text: '<span style="font-size: 1.25em; font-weight: bold; line-height: 1.2;">Heading 3</span>', 
                        action: () => this.execCommand('formatBlock', '<h3>'),
                        html: true 
                    },
                    { 
                        text: '<span style="font-size: 1.1em; font-weight: bold; line-height: 1.2;">Heading 4</span>', 
                        action: () => this.execCommand('formatBlock', '<h4>'),
                        html: true 
                    },
                    { 
                        text: '<span style="font-size: 1em; line-height: 1.2;">Paragraph</span>', 
                        action: () => this.execCommand('formatBlock', '<p>'),
                        html: true 
                    },
                ]
            },
            bold: {
                icon: '<i class="bi bi-type-bold"></i>',
                tooltip: 'Bold',
                action: () => this.execCommand('bold')
            },
            italic: {
                icon: '<i class="bi bi-type-italic"></i>',
                tooltip: 'Italic',
                action: () => this.execCommand('italic')
            },
            underline: {
                icon: '<i class="bi bi-type-underline"></i>',
                tooltip: 'Underline',
                action: () => this.execCommand('underline')
            },
            strikethrough: {
                icon: '<i class="bi bi-type-strikethrough"></i>',
                tooltip: 'Strikethrough',
                action: () => this.execCommand('strikeThrough')
            },
            link: {
                icon: '<i class="bi bi-link"></i>',
                tooltip: 'Insert Link',
                action: () => this.insertLink()
            },
            image: {
                icon: '<i class="bi bi-image"></i>',
                tooltip: 'Insert Image',
                action: () => this.insertImage()
            },
            video: {
                icon: '<i class="bi bi-youtube"></i>',
                tooltip: 'Insert Video',
                action: () => this.insertVideo()
            },
            code: {
                icon: '<i class="bi bi-code"></i>',
                tooltip: 'Code Block',
                action: () => this.insertCodeBlock()
            },
            quote: {
                icon: '<i class="bi bi-chat-square-quote"></i>',
                tooltip: 'Quote',
                action: () => this.execCommand('formatBlock', '<blockquote>')
            },
            list: {
                icon: '<i class="bi bi-list-ul"></i>',
                tooltip: 'List',
                dropdown: [
                    { text: 'Bullet List', action: () => this.execCommand('insertUnorderedList') },
                    { text: 'Numbered List', action: () => this.execCommand('insertOrderedList') }
                ]
            },
            align: {
                icon: '<i class="bi bi-text-left"></i>',
                tooltip: 'Align',
                dropdown: [
                    { text: 'Left', action: () => this.execCommand('justifyLeft') },
                    { text: 'Center', action: () => this.execCommand('justifyCenter') },
                    { text: 'Right', action: () => this.execCommand('justifyRight') },
                    { text: 'Justify', action: () => this.execCommand('justifyFull') }
                ]
            },
            table: {
                icon: '<i class="bi bi-table"></i>',
                tooltip: 'Insert Table',
                action: () => this.insertTable()
            },
            latex: {
                icon: '<i class="bi bi-calculator"></i>',
                tooltip: 'LaTeX Formula',
                action: () => this.insertLaTeX()
            }
        };

        // Clear existing toolbar
        this.toolbar.innerHTML = '';
        
        // Add buttons for each tool
        this.config.tools.forEach(toolName => {
            if (!tools[toolName]) return;
            
            const tool = tools[toolName];
            const button = document.createElement('button');
            button.type = 'button';
            button.className = 'btn btn-light btn-sm editor-tool';
            button.setAttribute('data-tool', toolName);
            button.innerHTML = tool.icon;
            button.title = tool.tooltip;
            
            if (tool.action) {
                button.addEventListener('click', tool.action);
            }
            
            if (tool.dropdown) {
                // Create dropdown for tools with multiple options
                const dropdownContainer = document.createElement('div');
                dropdownContainer.className = 'btn-group';
                
                const dropdownButton = document.createElement('button');
                dropdownButton.type = 'button';
                dropdownButton.className = 'btn btn-light btn-sm dropdown-toggle';
                dropdownButton.innerHTML = tool.icon;
                dropdownButton.title = tool.tooltip;
                dropdownButton.setAttribute('data-bs-toggle', 'dropdown');
                dropdownButton.setAttribute('aria-expanded', 'false');
                
                const dropdownMenu = document.createElement('ul');
                dropdownMenu.className = 'dropdown-menu';
                
                tool.dropdown.forEach(option => {
                    const item = document.createElement('li');
                    const link = document.createElement('a');
                    link.className = 'dropdown-item';
                    link.href = '#';
                    
                    // Handle HTML content in dropdown items
                    if (option.html) {
                        link.innerHTML = option.text;
                    } else {
                        link.textContent = option.text;
                    }
                    
                    link.addEventListener('click', (e) => {
                        e.preventDefault();
                        option.action();
                    });
                    item.appendChild(link);
                    dropdownMenu.appendChild(item);
                });
                
                dropdownContainer.appendChild(dropdownButton);
                dropdownContainer.appendChild(dropdownMenu);
                this.toolbar.appendChild(dropdownContainer);
            } else {
                this.toolbar.appendChild(button);
            }
        });

        // Add dark mode toggle
        const darkModeBtn = document.createElement('button');
        darkModeBtn.type = 'button';
        darkModeBtn.className = 'btn btn-light btn-sm ms-auto';
        darkModeBtn.innerHTML = '<i class="bi bi-moon"></i>';
        darkModeBtn.title = 'Toggle Dark Mode';
        darkModeBtn.addEventListener('click', () => this.toggleDarkMode());
        this.toolbar.appendChild(darkModeBtn);
    }

    /**
     * Set up event listeners
     */
    setupEvents() {
        // Track content changes
        this.editor.addEventListener('input', () => {
            this.state.isDirty = true;
            this.updateContent();
            this.updateCounters();
        });

        // Handle paste events to clean up pasted content
        this.editor.addEventListener('paste', (e) => this.handlePaste(e));
        
        // Handle drop events for images
        this.editor.addEventListener('dragover', (e) => {
            e.preventDefault();
            this.editor.classList.add('dragover');
        });
        
        this.editor.addEventListener('dragleave', () => {
            this.editor.classList.remove('dragover');
        });
        
        this.editor.addEventListener('drop', (e) => this.handleDrop(e));
        
        // Track selection for toolbar state
        document.addEventListener('selectionchange', () => {
            if (document.activeElement === this.editor) {
                this.state.selection = document.getSelection();
                this.updateToolbarState();
            }
        });
        
        // Add Clear Content button to toolbar
        const clearBtn = document.createElement('button');
        clearBtn.type = 'button';
        clearBtn.className = 'btn btn-light btn-sm ms-2';
        clearBtn.innerHTML = '<i class="bi bi-trash"></i>';
        clearBtn.title = 'Clear Content';
        clearBtn.addEventListener('click', () => {
            if (confirm('Are you sure you want to clear all content?')) {
                this.clearContent();
            }
        });
        
        if (this.toolbar) {
            this.toolbar.appendChild(clearBtn);
        }
        
        // Add keyboard shortcut listeners
        document.addEventListener('keydown', (e) => {
            // Ctrl+Shift+X to clear content
            if (e.ctrlKey && e.shiftKey && e.key === 'X') {
                if (confirm('Are you sure you want to clear all content?')) {
                    this.clearContent();
                }
                e.preventDefault();
            }
        });
    }

    /**
     * Set up autosave functionality
     */
    setupAutosave() {
        setInterval(() => {
            if (this.state.isDirty) {
                this.saveContent();
                console.log('Content autosaved');
            }
        }, this.config.autosaveInterval);
    }

    /**
     * Save the current content
     */
    saveContent() {
        this.updateContent();
        
        // Trigger the form's autosave if available
        if (typeof window.autoSavePost === 'function') {
            window.autoSavePost();
        }
        
        // Reset dirty state
        this.state.isDirty = false;
    }

    /**
     * Execute a document.execCommand with proper focus handling
     */
    execCommand(command, value = null) {
        this.editor.focus();
        document.execCommand(command, false, value);
        this.updateContent();
        this.state.isDirty = true;
    }

    /**
     * Update content in the hidden input field
     */
    updateContent() {
        // Process special elements before saving content
        this.processSpecialElements();
        
        this.state.content = this.editor.innerHTML;
        if (this.contentInput) {
            this.contentInput.value = this.state.content;
        }
        
        // Trigger content change event
        this.triggerContentChange();
    }

    /**
     * Trigger content change event
     */
    triggerContentChange() {
        // Create and dispatch custom event
        const event = new CustomEvent('editor-content-change', {
            detail: { content: this.state.content }
        });
        this.editor.dispatchEvent(event);
        
        // Update state
        this.state.isDirty = true;
    }

    /**
     * Update character and word counts
     */
    updateCounters() {
        if (!this.charCount && !this.wordCount) return;
        
        const text = this.editor.textContent || '';
        const charCount = text.length;
        const wordCount = text.trim() ? text.trim().split(/\s+/).length : 0;
        
        if (this.charCount) {
            this.charCount.textContent = charCount;
        }
        
        if (this.wordCount) {
            this.wordCount.textContent = wordCount;
        }
    }

    /**
     * Handle file drop events for image uploads
     */
    handleDrop(e) {
        e.preventDefault();
        e.stopPropagation();
        
        this.editor.classList.remove('dragover');
        
        // Check if files were dropped
        if (e.dataTransfer && e.dataTransfer.files && e.dataTransfer.files.length > 0) {
            const files = Array.from(e.dataTransfer.files);
            
            // Handle only image files
            const imageFiles = files.filter(file => file.type.startsWith('image/'));
            
            if (imageFiles.length > 0) {
                // Save current cursor position
                const savedSelection = this.saveSelection();
                
                // Process each image file
                const uploadPromises = imageFiles.map(file => {
                    const formData = new FormData();
                    formData.append('image', file);
                    
                    return fetch(this.config.uploadEndpoint, {
                        method: 'POST',
                        body: formData
                    })
                    .then(response => response.json())
                    .then(data => {
                        if (data.success && data.imageUrl) {
                            return data.imageUrl;
                        }
                        throw new Error(data.message || 'Upload failed');
                    });
                });
                
                // Process all uploads
                Promise.all(uploadPromises)
                    .then(imageUrls => {
                        // Restore selection
                        this.restoreSelection(savedSelection);
                        
                        // Insert all images at cursor position
                        imageUrls.forEach(url => {
                            const img = document.createElement('img');
                            img.src = url;
                            img.alt = 'Dropped image';
                            img.className = 'blog-image';
                            
                            this.insertNodeAtCursor(img);
                        });
                        
                        // Make the images resizable
                        this.makeImagesResizable();
                        
                        // Update content
                        this.updateContent();
                    })
                    .catch(error => {
                        console.error('Error uploading dropped images:', error);
                        alert('Error uploading one or more images. Please try again.');
                    });
            } else {
                alert('Please drop only image files.');
            }
        }
    }

    /**
     * Handle paste events to clean up pasted content
     */
    handlePaste(e) {
        // Check if paste contains files (images)
        if (e.clipboardData && e.clipboardData.files && e.clipboardData.files.length > 0) {
            e.preventDefault();
            
            const files = Array.from(e.clipboardData.files);
            const imageFiles = files.filter(file => file.type.startsWith('image/'));
            
            if (imageFiles.length > 0) {
                // Process pasted images similar to dropped images
                const savedSelection = this.saveSelection();
                
                imageFiles.forEach(file => {
                    const formData = new FormData();
                    formData.append('image', file);
                    
                    fetch(this.config.uploadEndpoint, {
                        method: 'POST',
                        body: formData
                    })
                    .then(response => response.json())
                    .then(data => {
                        if (data.success && data.imageUrl) {
                            // Restore selection
                            this.restoreSelection(savedSelection);
                            
                            // Insert image
                            const img = document.createElement('img');
                            img.src = data.imageUrl;
                            img.alt = 'Pasted image';
                            img.className = 'blog-image';
                            
                            this.insertNodeAtCursor(img);
                            this.makeImagesResizable();
                            this.updateContent();
                        } else {
                            throw new Error(data.message || 'Upload failed');
                        }
                    })
                    .catch(error => {
                        console.error('Error uploading pasted image:', error);
                        alert('Error uploading pasted image. Please try again.');
                    });
                });
                
                return;
            }
        }
        
        // For regular text content, let the browser handle it
        // But clean up the content to remove unwanted styles
        if (e.clipboardData && e.clipboardData.getData) {
            e.preventDefault();
            
            // Get plain text and HTML from clipboard
            const text = e.clipboardData.getData('text/plain');
            let html = e.clipboardData.getData('text/html');
            
            // If HTML is available, clean it up
            if (html) {
                // Create a temporary element to clean the HTML
                const tempDiv = document.createElement('div');
                tempDiv.innerHTML = html;
                
                // Remove unwanted elements and attributes
                const unwantedTags = ['script', 'style', 'meta', 'link', 'head'];
                unwantedTags.forEach(tag => {
                    const elements = tempDiv.getElementsByTagName(tag);
                    for (let i = elements.length - 1; i >= 0; i--) {
                        elements[i].parentNode.removeChild(elements[i]);
                    }
                });
                
                // Clean all remaining elements
                const cleanElement = (element) => {
                    // Allow only specific attributes
                    const allowedAttrs = ['href', 'src', 'alt', 'title'];
                    
                    // Remove all attributes except allowed ones
                    Array.from(element.attributes).forEach(attr => {
                        if (!allowedAttrs.includes(attr.name)) {
                            element.removeAttribute(attr.name);
                        }
                    });
                    
                    // Clean all child elements recursively
                    Array.from(element.children).forEach(cleanElement);
                };
                
                cleanElement(tempDiv);
                
                // Insert the cleaned HTML
                document.execCommand('insertHTML', false, tempDiv.innerHTML);
            } else {
                // If no HTML, insert plain text
                document.execCommand('insertText', false, text);
            }
            
            // Update content
            this.updateContent();
        }
    }

    /**
     * Update toolbar state based on current selection
     */
    updateToolbarState() {
        if (!this.toolbar) return;
        
        // Get selected element and parent elements
        const selection = window.getSelection();
        if (!selection.rangeCount) return;
        
        const range = selection.getRangeAt(0);
        let element = range.commonAncestorContainer;
        
        // If it's a text node, get its parent
        if (element.nodeType === 3) {
            element = element.parentNode;
        }
        
        // Add active class to relevant toolbar buttons
        const buttons = this.toolbar.querySelectorAll('.editor-tool');
        buttons.forEach(button => {
            button.classList.remove('active');
            
            const tool = button.getAttribute('data-tool');
            
            if (tool === 'bold' && element.closest('strong, b')) {
                button.classList.add('active');
            } else if (tool === 'italic' && element.closest('em, i')) {
                button.classList.add('active');
            } else if (tool === 'underline' && element.closest('u')) {
                button.classList.add('active');
            } else if (tool === 'strikethrough' && element.closest('s, strike')) {
                button.classList.add('active');
            } else if (tool === 'code' && element.closest('code, pre')) {
                button.classList.add('active');
            } else if (tool === 'quote' && element.closest('blockquote')) {
                button.classList.add('active');
            }
        });
    }

    /**
     * Insert image at cursor position
     */
    insertImage() {
        // Use the fixed image upload functionality from editorFixes
        if (window.editorFixes && typeof window.editorFixes.fixImageUpload === 'function') {
            // Simulate click on the file input (the implementation is in editorFixes.js)
            const imageBtn = document.querySelector('.editor-tool[data-tool="image"]');
            if (imageBtn) {
                // Create and trigger a click event
                const clickEvent = new MouseEvent('click', {
                    bubbles: true,
                    cancelable: true,
                    view: window
                });
                // This will activate the click handler in fixImageUpload
                // Prevent infinite recursion by setting a flag
                if (!this._imageUploadInProgress) {
                    this._imageUploadInProgress = true;
                    imageBtn.dispatchEvent(clickEvent);
                    // Reset the flag after a short delay
                    setTimeout(() => {
                        this._imageUploadInProgress = false;
                    }, 100);
                }
                return;
            }
        }
        
        // Fallback implementation if editorFixes is not available
        const fileInput = document.createElement('input');
        fileInput.type = 'file';
        fileInput.accept = 'image/*';
        fileInput.style.display = 'none';
        document.body.appendChild(fileInput);
        
        // Save selection
        const savedSelection = this.saveSelection();
        
        fileInput.addEventListener('change', () => {
            if (fileInput.files && fileInput.files[0]) {
                // Restore selection
                this.restoreSelection(savedSelection);
                
                const file = fileInput.files[0];
                const formData = new FormData();
                formData.append('image', file);
                
                // Upload image
                fetch(this.config.uploadEndpoint, {
                    method: 'POST',
                    body: formData
                })
                .then(response => response.json())
                .then(data => {
                    if (data.success && data.imageUrl) {
                        // Create image and insert at cursor
                        const img = document.createElement('img');
                        img.src = data.imageUrl;
                        img.alt = file.name.split('.')[0] || 'Blog image';
                        img.className = 'blog-image';
                        
                        // Insert at cursor position
                        this.insertNodeAtCursor(img);
                        
                        // Make the image resizable
                        this.makeImagesResizable();
                        
                        // Update content
                        this.updateContent();
                    } else {
                        console.error('Image upload failed:', data.message || 'Unknown error');
                        alert('Image upload failed: ' + (data.message || 'Unknown error'));
                    }
                })
                .catch(error => {
                    console.error('Error uploading image:', error);
                    alert('Error uploading image. Please try again.');
                });
            }
            
            // Remove the input
            document.body.removeChild(fileInput);
        });
        
        fileInput.click();
    }

    /**
     * Process special elements like code blocks, formulas, and videos to ensure they're saved correctly
     */
    processSpecialElements() {
        // Process code blocks to preserve formatting
        const codeBlocks = this.editor.querySelectorAll('pre code');
        codeBlocks.forEach(code => {
            // Ensure code content is preserved (not sanitized)
            code.setAttribute('data-preserve-content', 'true');
            if (code.parentElement && code.parentElement.tagName === 'PRE') {
                code.parentElement.setAttribute('data-preserve-content', 'true');
            }
        });
        
        // Process LaTeX formulas
        const latexElements = this.editor.querySelectorAll('.latex-formula');
        latexElements.forEach(latex => {
            // Ensure LaTeX content is preserved
            latex.setAttribute('data-preserve-content', 'true');
            
            // Remove edit buttons if present
            const editBtn = latex.querySelector('.latex-edit-btn');
            if (editBtn) editBtn.remove();
        });
        
        // Process videos to ensure proper structure
        const videos = this.editor.querySelectorAll('iframe');
        videos.forEach(iframe => {
            if (!iframe.closest('.video-container')) {
                // Create video container if not exists
                const videoContainer = document.createElement('div');
                videoContainer.className = 'video-container';
                videoContainer.setAttribute('contenteditable', 'false');
                videoContainer.style.position = 'relative';
                videoContainer.style.width = '100%';
                videoContainer.style.marginBottom = '20px';
                videoContainer.style.clear = 'both';
                
                // Create responsive wrapper
                const wrapper = document.createElement('div');
                wrapper.style.position = 'relative';
                wrapper.style.paddingBottom = '56.25%'; // 16:9 aspect ratio
                wrapper.style.height = '0';
                wrapper.style.overflow = 'hidden';
                wrapper.style.maxWidth = '100%';
                
                // Add iframe to wrapper
                iframe.style.position = 'absolute';
                iframe.style.top = '0';
                iframe.style.left = '0';
                iframe.style.width = '100%';
                iframe.style.height = '100%';
                
                // Set up structure
                iframe.parentNode.insertBefore(videoContainer, iframe);
                wrapper.appendChild(iframe);
                videoContainer.appendChild(wrapper);
                
                // Add video toolbar
                this.addVideoToolbar(videoContainer);
            }
        });
        
        // Process tables to ensure they have proper structure
        const tables = this.editor.querySelectorAll('table');
        tables.forEach(table => {
            table.setAttribute('data-preserve-structure', 'true');
            
            // Ensure table has proper classes
            if (!table.classList.contains('table')) {
                table.classList.add('table', 'table-bordered');
            }
        });
    }
    
    /**
     * Insert video at cursor position
     * @param {string} [url] - Optional YouTube URL to prefill
     */
    insertVideo(url = '') {
        const modalId = 'videoInsertModal';
        const modalSelector = `#${modalId}`;
        const saveSelection = this.saveSelection();
        
        // Create modal if it doesn't exist
        if (!document.getElementById(modalId)) {
            const modal = document.createElement('div');
            modal.className = 'modal fade';
            modal.id = modalId;
            modal.setAttribute('tabindex', '-1');
            modal.setAttribute('role', 'dialog');
            modal.setAttribute('aria-labelledby', `${modalId}Label`);
            modal.setAttribute('aria-hidden', 'true');
            
            modal.innerHTML = `
                <div class="modal-dialog" role="document">
                    <div class="modal-content">
                        <div class="modal-header">
                            <h5 class="modal-title" id="${modalId}Label">Insert YouTube Video</h5>
                            <!-- Include both Bootstrap 4 and 5 style close buttons for compatibility -->
                            <button type="button" class="close btn-close" data-dismiss="modal" data-bs-dismiss="modal" aria-label="Close" id="${modalId}Close" name="${modalId}Close">
                                <span aria-hidden="true">&times;</span>
                            </button>
                        </div>
                        <div class="modal-body">
                            <form id="videoForm" name="videoForm">
                                <div class="form-group mb-3">
                                    <label for="videoUrl" id="videoUrlLabel">YouTube URL</label>
                                    <input type="text" class="form-control" id="videoUrl" name="videoUrl"
                                           placeholder="https://www.youtube.com/watch?v=..." aria-labelledby="videoUrlLabel">
                                    <small class="form-text text-muted" id="videoUrlHelp">
                                        Enter a YouTube video URL or video ID
                                    </small>
                                </div>
                                <div class="form-group mb-3">
                                    <label for="videoSize" id="videoSizeLabel">Video Size</label>
                                    <select class="form-control" id="videoSize" name="videoSize" aria-labelledby="videoSizeLabel">
                                        <option value="regular" selected>Regular (16:9)</option>
                                        <option value="wide">Wide (21:9)</option>
                                        <option value="square">Square (1:1)</option>
                                    </select>
                                </div>
                                <div id="videoPreviewContainer" class="d-none mt-3">
                                    <h6 id="previewLabel">Preview:</h6>
                                    <div id="videoPreview" class="embed-responsive embed-responsive-16by9" aria-labelledby="previewLabel">
                                        <!-- Preview will be inserted here -->
                                    </div>
                                </div>
                            </form>
                        </div>
                        <div class="modal-footer">
                            <!-- Include both Bootstrap 4 and 5 data attributes -->
                            <button type="button" class="btn btn-secondary" data-dismiss="modal" data-bs-dismiss="modal" id="cancelVideoBtn" name="cancelVideoBtn">Cancel</button>
                            <button type="button" class="btn btn-primary" id="insertVideoBtn" name="insertVideoBtn">Insert</button>
                        </div>
                    </div>
                </div>
            `;
            
            document.body.appendChild(modal);
            
            // Add event listeners for modal events
            modal.addEventListener('hidden.bs.modal', function() {
                // Clean up after modal is hidden
                const urlInput = document.getElementById('videoUrl');
                const previewContainer = document.getElementById('videoPreviewContainer');
                const preview = document.getElementById('videoPreview');
                
                // Reset form values
                if (urlInput) urlInput.value = '';
                if (previewContainer) previewContainer.classList.add('d-none');
                if (preview) preview.innerHTML = '';
                
                // Remove backdrop if it exists
                const backdrop = document.querySelector('.modal-backdrop');
                if (backdrop) backdrop.remove();
                document.body.classList.remove('modal-open');
            });
        }
        
        // Get modal instance and elements
        const modal = document.getElementById(modalId);
        const urlInput = document.getElementById('videoUrl');
        const previewContainer = document.getElementById('videoPreviewContainer');
        const preview = document.getElementById('videoPreview');
        const insertBtn = document.getElementById('insertVideoBtn');
        const sizeSelect = document.getElementById('videoSize');
        
        // Clear previous values
        previewContainer.classList.add('d-none');
        preview.innerHTML = '';
        insertBtn.disabled = false;
        
        // Set up URL input handler if not already set
        if (!urlInput._handlerAdded) {
            const self = this; // Store 'this' reference for the event handler
            urlInput.addEventListener('input', function() {
                const videoUrl = this.value.trim();
                const videoId = self.extractYouTubeId(videoUrl);
                
                if (videoId) {
                    // Show preview
                    previewContainer.classList.remove('d-none');
                    preview.innerHTML = `
                        <iframe width="100%" height="auto" 
                                src="https://www.youtube.com/embed/${videoId}" 
                                frameborder="0" 
                                allow="accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture" 
                                allowfullscreen></iframe>
                    `;
                    // Update data
                    preview.dataset.videoId = videoId;
                    preview.dataset.videoUrl = videoUrl;
                    insertBtn.disabled = false;
                } else {
                    // Hide preview if URL is invalid
                    previewContainer.classList.add('d-none');
                    preview.innerHTML = '';
                    delete preview.dataset.videoId;
                    delete preview.dataset.videoUrl;
                    insertBtn.disabled = true;
                }
            });
            urlInput._handlerAdded = true;
        }
        
        // Set up insert button handler if not already set
        if (!insertBtn._handlerAdded) {
            insertBtn.addEventListener('click', () => {
                const videoId = preview.dataset.videoId;
                const videoUrl = preview.dataset.videoUrl || urlInput.value.trim();
                const size = sizeSelect.value;
                
                if (videoId) {
                    // Restore selection
                    this.restoreSelection(saveSelection);
                    
                    // Create video container element with proper structure
                    const videoContainer = document.createElement('div');
                    videoContainer.className = 'blog-video';
                    videoContainer.dataset.videoId = videoId;
                    videoContainer.dataset.videoUrl = videoUrl;
                    
                    // Create responsive wrapper for the video
                    const wrapper = document.createElement('div');
                    wrapper.className = 'embed-responsive embed-responsive-16by9';
                    wrapper.style.position = 'relative';
                    wrapper.style.paddingBottom = '56.25%'; // 16:9 aspect ratio
                    wrapper.style.height = '0';
                    wrapper.style.overflow = 'hidden';
                    wrapper.style.maxWidth = '100%';
                    
                    // Add size class
                    if (size === 'wide') {
                        videoContainer.classList.add('wide');
                        wrapper.style.paddingBottom = '42.85%'; // 21:9 aspect ratio
                    } else if (size === 'square') {
                        videoContainer.classList.add('square');
                        wrapper.style.paddingBottom = '100%'; // 1:1 aspect ratio
                    }
                    
                    // Create iframe with proper attributes
                    const iframe = document.createElement('iframe');
                    iframe.src = `https://www.youtube.com/embed/${videoId}`;
                    iframe.setAttribute('frameborder', '0');
                    iframe.setAttribute('allowfullscreen', '');
                    iframe.setAttribute('allow', 'accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture');
                    iframe.setAttribute('title', 'YouTube video player');
                    iframe.setAttribute('loading', 'lazy');
                    iframe.style.position = 'absolute';
                    iframe.style.top = '0';
                    iframe.style.left = '0';
                    iframe.style.width = '100%';
                    iframe.style.height = '100%';
                    
                    // Assemble the structure
                    wrapper.appendChild(iframe);
                    videoContainer.appendChild(wrapper);
                    
                    // Add toolbar for video management
                    this.addVideoToolbar(videoContainer);
                    
                    // Insert at cursor position
                    this.insertNodeAtCursor(videoContainer);
                    this.saveSelection();
                    
                    // Add a paragraph after video if needed
                    const p = document.createElement('p');
                    p.innerHTML = '<br>';
                    if (videoContainer.parentNode) {
                        videoContainer.parentNode.insertBefore(p, videoContainer.nextSibling);
                    }
                    
                    // Close modal
                    try {
                        // Try Bootstrap 5 way first
                        const bsModal = bootstrap.Modal.getInstance(modal);
                        if (bsModal) {
                            bsModal.hide();
                        } else {
                            // Fallback to jQuery for Bootstrap 4
                            $(modalSelector).modal('hide');
                        }
                    } catch (e) {
                        console.error('Error closing modal:', e);
                        // Manual fallback
                        modal.classList.remove('show');
                        modal.style.display = 'none';
                        document.querySelector('.modal-backdrop')?.remove();
                        document.body.classList.remove('modal-open');
                    }
                }
            });
            insertBtn._handlerAdded = true;
        }
        
        // Prefill URL if provided and trigger preview
        if (url) {
            urlInput.value = url;
            // Trigger input event to update preview
            const event = new Event('input', { bubbles: true });
            urlInput.dispatchEvent(event);
        } else {
            urlInput.value = '';
            previewContainer.classList.add('d-none');
            preview.innerHTML = '';
        }
        
        // Show the modal
        try {
            // Try Bootstrap 5 way first
            const bsModal = new bootstrap.Modal(modal);
            bsModal.show();
        } catch (e) {
            console.error('Bootstrap 5 modal failed, trying jQuery fallback:', e);
            try {
                // Fallback to jQuery for Bootstrap 4
                $(modalSelector).modal('show');
            } catch (e2) {
                console.error('jQuery modal failed, using manual fallback:', e2);
                // Manual fallback
                modal.classList.add('show');
                modal.style.display = 'block';
                
                // Add backdrop if needed
                if (!document.querySelector('.modal-backdrop')) {
                    const backdrop = document.createElement('div');
                    backdrop.className = 'modal-backdrop fade show';
                    document.body.appendChild(backdrop);
                }
                document.body.classList.add('modal-open');
            }
        }
        
        // Focus on URL input after modal is shown
        setTimeout(() => {
            urlInput.focus();
        }, 300);
    }

    /**
     * Add toolbar to video containers
     * @param {HTMLElement} container - The video container element
     */
    addVideoToolbar(container) {
        if (container.querySelector('.video-toolbar')) return;
        
        // Create video toolbar
        const toolbar = document.createElement('div');
        toolbar.className = 'video-toolbar';
        toolbar.style.position = 'absolute';
        toolbar.style.top = '-40px';
        toolbar.style.right = '0';
        toolbar.style.backgroundColor = 'rgba(0, 0, 0, 0.7)';
        toolbar.style.borderRadius = '4px';
        toolbar.style.padding = '5px 10px';
        toolbar.style.zIndex = '100';
        toolbar.style.display = 'none';
        toolbar.style.gap = '10px';
        toolbar.style.alignItems = 'center';
        
        // Generate unique IDs for controls
        const toolbarId = 'video-toolbar-' + Math.random().toString(36).substr(2, 9);
        const widthInputId = 'video-width-' + Math.random().toString(36).substr(2, 9);
        const widthLabelId = widthInputId + '-label';
        const deleteButtonId = 'video-delete-' + Math.random().toString(36).substr(2, 9);
        
        toolbar.id = toolbarId;
        
        // Width control
        const widthControl = document.createElement('div');
        widthControl.className = 'video-width-control d-flex align-items-center';
        widthControl.innerHTML = `
            <label id="${widthLabelId}" class="text-white me-2" style="font-size: 12px; margin-bottom: 0;">Width:</label>
            <input type="number" min="10" max="100" value="100" 
                   id="${widthInputId}" name="${widthInputId}"
                   class="form-control form-control-sm" style="width: 60px;"
                   aria-labelledby="${widthLabelId}">
            <span class="text-white ms-1" style="font-size: 12px;">%</span>
        `;
        
        const widthInput = widthControl.querySelector('input');
        widthInput.addEventListener('change', () => {
            let width = parseInt(widthInput.value);
            if (isNaN(width) || width < 10) width = 10;
            if (width > 100) width = 100;
            
            container.style.width = `${width}%`;
            
            // Update the state
            this.state.isDirty = true;
            this.updateContent();
        });
        
        // Delete button
        const deleteBtn = document.createElement('button');
        deleteBtn.type = 'button';
        deleteBtn.id = deleteButtonId;
        deleteBtn.name = deleteButtonId;
        deleteBtn.className = 'video-delete-btn btn btn-sm btn-danger';
        deleteBtn.innerHTML = '<i class="bi bi-trash"></i>';
        deleteBtn.style.background = 'none';
        deleteBtn.style.border = 'none';
        deleteBtn.style.color = 'white';
        deleteBtn.style.cursor = 'pointer';
        deleteBtn.style.fontSize = '14px';
        deleteBtn.setAttribute('aria-label', 'Delete video');
        
        // Add event listener for delete button
        deleteBtn.addEventListener('click', () => {
            if (confirm('Are you sure you want to delete this video?')) {
                container.remove();
                this.state.isDirty = true;
                this.updateContent();
            }
        });
        
        // Add controls to toolbar
        toolbar.appendChild(widthControl);
        toolbar.appendChild(deleteBtn);
        
        // Show/hide toolbar on hover
        container.addEventListener('mouseenter', () => {
            toolbar.style.display = 'flex';
        });
        
        container.addEventListener('mouseleave', () => {
            toolbar.style.display = 'none';
        });
        
        container.appendChild(toolbar);
    }

    /**
     * Extract YouTube video ID from various URL formats
     * @param {string} url - YouTube URL or video ID
     * @returns {string|null} - YouTube video ID or null if invalid
     */
    extractYouTubeId(url) {
        if (!url) return null;
        
        try {
            // Check if the url itself is just a video ID (11 characters)
            if (/^[a-zA-Z0-9_-]{11}$/.test(url)) {
                return url;
            }
            
            // Use regex patterns to match various YouTube URL formats
            const patterns = [
                // Standard YouTube URLs (youtube.com/watch?v=ID)
                /(?:youtube\.com\/(?:[^\/]+\/.+\/|(?:v|e(?:mbed)?)\/|.*[?&]v=)|youtu\.be\/)([^"&?\/\s]{11})/i,
                // Short youtu.be links
                /youtu\.be\/([^"&?\/\s]{11})/i,
                // Embed URLs
                /youtube\.com\/embed\/([^"&?\/\s]{11})/i
            ];
            
            for (const pattern of patterns) {
                const match = url.match(pattern);
                if (match && match[1]) {
                    return match[1];
                }
            }
            
            // Try parsing as URL if patterns didn't match
            try {
                const parsedUrl = new URL(url);
                
                // Handle youtube.com URLs
                if (parsedUrl.hostname.includes('youtube.com')) {
                    // Get from search params
                    if (parsedUrl.searchParams.has('v')) {
                        return parsedUrl.searchParams.get('v');
                    }
                    
                    // Handle /embed/ or /v/ formats
                    const pathMatch = parsedUrl.pathname.match(/\/(?:embed|v)\/([a-zA-Z0-9_-]{11})/);
                    if (pathMatch) return pathMatch[1];
                } 
                // Handle youtu.be URLs
                else if (parsedUrl.hostname.includes('youtu.be')) {
                    // Remove leading slash
                    return parsedUrl.pathname.substring(1);
                }
            } catch (e) {
                // URL parsing failed, continue to fallback
                console.log("URL parsing failed:", e);
            }
            
            return null;
        } catch (error) {
            console.error("Error extracting YouTube ID:", error);
            return null;
        }
    }
    
    /**
     * Save current selection
     */
    saveSelection() {
        if (window.getSelection) {
            const sel = window.getSelection();
            if (sel.getRangeAt && sel.rangeCount) {
                return sel.getRangeAt(0);
            }
        }
        return null;
    }
    
    /**
     * Restore saved selection
     */
    restoreSelection(range) {
        if (range) {
            if (window.getSelection) {
                const sel = window.getSelection();
                sel.removeAllRanges();
                sel.addRange(range);
            }
        }
    }
    
    /**
     * Insert code block
     */
    insertCodeBlock() {
        const selection = this.saveSelection();
        
        // Insert code block with triple backticks
        this.restoreSelection(selection);
        document.execCommand('insertHTML', false, '```\n// Your code here\n```');
        
        // Place cursor inside the code block
        setTimeout(() => {
            // Find the code block we just inserted
            const range = document.createRange();
            const selection = window.getSelection();
            
            // Get cursor position inside the code block
            const nodes = this.editor.childNodes;
            let codeNode = null;
            
            // Find the code block node
            for (let i = 0; i < nodes.length; i++) {
                if (nodes[i].textContent && nodes[i].textContent.trim().startsWith('```')) {
                    codeNode = nodes[i];
                    break;
                }
            }
            
            // Place cursor after the first backticks and newline
            if (codeNode && codeNode.firstChild) {
                const textContent = codeNode.textContent;
                const cursorPos = textContent.indexOf('\n') + 1;
                
                if (cursorPos > 0) {
                    // Create a text node for the content before cursor
                    const beforeCursor = document.createTextNode(textContent.substring(0, cursorPos));
                    // Create a text node for the content after cursor
                    const afterCursor = document.createTextNode(textContent.substring(cursorPos));
                    
                    // Replace the content
                    codeNode.textContent = '';
                    codeNode.appendChild(beforeCursor);
                    
                    // Set cursor position
                    range.setStart(codeNode, 1);
                    range.collapse(true);
                    
                    // Restore the rest of the content
                    codeNode.appendChild(afterCursor);
                    
                    // Apply the selection
                    selection.removeAllRanges();
                    selection.addRange(range);
                }
            }
                
                // Update content
                this.updateContent();
        }, 0);
    }
    
    /**
     * Insert table
     */
    insertTable() {
        // Create a modal for table insertion
        const modalId = 'tableInsertModal';
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
                            <h5 class="modal-title" id="${modalId}Label">Insert Table</h5>
                            <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>
                        </div>
                        <div class="modal-body">
                            <div class="row mb-3">
                                <div class="col">
                                    <label for="tableRows" class="form-label">Rows</label>
                                    <input type="number" class="form-control" id="tableRows" min="1" max="20" value="3">
                                </div>
                                <div class="col">
                                    <label for="tableCols" class="form-label">Columns</label>
                                    <input type="number" class="form-control" id="tableCols" min="1" max="10" value="3">
                                </div>
                            </div>
                            <div class="mb-3">
                                <label class="form-label">Table Preview</label>
                                <div id="tablePreview" class="table-responsive border p-2">
                                    <table class="table table-bordered">
                                        <thead>
                                            <tr>
                                                <th>Header 1</th>
                                                <th>Header 2</th>
                                                <th>Header 3</th>
                                            </tr>
                                        </thead>
                                        <tbody>
                                            <tr>
                                                <td>Cell</td>
                                                <td>Cell</td>
                                                <td>Cell</td>
                                            </tr>
                                            <tr>
                                                <td>Cell</td>
                                                <td>Cell</td>
                                                <td>Cell</td>
                                            </tr>
                                            <tr>
                                                <td>Cell</td>
                                                <td>Cell</td>
                                                <td>Cell</td>
                                            </tr>
                                        </tbody>
                                    </table>
                                </div>
                            </div>
                            <div class="mb-3">
                                <div class="form-check">
                                    <input class="form-check-input" type="checkbox" id="hasHeader" checked>
                                    <label class="form-check-label" for="hasHeader">
                                        Include header row
                                    </label>
                                </div>
                            </div>
                        </div>
                        <div class="modal-footer">
                            <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">Cancel</button>
                            <button type="button" class="btn btn-primary" id="insertTableBtn">Insert Table</button>
                        </div>
                    </div>
                </div>
            `;
            
            document.body.appendChild(modal);
            
            // Save selection when the modal is shown
            modal.addEventListener('show.bs.modal', () => {
                this.savedSelection = this.saveSelection();
            });
            
            // Preview functionality
            const rowsInput = modal.querySelector('#tableRows');
            const colsInput = modal.querySelector('#tableCols');
            const hasHeaderCheckbox = modal.querySelector('#hasHeader');
            const tablePreview = modal.querySelector('#tablePreview');
            const insertBtn = modal.querySelector('#insertTableBtn');
            
            const updatePreview = () => {
                const rows = parseInt(rowsInput.value, 10) || 3;
                const cols = parseInt(colsInput.value, 10) || 3;
                const hasHeader = hasHeaderCheckbox.checked;
                
                let html = '<table class="table table-bordered">';
                
                if (hasHeader) {
                    html += '<thead><tr>';
                    for (let i = 0; i < cols; i++) {
                        html += `<th>Header ${i+1}</th>`;
                    }
                    html += '</tr></thead>';
                }
                
                html += '<tbody>';
                for (let i = 0; i < rows; i++) {
                    html += '<tr>';
                    for (let j = 0; j < cols; j++) {
                        html += '<td>Cell</td>';
                    }
                    html += '</tr>';
                }
                html += '</tbody></table>';
                
                tablePreview.innerHTML = html;
            };
            
            rowsInput.addEventListener('input', updatePreview);
            colsInput.addEventListener('input', updatePreview);
            hasHeaderCheckbox.addEventListener('change', updatePreview);
            
            // Insert button click handler
            insertBtn.addEventListener('click', () => {
                const rows = parseInt(rowsInput.value, 10) || 3;
                const cols = parseInt(colsInput.value, 10) || 3;
                const hasHeader = hasHeaderCheckbox.checked;
                
                // Generate table HTML
                let tableHtml = '<table class="table table-bordered">';
                
                if (hasHeader) {
                    tableHtml += '<thead><tr>';
                    for (let i = 0; i < cols; i++) {
                        tableHtml += `<th>Header ${i+1}</th>`;
                    }
                    tableHtml += '</tr></thead>';
                }
                
                tableHtml += '<tbody>';
                for (let i = 0; i < rows; i++) {
                    tableHtml += '<tr>';
                    for (let j = 0; j < cols; j++) {
                        tableHtml += '<td>Cell</td>';
                    }
                    tableHtml += '</tr>';
                }
                tableHtml += '</tbody></table>';
                
                // Restore selection and insert table
                this.restoreSelection(this.savedSelection);
                document.execCommand('insertHTML', false, tableHtml);
                this.updateContent();
                
                // Close modal
                bootstrap.Modal.getInstance(modal).hide();
            });
        }
        
        // Show the modal
        const bsModal = new bootstrap.Modal(modal);
        bsModal.show();
    }
    
    /**
     * Insert LaTeX formula
     */
    insertLaTeX() {
        const saveSelection = this.saveSelection();
        
        // Create a modal for LaTeX insertion
        const modalId = 'latexInsertModal';
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
                            <h5 class="modal-title" id="${modalId}Label">Insert LaTeX Formula</h5>
                            <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>
                        </div>
                        <div class="modal-body">
                            <div class="mb-3">
                                <label for="latexFormula" class="form-label">Enter LaTeX Formula</label>
                                <input type="text" class="form-control" id="latexFormula" placeholder="E = mc^2">
                            </div>
                            <div class="mb-3">
                                <label class="form-label">Preview</label>
                                <div id="latexPreview" class="bg-light p-3 rounded">
                                    <span>No formula entered</span>
                                </div>
                            </div>
                            <div class="mb-3">
                                <label class="form-label">Common Formulas</label>
                                <div class="d-flex flex-wrap gap-2">
                                    <button type="button" class="btn btn-sm btn-outline-secondary latex-example" data-formula="E = mc^2">E = mc^2</button>
                                    <button type="button" class="btn btn-sm btn-outline-secondary latex-example" data-formula="\\frac{-b \\pm \\sqrt{b^2-4ac}}{2a}">Quadratic Formula</button>
                                    <button type="button" class="btn btn-sm btn-outline-secondary latex-example" data-formula="\\sum_{i=1}^{n} i = \\frac{n(n+1)}{2}">Sum Formula</button>
                                    <button type="button" class="btn btn-sm btn-outline-secondary latex-example" data-formula="\\int_a^b f(x) \\, dx">Integral</button>
                                </div>
                            </div>
                        </div>
                        <div class="modal-footer">
                            <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">Cancel</button>
                            <button type="button" class="btn btn-primary" id="insertLatexBtn">Insert Formula</button>
                        </div>
                    </div>
                </div>
            `;
            
            document.body.appendChild(modal);
            
            // Preview functionality
            const formulaInput = modal.querySelector('#latexFormula');
            const preview = modal.querySelector('#latexPreview');
            const insertBtn = modal.querySelector('#insertLatexBtn');
            const exampleBtns = modal.querySelectorAll('.latex-example');
            
            const updatePreview = () => {
                const formula = formulaInput.value.trim();
                
                if (formula) {
                    preview.innerHTML = `<span class="latex-formula">$$${formula}$$</span>`;
                    
                    // Render with MathJax if available
                    if (typeof MathJax !== 'undefined') {
                        try {
                            MathJax.typesetPromise([preview]).catch(err => {
                                console.error('MathJax error:', err);
                                preview.innerHTML = '<span class="text-danger">Error rendering formula</span>';
                            });
                        } catch (e) {
                            console.error('MathJax error:', e);
                        }
                    }
                } else {
                    preview.innerHTML = '<span>No formula entered</span>';
                }
            };
            
            formulaInput.addEventListener('input', updatePreview);
            
            // Example buttons
            exampleBtns.forEach(btn => {
                btn.addEventListener('click', () => {
                    formulaInput.value = btn.getAttribute('data-formula');
                    updatePreview();
                });
            });
            
            // Insert button click handler
            insertBtn.addEventListener('click', () => {
                const formula = formulaInput.value.trim();
                
                if (formula) {
                    // Restore selection and insert LaTeX
                    this.restoreSelection(saveSelection);
                    
                    // Create LaTeX element with data-preserve-content
                    const latexSpan = document.createElement('span');
                    latexSpan.className = 'latex-formula';
                    latexSpan.setAttribute('data-preserve-content', 'true');
                    latexSpan.textContent = `$$${formula}$$`;
                    
                    // Insert the element
                    this.insertNodeAtCursor(latexSpan);
                    
                    // Render LaTeX formula if MathJax is available
                    if (typeof MathJax !== 'undefined') {
                        try {
                            MathJax.typesetPromise([latexSpan.parentNode]).catch(err => {
                                console.error('MathJax rendering error:', err);
                            });
                        } catch (e) {
                            console.error('MathJax error:', e);
                        }
                    }
                    
                    // Add a paragraph after formula for better editing
                        const p = document.createElement('p');
                        p.innerHTML = '<br>';
                        if (latexSpan.parentNode) {
                            latexSpan.parentNode.insertBefore(p, latexSpan.nextSibling);
                    }
                    
                    // Update content
                    this.updateContent();
                    
                    // Reset form
                    formulaInput.value = '';
                    preview.innerHTML = '<span>No formula entered</span>';
                    
                    // Close modal
                    bootstrap.Modal.getInstance(modal).hide();
                }
            });
        } else {
            // Clear the form when reopening
            const formulaInput = modal.querySelector('#latexFormula');
            const preview = modal.querySelector('#latexPreview');
            
            if (formulaInput) {
                formulaInput.value = '';
            }
            
            if (preview) {
                preview.innerHTML = '<span>No formula entered</span>';
            }
        }
        
        // Show modal
        const bsModal = new bootstrap.Modal(modal);
        bsModal.show();
    }
    
    /**
     * Get current content
     */
    getContent() {
        return this.state.content;
    }
    
    /**
     * Generate table of contents from headings
     */
    generateTOC() {
        const headings = this.editor.querySelectorAll('h1, h2, h3, h4, h5, h6');
        let toc = '<div class="toc"><h4>Table of Contents</h4><ul>';
        
        headings.forEach((heading, index) => {
            // Add ID to heading if it doesn't have one
            if (!heading.id) {
                heading.id = `heading-${index}`;
            }
            
            const level = heading.tagName.charAt(1);
            toc += `<li class="toc-level-${level}"><a href="#${heading.id}">${heading.textContent}</a></li>`;
        });
        
        toc += '</ul></div>';
        return toc;
    }

    /**
     * Make images resizable in the editor
     */
    makeImagesResizable() {
        const images = this.editor.querySelectorAll('img.blog-image:not([data-resizable="true"])');
        
        images.forEach(img => {
            // Mark as processed
            img.setAttribute('data-resizable', 'true');
            
            // Check if image is already in a wrapper
            let wrapper = img.closest('.image-wrapper');
            if (!wrapper) {
                // Create wrapper for the image
                wrapper = document.createElement('div');
                wrapper.className = 'image-wrapper';
                wrapper.style.display = 'inline-block';
                wrapper.style.position = 'relative';
                wrapper.style.margin = '5px 0';
                wrapper.style.maxWidth = '100%'; // Prevent overflow
                
                // Insert wrapper before image and move image inside wrapper
                img.parentNode.insertBefore(wrapper, img);
                wrapper.appendChild(img);
            }
            
            // Set wrapper and image styles
            img.style.display = 'block';
            img.style.maxWidth = '100%';
            
            // Add resize handles if not already added
            if (!wrapper.querySelector('.resize-handle')) {
                // Add resize handles
                const handles = ['nw', 'ne', 'sw', 'se', 'n', 's', 'e', 'w'];
                handles.forEach(position => {
                    const handle = document.createElement('div');
                    handle.className = `resize-handle resize-handle-${position}`;
                    handle.setAttribute('data-position', position);
                    handle.style.position = 'absolute';
                    handle.style.width = '10px';
                    handle.style.height = '10px';
                    handle.style.backgroundColor = '#0d6efd';
                    handle.style.zIndex = '100';
                    
                    // Position the handle
                    switch (position) {
                        case 'nw': handle.style.top = '-5px'; handle.style.left = '-5px'; handle.style.cursor = 'nwse-resize'; break;
                        case 'ne': handle.style.top = '-5px'; handle.style.right = '-5px'; handle.style.cursor = 'nesw-resize'; break;
                        case 'sw': handle.style.bottom = '-5px'; handle.style.left = '-5px'; handle.style.cursor = 'nesw-resize'; break;
                        case 'se': handle.style.bottom = '-5px'; handle.style.right = '-5px'; handle.style.cursor = 'nwse-resize'; break;
                        case 'n': handle.style.top = '-5px'; handle.style.left = '50%'; handle.style.transform = 'translateX(-50%)'; handle.style.cursor = 'ns-resize'; break;
                        case 's': handle.style.bottom = '-5px'; handle.style.left = '50%'; handle.style.transform = 'translateX(-50%)'; handle.style.cursor = 'ns-resize'; break;
                        case 'e': handle.style.right = '-5px'; handle.style.top = '50%'; handle.style.transform = 'translateY(-50%)'; handle.style.cursor = 'ew-resize'; break;
                        case 'w': handle.style.left = '-5px'; handle.style.top = '50%'; handle.style.transform = 'translateY(-50%)'; handle.style.cursor = 'ew-resize'; break;
                    }
                    
                    // Mousedown event for resizing
                    handle.addEventListener('mousedown', (e) => {
                        e.preventDefault();
                        e.stopPropagation();
                        
                        // Prevent editor from getting focus
                        this.editor.setAttribute('contenteditable', 'false');
                        
                        const startX = e.clientX;
                        const startY = e.clientY;
                        const startWidth = img.offsetWidth;
                        const startHeight = img.offsetHeight;
                        const wrapperWidth = wrapper.offsetWidth;
                        const position = handle.getAttribute('data-position');
                        
                        // Show dimensions overlay
                        const dimensions = document.createElement('div');
                        dimensions.className = 'image-dimensions';
                        dimensions.style.position = 'absolute';
                        dimensions.style.top = '0';
                        dimensions.style.left = '0';
                        dimensions.style.backgroundColor = 'rgba(0,0,0,0.7)';
                        dimensions.style.color = 'white';
                        dimensions.style.padding = '5px';
                        dimensions.style.fontSize = '12px';
                        dimensions.style.borderRadius = '3px';
                        dimensions.style.zIndex = '101';
                        dimensions.textContent = `${img.offsetWidth} × ${img.offsetHeight}`;
                        wrapper.appendChild(dimensions);
                        
                        const onMouseMove = (moveEvent) => {
                            moveEvent.preventDefault();
                            
                            let newWidth = startWidth;
                            let newHeight = startHeight;
                            
                            // Calculate new dimensions based on handle position
                            if (position.includes('e')) {
                                newWidth = startWidth + (moveEvent.clientX - startX);
                            } else if (position.includes('w')) {
                                newWidth = startWidth - (moveEvent.clientX - startX);
                            }
                            
                            if (position.includes('s')) {
                                newHeight = startHeight + (moveEvent.clientY - startY);
                            } else if (position.includes('n')) {
                                newHeight = startHeight - (moveEvent.clientY - startY);
                            }
                            
                            // Maintain aspect ratio with Shift key
                            if (moveEvent.shiftKey) {
                                const ratio = startWidth / startHeight;
                                if (position.includes('n') || position.includes('s')) {
                                    newWidth = Math.round(newHeight * ratio);
                                } else {
                                    newHeight = Math.round(newWidth / ratio);
                                }
                            }
                            
                            // Apply minimum dimensions and ensure image doesn't exceed editor width
                            const maxAllowedWidth = this.editor.offsetWidth * 0.98; // 98% of editor width
                            
                            if (newWidth >= 30 && newHeight >= 30 && newWidth <= maxAllowedWidth) {
                                img.style.width = `${newWidth}px`;
                                img.style.height = `${newHeight}px`;
                                dimensions.textContent = `${newWidth} × ${newHeight}`;
                            }
                        };
                        
                        const onMouseUp = () => {
                            document.removeEventListener('mousemove', onMouseMove);
                            document.removeEventListener('mouseup', onMouseUp);
                            
                            // Re-enable contenteditable
                            this.editor.setAttribute('contenteditable', 'true');
                            
                            // Remove dimensions overlay
                            if (dimensions.parentNode) {
                                dimensions.parentNode.removeChild(dimensions);
                            }
                            
                            // Update content state
                            this.updateContent();
                        };
                        
                        document.addEventListener('mousemove', onMouseMove);
                        document.addEventListener('mouseup', onMouseUp);
                    });
                    
                    wrapper.appendChild(handle);
                });
            }
            
            // Add toolbar for image actions if not already added
            if (!wrapper.querySelector('.image-toolbar')) {
                const toolbar = document.createElement('div');
                toolbar.className = 'image-toolbar';
                toolbar.style.position = 'absolute';
                toolbar.style.top = '-30px';
                toolbar.style.left = '0';
                toolbar.style.right = '0';
                toolbar.style.height = '30px';
                toolbar.style.backgroundColor = '#f8f9fa';
                toolbar.style.borderRadius = '3px';
                toolbar.style.display = 'none';
                toolbar.style.alignItems = 'center';
                toolbar.style.justifyContent = 'center';
                toolbar.style.gap = '5px';
                toolbar.style.boxShadow = '0 1px 3px rgba(0,0,0,0.1)';
                toolbar.style.zIndex = '99';
                
                // Actions: Align left, center, right
                const alignLeftBtn = document.createElement('button');
                alignLeftBtn.type = 'button';
                alignLeftBtn.innerHTML = '<i class="bi bi-align-start"></i>';
                alignLeftBtn.title = 'Align Left';
                alignLeftBtn.style.border = 'none';
                alignLeftBtn.style.background = 'none';
                alignLeftBtn.style.cursor = 'pointer';
                alignLeftBtn.style.padding = '3px';
                alignLeftBtn.addEventListener('click', (e) => {
                    e.preventDefault();
                    e.stopPropagation();
                    
                    // Update the wrapper element to align image
                    wrapper.style.display = 'block';
                    wrapper.style.textAlign = 'left';
                    wrapper.style.margin = '5px auto 5px 0';
                    wrapper.style.width = '100%';
                    wrapper.style.clear = 'both'; // Clear floats
                    
                    img.style.display = 'inline-block';
                    img.style.float = 'left';
                    img.style.marginRight = '15px';
                    img.style.marginBottom = '10px';
                    
                    this.updateContent();
                });
                
                const alignCenterBtn = document.createElement('button');
                alignCenterBtn.type = 'button';
                alignCenterBtn.innerHTML = '<i class="bi bi-align-center"></i>';
                alignCenterBtn.title = 'Align Center';
                alignCenterBtn.style.border = 'none';
                alignCenterBtn.style.background = 'none';
                alignCenterBtn.style.cursor = 'pointer';
                alignCenterBtn.style.padding = '3px';
                alignCenterBtn.addEventListener('click', (e) => {
                    e.preventDefault();
                    e.stopPropagation();
                    
                    wrapper.style.display = 'block';
                    wrapper.style.textAlign = 'center';
                    wrapper.style.margin = '5px auto';
                    wrapper.style.width = '100%';
                    wrapper.style.clear = 'both'; // Clear floats
                    
                    img.style.display = 'inline-block';
                    img.style.float = 'none';
                    img.style.marginRight = '0';
                    img.style.marginBottom = '10px';
                    
                    this.updateContent();
                });
                
                const alignRightBtn = document.createElement('button');
                alignRightBtn.type = 'button';
                alignRightBtn.innerHTML = '<i class="bi bi-align-end"></i>';
                alignRightBtn.title = 'Align Right';
                alignRightBtn.style.border = 'none';
                alignRightBtn.style.background = 'none';
                alignRightBtn.style.cursor = 'pointer';
                alignRightBtn.style.padding = '3px';
                alignRightBtn.addEventListener('click', (e) => {
                    e.preventDefault();
                    e.stopPropagation();
                    
                    wrapper.style.display = 'block';
                    wrapper.style.textAlign = 'right';
                    wrapper.style.margin = '5px 0 5px auto';
                    wrapper.style.width = '100%';
                    wrapper.style.clear = 'both'; // Clear floats
                    
                    img.style.display = 'inline-block';
                    img.style.float = 'right';
                    img.style.marginLeft = '15px';
                    img.style.marginBottom = '10px';
                    
                    this.updateContent();
                });
                
                // Delete button
                const deleteBtn = document.createElement('button');
                deleteBtn.type = 'button';
                deleteBtn.innerHTML = '<i class="bi bi-trash"></i>';
                deleteBtn.title = 'Delete Image';
                deleteBtn.style.border = 'none';
                deleteBtn.style.background = 'none';
                deleteBtn.style.color = '#dc3545';
                deleteBtn.style.cursor = 'pointer';
                deleteBtn.style.padding = '3px';
                deleteBtn.addEventListener('click', (e) => {
                    e.preventDefault();
                    e.stopPropagation();
                    if (confirm('Are you sure you want to delete this image?')) {
                        wrapper.remove();
                        this.triggerContentChange();
                    }
                });
                
                // Add buttons to toolbar
                toolbar.appendChild(alignLeftBtn);
                toolbar.appendChild(alignCenterBtn);
                toolbar.appendChild(alignRightBtn);
                toolbar.appendChild(deleteBtn);
                wrapper.appendChild(toolbar);
                
                // Show/hide toolbar on hover
                wrapper.addEventListener('mouseenter', () => {
                    toolbar.style.display = 'flex';
                });
                
                wrapper.addEventListener('mouseleave', () => {
                    toolbar.style.display = 'none';
                });
            }
        });
        
        // Add CSS for handles if not already added
        if (!document.getElementById('blog-editor-image-styles')) {
            const styleElement = document.createElement('style');
            styleElement.id = 'blog-editor-image-styles';
            styleElement.textContent = `
                .blog-editor .image-wrapper:hover .resize-handle {
                    display: block;
                }
                .blog-editor .resize-handle {
                    display: none;
                    border-radius: 50%;
                }
                .blog-editor .resize-handle:hover {
                    transform: scale(1.5);
                }
                .blog-editor .resize-handle-n, 
                .blog-editor .resize-handle-s {
                    transform: translateX(-50%)!important;
                }
                .blog-editor .resize-handle-e, 
                .blog-editor .resize-handle-w {
                    transform: translateY(-50%)!important;
                }
                .blog-editor .video-container {
                    margin-bottom: 20px;
                    clear: both;
                    position: relative;
                    width: 100%;
                }
                .blog-editor .video-container:hover .video-toolbar {
                    display: flex;
                }
                .blog-editor code, .blog-editor pre {
                    background-color: #f8f9fa;
                    border-radius: 4px;
                    font-family: monospace;
                    white-space: pre-wrap;
                }
                .blog-editor pre {
                    padding: 10px;
                    overflow: auto;
                    max-width: 100%;
                    margin: 10px 0;
                }
                .blog-editor .latex-formula {
                    display: inline-block;
                    padding: 2px 4px;
                    max-width: 100%;
                    overflow-x: auto;
                }
                .blog-editor table {
                    width: 100%;
                    max-width: 100%;
                    margin-bottom: 1rem;
                    border-collapse: collapse;
                }
                .blog-editor table td, .blog-editor table th {
                    padding: 0.5rem;
                    border: 1px solid #dee2e6;
                }
                .blog-editor table th {
                    background-color: #f8f9fa;
                }
            `;
            document.head.appendChild(styleElement);
        }
    }

    /**
     * Insert a node at the current cursor position
     */
    insertNodeAtCursor(node) {
        const selection = window.getSelection();
        if (selection.rangeCount) {
            const range = selection.getRangeAt(0);
            range.deleteContents();
            range.insertNode(node);
            
            // Move cursor after the inserted node
            range.setStartAfter(node);
            range.setEndAfter(node);
            selection.removeAllRanges();
            selection.addRange(range);
        } else {
            // If no selection, append to the end
            this.editor.appendChild(node);
        }
    }

    /**
     * Clear editor content completely
     */
    clearContent() {
        // Clear editor content
        this.editor.innerHTML = '<p><br></p>';
        
        // Clear hidden input
        if (this.contentInput) {
            this.contentInput.value = '';
        }
        
        // Reset state
        this.state.content = '';
        this.state.isDirty = true;
        this.triggerContentChange();
        
        // Update counters
        this.updateCounters();
    }

    /**
     * Reset media elements in the editor and related fields
     */
    resetMedia() {
        // Remove all videos from editor
        const videos = this.editor.querySelectorAll('.video-container');
        videos.forEach(video => video.remove());
        
        // Clear VideoUrl field if exists
        const videoUrlInput = document.querySelector('#VideoUrl');
        if (videoUrlInput) {
            videoUrlInput.value = '';
        }
        
        // Trigger content change
        this.state.isDirty = true;
        this.triggerContentChange();
    }

    /**
     * Add CSS styles for editor
     */
    addEditorStyles() {
        // Add CSS for handles if not already added
        if (!document.getElementById('blog-editor-image-styles')) {
            const styleElement = document.createElement('style');
            styleElement.id = 'blog-editor-image-styles';
            styleElement.textContent = `
                .blog-editor .image-wrapper:hover .resize-handle {
                    display: block;
                }
                .blog-editor .resize-handle {
                    display: none;
                    border-radius: 50%;
                }
                .blog-editor .resize-handle:hover {
                    transform: scale(1.5);
                }
                .blog-editor .resize-handle-n, 
                .blog-editor .resize-handle-s {
                    transform: translateX(-50%)!important;
                }
                .blog-editor .resize-handle-e, 
                .blog-editor .resize-handle-w {
                    transform: translateY(-50%)!important;
                }
                .blog-editor .video-container {
                    margin-bottom: 20px;
                    clear: both;
                    position: relative;
                    width: 100%;
                }
                .blog-editor .video-container:hover .video-toolbar {
                    display: flex;
                }
                .blog-editor code, .blog-editor pre {
                    background-color: #f8f9fa;
                    border-radius: 4px;
                    font-family: monospace;
                    white-space: pre-wrap;
                }
                .blog-editor pre {
                    padding: 10px;
                    overflow: auto;
                    max-width: 100%;
                    margin: 10px 0;
                }
                .blog-editor .latex-formula {
                    display: inline-block;
                    padding: 2px 4px;
                    max-width: 100%;
                    overflow-x: auto;
                }
                .blog-editor table {
                    width: 100%;
                    max-width: 100%;
                    margin-bottom: 1rem;
                    border-collapse: collapse;
                }
                .blog-editor table td, .blog-editor table th {
                    padding: 0.5rem;
                    border: 1px solid #dee2e6;
                }
                .blog-editor table th {
                    background-color: #f8f9fa;
                }
                .blog-editor p:empty:after {
                    content: '\\200b'; /* Zero-width space to ensure empty paragraphs render */
                }
            `;
            document.head.appendChild(styleElement);
        }
    }
}

// Add global instance for initialization
window.initBlogEditor = function(options) {
    return new BlogEditor(options);
}; 