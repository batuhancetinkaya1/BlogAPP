/**
 * Blog Editor Initialization
 * Connects the editor to the form and initializes all necessary functionality
 */

document.addEventListener('DOMContentLoaded', function() {
    // Check if we're on a Create or Edit page
    const isCreatePage = window.location.pathname.includes('/Posts/Create');
    const isEditPage = window.location.pathname.includes('/Posts/Edit');
    
    if (!isCreatePage && !isEditPage) return;
    
    // Initialize MathJax for LaTeX rendering
    initMathJax();
    
    // Create editor container and toolbar
    setupEditorDOM();
    
    // Initialize the editor
    const editor = window.initBlogEditor({
        selector: '#blogEditor',
        contentSelector: '#Content',
        toolbarSelector: '#editorToolbar',
        charCountSelector: '#charCount',
        wordCountSelector: '#wordCount',
        uploadEndpoint: '/Posts/UploadImage',
        placeholder: 'Write your blog post content here...',
        autosaveInterval: 30000, // 30 seconds
    });
    
    // Initialize character counters for title and description
    initCharacterCounters();
    
    // Initialize Prism for code highlighting
    initCodeHighlighting();
    
    // Set up form submission handlers
    setupFormHandlers(editor);
    
    // Add message about autosave
    addAutosaveMessage();
    
    // Initialize editor fixes after all components are loaded
    setTimeout(() => {
        // Apply editor fixes via the newly created object
        if (window.editorFixes) {
            // Apply all available fixes
            if (window.editorFixes.fixImageUpload) window.editorFixes.fixImageUpload();
            if (window.editorFixes.fixVideoButtonIssue) window.editorFixes.fixVideoButtonIssue();
            if (window.editorFixes.fixFormulaCursorPosition) window.editorFixes.fixFormulaCursorPosition();
            if (window.editorFixes.fixLinkInsertion) window.editorFixes.fixLinkInsertion();
            if (window.editorFixes.fixImageToolbarsAndResizing) window.editorFixes.fixImageToolbarsAndResizing();
            
            // Process any images that might be in the editor
            if (window.editorFixes.processImages) window.editorFixes.processImages();
            
            console.log('Applied all editor fixes successfully');
        }
    }, 1000);
    
    console.log('Blog editor initialized with enhanced functionality');
});

/**
 * Initialize MathJax for LaTeX rendering
 */
function initMathJax() {
    if (typeof MathJax === 'undefined') {
        window.MathJax = {
            tex: {
                inlineMath: [['$', '$'], ['\\(', '\\)']],
                displayMath: [['$$', '$$'], ['\\[', '\\]']],
                processEscapes: true,
                processEnvironments: true
            },
            options: {
                skipHtmlTags: ['script', 'noscript', 'style', 'textarea', 'pre', 'code'],
                ignoreHtmlClass: 'tex2jax_ignore'
            }
        };
        
        const script = document.createElement('script');
        script.src = 'https://cdn.jsdelivr.net/npm/mathjax@3/es5/tex-mml-chtml.js';
        script.async = true;
        document.head.appendChild(script);
    }
}

/**
 * Set up editor DOM elements
 */
function setupEditorDOM() {
    const editorContainer = document.getElementById('Content')?.parentElement;
    if (!editorContainer) return;
    
    // Create editor container
    const blogEditor = document.createElement('div');
    blogEditor.id = 'blogEditor';
    blogEditor.className = 'blog-editor';
    blogEditor.style.minHeight = '400px';
    blogEditor.style.border = '1px solid #ced4da';
    blogEditor.style.borderRadius = '0.25rem';
    blogEditor.style.padding = '1rem';
    blogEditor.style.marginBottom = '1rem';
    
    // Create toolbar
    const editorToolbar = document.createElement('div');
    editorToolbar.id = 'editorToolbar';
    editorToolbar.className = 'editor-toolbar d-flex flex-wrap gap-1 mb-2 p-2 bg-light rounded border';
    
    // Create character counter
    const charCounter = document.createElement('div');
    charCounter.className = 'editor-counter d-flex justify-content-end text-muted small mt-2';
    charCounter.innerHTML = '<span id="charCount">0</span> karakter / <span id="wordCount">0</span> kelime';
    
    // Replace textarea with editor components
    editorContainer.appendChild(editorToolbar);
    editorContainer.appendChild(blogEditor);
    editorContainer.appendChild(charCounter);
    
    // Hide the original textarea but keep it for form submission
    const textarea = document.getElementById('Content');
    if (textarea) {
        textarea.style.display = 'none';
    }
}

/**
 * Initialize character counters for title and description
 */
function initCharacterCounters() {
    const titleInput = document.getElementById('Title');
    const descInput = document.getElementById('Description');
    
    // Title counter
    if (titleInput) {
        const titleContainer = titleInput.parentElement;
        if (titleContainer) {
            const titleCounter = document.createElement('div');
            titleCounter.className = 'form-text text-end';
            titleCounter.innerHTML = `<span id="titleCount">${titleInput.value.length}</span>/100`;
            titleContainer.appendChild(titleCounter);
            
            titleInput.addEventListener('input', function() {
                document.getElementById('titleCount').textContent = this.value.length;
                if (this.value.length > 100) {
                    titleCounter.classList.add('text-danger');
                } else {
                    titleCounter.classList.remove('text-danger');
                }
            });
        }
    }
    
    // Description counter
    if (descInput) {
        const descContainer = descInput.parentElement;
        if (descContainer) {
            const descCounter = document.createElement('div');
            descCounter.className = 'form-text text-end';
            descCounter.innerHTML = `<span id="descCount">${descInput.value.length}</span>/160`;
            descContainer.appendChild(descCounter);
            
            descInput.addEventListener('input', function() {
                document.getElementById('descCount').textContent = this.value.length;
                if (this.value.length > 160) {
                    descCounter.classList.add('text-danger');
                } else {
                    descCounter.classList.remove('text-danger');
                }
            });
        }
    }
}

/**
 * Initialize Prism for code highlighting
 */
function initCodeHighlighting() {
    if (typeof Prism === 'undefined') {
        // Add Prism CSS
        const prismCss = document.createElement('link');
        prismCss.rel = 'stylesheet';
        prismCss.href = 'https://cdn.jsdelivr.net/npm/prismjs@1.29.0/themes/prism.min.css';
        document.head.appendChild(prismCss);
        
        // Add Prism JS
        const prismJs = document.createElement('script');
        prismJs.src = 'https://cdn.jsdelivr.net/npm/prismjs@1.29.0/prism.min.js';
        document.head.appendChild(prismJs);
        
        // Add languages
        const prismComponents = document.createElement('script');
        prismComponents.src = 'https://cdn.jsdelivr.net/npm/prismjs@1.29.0/components/prism-javascript.min.js';
        document.head.appendChild(prismComponents);
    }
}

/**
 * Set up form handlers for saving content
 */
function setupFormHandlers(editor) {
    if (!editor) return;

    const form = document.getElementById('blogPostForm');
    if (!form) return;
    
    form.addEventListener('submit', function(e) {
        // Update the hidden textarea with the editor content
        const content = editor.getContent();
        const contentInput = document.getElementById('Content');
        
        if (contentInput && content) {
            contentInput.value = content;
        }
        
        // Validate content is not empty
        if (!contentInput.value.trim()) {
            e.preventDefault();
            alert('İçerik alanı boş olamaz. Lütfen bir şeyler yazın.');
            return false;
        }
        
        // Add a small spinner next to the submit button
        const submitBtn = e.submitter;
        if (submitBtn) {
            const originalText = submitBtn.innerHTML;
            submitBtn.innerHTML = '<span class="spinner-border spinner-border-sm me-2" role="status" aria-hidden="true"></span>' + originalText;
            submitBtn.disabled = true;
            
            // Enable the button after a timeout (in case form submission fails)
            setTimeout(() => {
                submitBtn.innerHTML = originalText;
                submitBtn.disabled = false;
            }, 5000);
        }
        
        return true;
    });
}

/**
 * Add autosave message
 */
function addAutosaveMessage() {
    const form = document.getElementById('blogPostForm');
    if (!form) return;
    
    const autosaveMsg = document.createElement('div');
    autosaveMsg.className = 'mt-2 text-muted small';
    autosaveMsg.innerHTML = '<i class="bi bi-info-circle"></i> İçerik her 30 saniyede bir otomatik olarak kaydedilir.';
    
    const buttons = form.querySelector('.d-flex.justify-content-between.mt-4');
    if (buttons) {
        buttons.parentNode.insertBefore(autosaveMsg, buttons.nextSibling);
    } else {
        form.appendChild(autosaveMsg);
    }
}

/**
 * Handle editor dark mode toggle
 */
document.addEventListener('blogEditorDarkModeToggle', function(e) {
    const darkMode = e.detail.darkMode;
    const toolbar = document.getElementById('editorToolbar');
    const counter = document.querySelector('.blog-editor-counter');
    
    if (toolbar) {
        toolbar.classList.toggle('dark-mode', darkMode);
    }
    
    if (counter) {
        counter.classList.toggle('dark-mode', darkMode);
    }
}); 