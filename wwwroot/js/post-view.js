/**
 * Post View Script
 * Handles MathJax rendering and other post view functionality
 */

document.addEventListener('DOMContentLoaded', function() {
    // Add CSS for LaTeX formulas
    addFormulaStyles();
    
    // Initialize MathJax for LaTeX rendering in post views
    initMathJaxForPostView();
    
    // Initialize code highlighting
    initCodeHighlighting();
    
    // Add styles for proper post view layout and hide image toolbars
    addPostViewStyles();
    
    // Fix post layout issues
    fixPostLayoutIssues();
    
    // Initialize video monitoring with a delay to ensure content is rendered
    setTimeout(monitorAndFixVideos, 1000);
    
    // Run again after a longer delay to catch any dynamically loaded content
    setTimeout(monitorAndFixVideos, 3000);
    
    console.log('Post view initialized successfully');
});

/**
 * Add CSS styles for LaTeX formulas
 */
function addFormulaStyles() {
    const style = document.createElement('style');
    style.textContent = `
        /* Inline formulas */
        .latex-formula[data-mode="inline"] {
            display: inline-flex !important;
            align-items: center !important;
            margin: 0 3px !important;
            padding: 0 !important;
            vertical-align: baseline !important;
            white-space: nowrap !important;
            line-height: normal !important;
            border-left: none !important;
            position: relative !important;
            transform: translateY(-1px) !important;
            overflow: visible !important;
        }
        
        .latex-formula[data-mode="inline"] .MathJax {
            display: inline !important;
            margin: 0 !important;
            padding: 0 !important;
            vertical-align: middle !important;
            overflow: visible !important;
        }

        /* Force MathJax to align with text properly */
        .MJX-TEX {
            vertical-align: middle !important;
            overflow: visible !important;
        }
        
        /* Block formulas */
        .latex-formula[data-mode="block"] {
            display: block !important;
            margin: 1.5rem 0 !important;
            padding: 1rem !important;
            background-color: #f9fafb !important;
            border-radius: 6px !important;
            overflow-x: auto !important;
            overflow-y: hidden !important;
            border: 1px solid #e9ecef !important;
            box-shadow: 0 1px 3px rgba(0,0,0,0.05) !important;
            max-width: 100% !important;
        }
        
        /* Hide scrollbars but allow scrolling for block formulas if needed */
        .latex-formula[data-mode="block"]::-webkit-scrollbar {
            height: 0;
            width: 0;
        }
        
        /* Hide scrollbars in Firefox */
        .latex-formula[data-mode="block"] {
            scrollbar-width: none;
        }
        
        /* Dark mode support */
        .dark-mode .latex-formula[data-mode="block"] {
            background-color: #2d3339 !important;
            border-color: #4a5056 !important;
            box-shadow: 0 1px 3px rgba(0,0,0,0.2) !important;
        }
        
        /* Adjust paragraph spacing around formulas */
        p:has(.latex-formula[data-mode="inline"]) {
            margin-bottom: 1rem !important;
        }
    `;
    document.head.appendChild(style);
}

/**
 * Initialize MathJax for LaTeX rendering in post views
 */
function initMathJaxForPostView() {
    // Check if we're on the post detail page
    const isPostDetailPage = window.location.pathname.match(/\/posts\/[^\/]+$/i) !== null;
    
    if (!isPostDetailPage) return;
    
    console.log('Initializing MathJax for post view...');
    
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
            },
            startup: {
                pageReady: () => {
                    console.log('MathJax is ready, typesetting page...');
                    // Initial typeset of the page
                    MathJax.typesetPromise()
                        .then(() => {
                            // Process formulas after typesetting
                            processFormulasAfterRendering();
                        })
                        .catch(err => {
                            console.error('MathJax typesetting error:', err);
                        });
                }
            }
        };
        
        // Load MathJax script
        const script = document.createElement('script');
        script.src = 'https://cdn.jsdelivr.net/npm/mathjax@3/es5/tex-mml-chtml.js';
        script.async = true;
        document.head.appendChild(script);
        
        console.log('MathJax script added to document');
    } else {
        // MathJax is already loaded, process the page
        if (MathJax.typesetPromise) {
            console.log('MathJax already loaded, processing page content...');
            MathJax.typesetPromise()
                .then(() => {
                    // Process formulas after typesetting
                    processFormulasAfterRendering();
                })
                .catch(err => {
                    console.error('MathJax typesetting error:', err);
                });
        }
    }
    
    // Set up a small delay to ensure all content is loaded before rendering
    setTimeout(() => {
        // Pre-process formulas to identify inline vs. block
        prepareFormulas();
        
        if (typeof MathJax !== 'undefined' && MathJax.typesetPromise) {
            console.log('Running delayed MathJax typesetting...');
            MathJax.typesetPromise()
                .then(() => {
                    // Process formulas after typesetting
                    processFormulasAfterRendering();
                })
                .catch(err => {
                    console.error('MathJax delayed typesetting error:', err);
                });
        }
    }, 500);
}

/**
 * Prepare LaTeX formulas by identifying inline vs. block formulas
 * before MathJax processing
 */
function prepareFormulas() {
    // Find all LaTeX content in the document
    const blogContent = document.querySelector('.blog-content');
    if (!blogContent) return;
    
    // Process text nodes to find and wrap LaTeX formulas
    processTextNodesForLaTeX(blogContent);
    
    // Find all formula elements and prepare them
    const formulas = document.querySelectorAll('.latex-formula');
    
    formulas.forEach(formula => {
        const content = formula.textContent || '';
        
        // Check if formula is inline or block
        let isInline = true;
        
        // Block formulas start and end with $$ or \[ and \]
        if ((content.startsWith('$$') && content.endsWith('$$')) || 
            (content.startsWith('\\[') && content.endsWith('\\]'))) {
            isInline = false;
        }
        // Inline formulas start and end with single $ or \( and \)
        else if ((content.startsWith('$') && content.endsWith('$') && !content.startsWith('$$')) || 
                 (content.startsWith('\\(') && content.endsWith('\\)'))) {
            isInline = true;
        }
        
        // Mark the formula with the appropriate display mode
        formula.setAttribute('data-mode', isInline ? 'inline' : 'block');
        
        // Add appropriate CSS for display mode
        if (isInline) {
            formula.style.display = 'inline';
            formula.style.margin = '0 2px';
            formula.style.whiteSpace = 'nowrap';
            formula.style.verticalAlign = 'middle';
        } else {
            formula.style.display = 'block';
            formula.style.margin = '1rem 0';
            formula.style.maxWidth = '100%';
            formula.style.overflowX = 'auto';
        }
    });

    // Add mobile-specific handling for formulas
    adjustFormulasForMobile();
    
    // Listen for window resize to adjust formulas
    window.addEventListener('resize', debounce(adjustFormulasForMobile, 250));
}

/**
 * Adjust formulas for mobile devices
 */
function adjustFormulasForMobile() {
    const isMobile = window.innerWidth < 768;
    const blockFormulas = document.querySelectorAll('.latex-formula[data-mode="block"]');
    
    blockFormulas.forEach(formula => {
        if (isMobile) {
            formula.style.overflowX = 'auto';
            formula.style.overflowY = 'hidden';
            formula.style.maxWidth = '100%';
            formula.style.padding = '0.75rem';
            formula.style.margin = '1rem 0';
            formula.style.fontSize = '90%';
            
            // Hide scrollbars but keep functionality
            if (CSS.supports('scrollbar-width', 'none')) {
                formula.style.scrollbarWidth = 'none'; // Firefox
            }
            // For WebKit browsers
            const styleSheet = document.createElement('style');
            styleSheet.textContent = `
                .latex-formula[data-mode="block"]::-webkit-scrollbar {
                    height: 0;
                    width: 0;
                }
            `;
            document.head.appendChild(styleSheet);
        } else {
            formula.style.overflowX = 'auto';
            formula.style.overflowY = 'hidden';
            formula.style.maxWidth = '100%';
            formula.style.padding = '1rem';
            formula.style.margin = '1.5rem 0';
            formula.style.fontSize = '100%';
        }
    });
    
    const inlineFormulas = document.querySelectorAll('.latex-formula[data-mode="inline"]');
    inlineFormulas.forEach(formula => {
        if (isMobile) {
            // For mobile, ensure inline formulas can break if needed
            formula.style.display = 'inline-flex';
            formula.style.alignItems = 'center';
            formula.style.maxWidth = '100%';
            formula.style.overflow = 'visible';
            formula.style.verticalAlign = 'middle';
            formula.style.fontSize = '95%';
            
            // Get all MathJax elements inside this formula
            const mathJaxElements = formula.querySelectorAll('.MathJax');
            mathJaxElements.forEach(mathJax => {
                mathJax.style.maxWidth = '100%';
                mathJax.style.overflow = 'visible';
                mathJax.style.fontSize = '95%';
            });
        } else {
            formula.style.display = 'inline-flex';
            formula.style.alignItems = 'center';
            formula.style.maxWidth = '';
            formula.style.whiteSpace = 'nowrap';
            formula.style.overflow = 'visible';
            formula.style.fontSize = '100%';
            
            // Reset MathJax elements
            const mathJaxElements = formula.querySelectorAll('.MathJax');
            mathJaxElements.forEach(mathJax => {
                mathJax.style.maxWidth = '';
                mathJax.style.overflow = 'visible';
                mathJax.style.fontSize = '100%';
            });
        }
    });
}

/**
 * Simple debounce function to limit function calls
 */
function debounce(func, wait) {
    let timeout;
    return function() {
        const context = this;
        const args = arguments;
        clearTimeout(timeout);
        timeout = setTimeout(() => func.apply(context, args), wait);
    };
}

/**
 * Process all text nodes to find and wrap LaTeX formulas
 */
function processTextNodesForLaTeX(element) {
    // Skip if this is a .latex-formula element (already processed)
    if (element.classList && element.classList.contains('latex-formula')) {
        return;
    }
    
    // Process child nodes
    const childNodes = [...element.childNodes];
    
    childNodes.forEach(node => {
        // If it's a text node, process it for LaTeX
        if (node.nodeType === Node.TEXT_NODE) {
            const text = node.textContent;
            
            // Check for LaTeX delimiters
            if ((text.includes('$') || text.includes('\\(') || text.includes('\\[')) && 
                (text.includes('$') || text.includes('\\)') || text.includes('\\]'))) {
                
                // Replace the text node with processed content
                const fragment = wrapLaTeXInTextNode(text);
                if (fragment) {
                    node.parentNode.replaceChild(fragment, node);
                }
            }
        } 
        // If it's an element, process its children recursively
        else if (node.nodeType === Node.ELEMENT_NODE) {
            processTextNodesForLaTeX(node);
        }
    });
}

/**
 * Wrap LaTeX code in text with appropriate span elements
 */
function wrapLaTeXInTextNode(text) {
    if (!text.includes('$') && !text.includes('\\(') && !text.includes('\\[')) {
        return null;
    }
    
    const fragment = document.createDocumentFragment();
    
    // Pattern to match block LaTeX first (to avoid overlap with inline)
    // Important: process block formulas first to avoid issues with nested delimiters
    const patterns = [
        { regex: /\$\$(.*?)\$\$/gs, isInline: false },   // $$...$$ (with 's' flag for multiline)
        { regex: /\\\[(.*?)\\\]/gs, isInline: false },   // \[...\] (with 's' flag for multiline)
        { regex: /\$([^\$]+?)\$/g, isInline: true },     // $...$ (non-greedy, avoid nested $ signs)
        { regex: /\\\((.*?)\\\)/g, isInline: true }      // \(...\)
    ];
    
    // Create a working copy of the text
    let remainingText = text;
    
    // Process each formula pattern
    for (const pattern of patterns) {
        // Split the text by the formula pattern and process
        const parts = remainingText.split(pattern.regex);
        
        if (parts.length > 1) { // If any matches found
            remainingText = ''; // Reset the remaining text
            
            for (let i = 0; i < parts.length; i++) {
                if (i % 2 === 0) {
                    // This is text before/between/after formulas - save for next iteration
                    remainingText += parts[i];
                } else {
                    // This is a formula - add the text before it from the saved text
                    if (remainingText) {
                        fragment.appendChild(document.createTextNode(remainingText));
                        remainingText = '';
                    }
                    
                    // Create the formula element with the right delimiters
                    const formula = document.createElement('span');
                    formula.className = 'latex-formula';
                    formula.setAttribute('data-mode', pattern.isInline ? 'inline' : 'block');
                    
                    // Add appropriate delimiters back
                    let formulaText = '';
                    if (pattern.isInline) {
                        formulaText = pattern.regex.toString().includes('\\\\(') 
                            ? `\\(${parts[i]}\\)` 
                            : `$${parts[i]}$`;
                    } else {
                        formulaText = pattern.regex.toString().includes('\\\\[') 
                            ? `\\[${parts[i]}\\]` 
                            : `$$${parts[i]}$$`;
                    }
                    
                    formula.textContent = formulaText;
                    fragment.appendChild(formula);
                }
            }
            
            // Add any remaining text
            if (remainingText) {
                fragment.appendChild(document.createTextNode(remainingText));
            }
            
            // Create a new text node for the next pattern to process
            const tempDiv = document.createElement('div');
            tempDiv.appendChild(fragment.cloneNode(true));
            remainingText = tempDiv.innerHTML;
        }
    }
    
    // If we didn't process any formulas, return the original text
    if (remainingText === text) {
        fragment.appendChild(document.createTextNode(text));
    }
    // Otherwise, parse the remaining HTML and add to fragment
    else if (remainingText !== '') {
        const tempDiv = document.createElement('div');
        tempDiv.innerHTML = remainingText;
        
        while (tempDiv.firstChild) {
            fragment.appendChild(tempDiv.firstChild);
        }
    }
    
    return fragment;
}

/**
 * Process formulas after MathJax has rendered them
 */
function processFormulasAfterRendering() {
    const mathJaxElements = document.querySelectorAll('.MathJax');
    mathJaxElements.forEach(mathJax => {
        const formula = mathJax.closest('.latex-formula');
        if (formula) {
            const isInline = formula.getAttribute('data-mode') === 'inline';
            
            // Apply different styles based on inline vs. block
            if (isInline) {
                formula.style.display = 'inline-flex';
                formula.style.alignItems = 'center';
                formula.style.margin = '0 3px';
                formula.style.padding = '0';
                formula.style.verticalAlign = 'baseline';
                formula.style.whiteSpace = 'nowrap';
                formula.style.lineHeight = 'normal';
                formula.style.borderLeft = 'none';
                formula.style.position = 'relative';
                formula.style.transform = 'translateY(-1px)';
                formula.style.overflow = 'visible';
                
                mathJax.style.display = 'inline';
                mathJax.style.margin = '0';
                mathJax.style.verticalAlign = 'middle';
                mathJax.style.overflow = 'visible';
                
                // Find MJX-TEX elements and adjust their alignment
                const mjxTexElements = mathJax.querySelectorAll('.MJX-TEX');
                mjxTexElements.forEach(el => {
                    el.style.verticalAlign = 'middle';
                    el.style.overflow = 'visible';
                });
            } else {
                formula.style.display = 'block';
                formula.style.margin = '1.5rem 0';
                formula.style.padding = '1rem';
                formula.style.maxWidth = '100%';
                formula.style.overflowX = 'auto';
                formula.style.overflowY = 'hidden';
                
                // Hide scrollbars but keep functionality
                if (CSS.supports('scrollbar-width', 'none')) {
                    formula.style.scrollbarWidth = 'none'; // Firefox
                }
                
                // Check if dark mode is enabled
                const isDarkMode = document.body.classList.contains('dark-mode') || 
                                   document.documentElement.classList.contains('dark-mode');
                
                // Apply appropriate styling based on theme
                if (isDarkMode) {
                    formula.style.backgroundColor = '#2d3339';
                    formula.style.border = '1px solid #4a5056';
                    formula.style.color = '#f8f9fa';
                    formula.style.boxShadow = '0 1px 3px rgba(0,0,0,0.2)';
                } else {
                    formula.style.backgroundColor = '#f9fafb';
                    formula.style.border = '1px solid #e9ecef';
                    formula.style.boxShadow = '0 1px 3px rgba(0,0,0,0.05)';
                }
                
                formula.style.borderRadius = '6px';
            }
        }
    });
    
    // Handle dark mode toggle if it happens after rendering
    const darkModeObserver = new MutationObserver(mutations => {
        mutations.forEach(mutation => {
            if (mutation.type === 'attributes' && 
                (mutation.attributeName === 'class' || mutation.attributeName === 'data-theme')) {
                
                const isDarkMode = document.body.classList.contains('dark-mode') || 
                                   document.documentElement.classList.contains('dark-mode');
                
                // Update all block formulas
                const blockFormulas = document.querySelectorAll('.latex-formula[data-mode="block"]');
                blockFormulas.forEach(formula => {
                    if (isDarkMode) {
                        formula.style.backgroundColor = '#2d3339';
                        formula.style.border = '1px solid #4a5056';
                        formula.style.color = '#f8f9fa';
                        formula.style.boxShadow = '0 1px 3px rgba(0,0,0,0.2)';
                    } else {
                        formula.style.backgroundColor = '#f9fafb';
                        formula.style.border = '1px solid #e9ecef';
                        formula.style.color = '';
                        formula.style.boxShadow = '0 1px 3px rgba(0,0,0,0.05)';
                    }
                });
            }
        });
    });
    
    // Observe both body and html for class changes (dark mode toggle)
    darkModeObserver.observe(document.body, { attributes: true });
    darkModeObserver.observe(document.documentElement, { attributes: true });
}

/**
 * Initialize code highlighting for post content
 */
function initCodeHighlighting() {
    // Check if we're on a post detail page
    const isPostDetailPage = window.location.pathname.match(/\/posts\/[^\/]+$/i) !== null;
    
    if (!isPostDetailPage) return;
    
    // Fix Prism extend issues preemptively
    fixPrismExtendBug();
    
    // Find all code blocks in post content
    const codeBlocks = document.querySelectorAll('pre code');
    
    // Apply syntax highlighting if Prism is available
    if (typeof Prism !== 'undefined') {
        // Process each code block
        codeBlocks.forEach(codeBlock => {
            // Get language class from code block
            let langClass = Array.from(codeBlock.classList).find(cls => cls.startsWith('language-'));
            
            // If no language class is specified, default to 'language-javascript'
            if (!langClass) {
                codeBlock.classList.add('language-javascript');
            }
            
            try {
                // Highlight the code
                Prism.highlightElement(codeBlock);
            } catch (error) {
                console.error('Error during syntax highlighting:', error);
            }
        });
    } else {
        // Prism is not available, we'll style code blocks manually
        const codeStyle = document.createElement('style');
        codeStyle.textContent = `
            pre {
                background-color: #f5f5f5;
                padding: 1rem;
                border-radius: 4px;
                overflow-x: auto;
                border: 1px solid #e0e0e0;
            }
            
            code {
                font-family: 'Consolas', 'Monaco', 'Courier New', monospace;
                font-size: 0.9em;
                line-height: 1.5;
            }
        `;
        document.head.appendChild(codeStyle);
    }
    
    // Load Prism if not already loaded
    if (typeof Prism === 'undefined') {
        loadPrismScriptsAndStyles();
    }
}

/**
 * Fix the Prism extend bug that causes "Cannot set properties of undefined"
 */
function fixPrismExtendBug() {
    if (typeof Prism !== 'undefined' && Prism.util && Prism.util.extend) {
        // Create a safe wrapper for the extend method
        const originalExtend = Prism.util.extend;
        
        Prism.util.extend = function(obj, properties) {
            // Ensure the object exists
            obj = obj || {};
            return originalExtend(obj, properties);
        };
        
        // Fix already loaded languages
        Object.keys(Prism.languages).forEach(lang => {
            if (typeof Prism.languages[lang] === 'object' && Prism.languages[lang] !== null) {
                // Fix any undefined properties
                ['tokenize', 'rest', 'tokens', 'hooks'].forEach(prop => {
                    if (Prism.languages[lang][prop] === undefined) {
                        Prism.languages[lang][prop] = {};
                    }
                });
            }
        });
    }
}

/**
 * Load Prism scripts and styles dynamically
 */
function loadPrismScriptsAndStyles() {
    // Add Prism CSS
    const prismCss = document.createElement('link');
    prismCss.rel = 'stylesheet';
    prismCss.href = '/lib/prism/prism.css';
    document.head.appendChild(prismCss);
    
    // Load Prism core with deferred loading
    const prismScript = document.createElement('script');
    prismScript.src = '/lib/prism/prism.js';
    prismScript.defer = true;
    prismScript.onload = function() {
        // Fix the extend bug when Prism loads
        fixPrismExtendBug();
        
        // Load additional language components
        const languages = ['javascript', 'css', 'csharp', 'markup', 'python', 'sql'];
        languages.forEach(lang => {
            const script = document.createElement('script');
            script.src = `/lib/prism/components/prism-${lang}.min.js`;
            script.defer = true;
            script.onload = function() {
                // Fix bugs in this language component
                fixPrismExtendBug();
            };
            document.head.appendChild(script);
        });
        
        // Re-highlight code blocks after Prism is loaded
        setTimeout(() => {
            if (typeof Prism !== 'undefined') {
                Prism.highlightAll();
            }
        }, 500);
    };
    
    document.head.appendChild(prismScript);
}

/**
 * Add styles for proper post view layout and hide image toolbars
 */
function addPostViewStyles() {
    const style = document.createElement('style');
    style.id = 'post-view-styles';
    style.textContent = `
        /* Post content container */
        .post-detail,
        .post-content,
        .blog-post-detail {
            position: relative;
            display: block;
            width: 100%;
            overflow: hidden; /* Contain floats */
        }
        
        /* Images in post */
        .post-detail img,
        .post-content img {
            max-width: 100%;
            height: auto;
        }
        
        /* Image wrapper */
        .image-wrapper {
            margin-bottom: 1.5rem;
            max-width: 100%;
        }
        
        /* Float clearing for image wrappers */
        .image-wrapper.align-left,
        .image-wrapper.align-right {
            margin-bottom: 1rem;
        }
        
        .image-wrapper.align-left {
            float: left;
            margin-right: 1rem;
        }
        
        .image-wrapper.align-right {
            float: right;
            margin-left: 1rem;
        }
        
        .image-wrapper.align-center {
            display: flex;
            justify-content: center;
            margin-left: auto;
            margin-right: auto;
        }
        
        /* Hide all editor controls in post view */
        .post-content .image-toolbar,
        .post-content .resize-handle,
        .post-content .image-dimensions,
        .post-content .crop-button,
        .post-detail .image-toolbar,
        .post-detail .resize-handle,
        .post-detail .image-dimensions,
        .post-detail .crop-button,
        body:not(.blog-editing-active) .image-toolbar,
        body:not(.blog-editing-active) .resize-handle,
        body:not(.blog-editing-active) .image-dimensions,
        body:not(.blog-editing-active) .crop-button {
            display: none !important;
            visibility: hidden !important;
            opacity: 0 !important;
            pointer-events: none !important;
        }
        
        /* Post interaction elements (likes, comments) */
        .post-actions,
        .post-interaction,
        .post-footer {
            display: block;
            width: 100%;
            clear: both;
            margin-top: 2rem;
            padding-top: 1rem;
            border-top: 1px solid #e9ecef;
            overflow: hidden; /* Contain floats */
        }
        
        /* Like buttons container */
        .like-buttons-container,
        .post-reactions,
        .post-likes,
        [class*="like-container"] {
            display: flex;
            gap: 0.5rem;
            margin-bottom: 1rem;
        }
        
        /* Comments section */
        .comments-section, 
        .post-comments,
        [class*="comment-section"] {
            clear: both;
            margin-top: 1.5rem;
            border-top: 1px solid #e9ecef;
            padding-top: 1.5rem;
        }
        
        /* Clearfix utility */
        .clearfix::after {
            content: "";
            display: table;
            clear: both;
        }
    `;
    document.head.appendChild(style);
}

/**
 * Fix post layout issues using proper DOM manipulation
 */
function fixPostLayoutIssues() {
    // Wait for DOM to be fully loaded
    setTimeout(() => {
        // Find the post content container
        const postContainer = document.querySelector('.post-detail, .post-content, .blog-post-detail, article.post');
        if (!postContainer) return;
        
        // Add clearfix to post container
        postContainer.classList.add('clearfix');
        
        // Ensure proper layout structure
        restructurePostElements(postContainer);
        
        // Force hide all editor controls
        hideEditorControls();
        
        // Find and restructure post interaction elements
        restructureInteractionElements();
        
        // Add resize observer to handle dynamic content changes
        setupResizeObserver();
    }, 500);
}

/**
 * Restructure post elements for proper layout
 */
function restructurePostElements(container) {
    // Fix image wrappers
    const imageWrappers = container.querySelectorAll('.image-wrapper');
    imageWrappers.forEach(wrapper => {
        // Ensure proper alignment class is applied
        if (wrapper.style.float === 'left') {
            wrapper.classList.add('align-left');
        } else if (wrapper.style.float === 'right') {
            wrapper.classList.add('align-right');
        } else if (wrapper.style.textAlign === 'center' || 
                  wrapper.style.margin?.includes('auto')) {
            wrapper.classList.add('align-center');
        }
        
        // Clear any interfering inline styles
        if (wrapper.classList.contains('align-center')) {
            wrapper.style.float = 'none';
            wrapper.style.clear = 'both';
        }
    });
    
    // Add spacing between paragraphs and images
    const paragraphs = container.querySelectorAll('p');
    paragraphs.forEach(p => {
        if (p.previousElementSibling && 
            (p.previousElementSibling.classList?.contains('image-wrapper') || 
             p.previousElementSibling.tagName === 'IMG')) {
            p.style.clear = 'both';
        }
    });
}

/**
 * Hide all editor controls
 */
function hideEditorControls() {
    const editorControls = document.querySelectorAll('.image-toolbar, .resize-handle, .image-dimensions, .crop-button');
    editorControls.forEach(control => {
        control.style.display = 'none';
        control.style.visibility = 'hidden';
        control.style.opacity = '0';
        control.style.position = 'absolute';
        control.style.pointerEvents = 'none';
        control.setAttribute('aria-hidden', 'true');
    });
}

/**
 * Restructure post interaction elements
 */
function restructureInteractionElements() {
    // Find post interaction container
    const interactionContainer = document.querySelector('.post-actions, .post-interaction, .post-footer');
    if (!interactionContainer) {
        // Try to find like buttons without a container
        const likeButtons = document.querySelectorAll('[class*="like"], [class*="begen"]');
        if (likeButtons.length > 0) {
            // Get the parent of the first like button
            const parent = likeButtons[0].parentElement;
            
            // Create a proper container
            const container = document.createElement('div');
            container.className = 'post-actions clearfix';
            
            // Create a like buttons container
            const likesContainer = document.createElement('div');
            likesContainer.className = 'like-buttons-container';
            
            // Move all like buttons to the container
            likeButtons.forEach(button => {
                // Clone to avoid issues with moving nodes
                const clone = button.cloneNode(true);
                likesContainer.appendChild(clone);
                // Remove the original if successful
                if (clone.parentElement === likesContainer) {
                    button.parentElement.removeChild(button);
                }
            });
            
            // Add containers to DOM
            container.appendChild(likesContainer);
            
            // Find comments section
            const commentsSection = document.querySelector('.comments-section, [class*="comment"], [class*="yorum"]');
            if (commentsSection) {
                // Insert container before comments
                commentsSection.parentElement.insertBefore(container, commentsSection);
                
                // Add clearfix before comments
                const clearDiv = document.createElement('div');
                clearDiv.className = 'clearfix';
                commentsSection.parentElement.insertBefore(clearDiv, commentsSection);
                
                // Add class to comments section
                commentsSection.classList.add('comments-section');
                commentsSection.style.clear = 'both';
            } else {
                // Append to post container
                const postContainer = document.querySelector('.post-detail, .post-content, .blog-post-detail');
                if (postContainer) {
                    postContainer.appendChild(container);
                }
            }
        }
    } else {
        // Add clearfix to existing container
        interactionContainer.classList.add('clearfix');
        interactionContainer.style.clear = 'both';
        
        // Find like buttons
        const likeButtons = interactionContainer.querySelectorAll('[class*="like"], [class*="begen"]');
        if (likeButtons.length > 0) {
            // Check if they're already in a container
            if (!likeButtons[0].closest('.like-buttons-container')) {
                // Create a container for like buttons
                const likesContainer = document.createElement('div');
                likesContainer.className = 'like-buttons-container';
                
                // Move buttons to container
                likeButtons.forEach(button => {
                    // Only move if not already in a good container
                    if (!button.closest('.like-buttons-container')) {
                        likesContainer.appendChild(button);
                    }
                });
                
                // Add container to interaction container
                interactionContainer.insertBefore(likesContainer, interactionContainer.firstChild);
            }
        }
    }
    
    // Ensure comments section has clearfix
    const commentsSection = document.querySelector('.comments-section, [class*="comment"], [class*="yorum"]');
    if (commentsSection) {
        commentsSection.classList.add('comments-section');
        commentsSection.style.clear = 'both';
    }
}

/**
 * Set up resize observer to handle dynamic content changes
 */
function setupResizeObserver() {
    if (typeof ResizeObserver !== 'undefined') {
        const postContainer = document.querySelector('.post-detail, .post-content, .blog-post-detail');
        if (postContainer) {
            const observer = new ResizeObserver(entries => {
                // Re-apply fixes when container size changes
                entries.forEach(entry => {
                    if (entry.target === postContainer) {
                        hideEditorControls();
                    }
                });
            });
            
            observer.observe(postContainer);
        }
    }
}

/**
 * Fix Prism.js and other JavaScript errors
 */
function fixJavaScriptErrors() {
    // Fix "Cannot set properties of undefined (setting 'class-name')" error in Prism
    try {
        // Create a global safety wrapper for Prism
        if (typeof Prism !== 'undefined') {
            // Create a safe extend method for Prism
            const originalExtend = Prism.util.extend;
            Prism.util.extend = function(obj, properties) {
                // Check if obj is undefined/null and provide a fallback empty object
                obj = obj || {};
                return originalExtend(obj, properties);
            };
            
            // Fix any already-loaded language definitions
            for (const lang in Prism.languages) {
                if (Prism.languages.hasOwnProperty(lang)) {
                    if (Prism.languages[lang] && typeof Prism.languages[lang] === 'object') {
                        // Make sure it has a tokens property
                        Prism.languages[lang].tokens = Prism.languages[lang].tokens || {};
                    }
                }
            }
            
            console.log('Applied Prism.js error fixes');
        }
    } catch (e) {
        console.error('Error applying Prism fixes:', e);
    }
    
    // Fix CORS issues with external resources
    fixCrossOriginIssues();
}

/**
 * Fix cross-origin issues with external resources
 */
function fixCrossOriginIssues() {
    // Add crossorigin attribute to all external scripts
    document.querySelectorAll('script[src^="http"]').forEach(script => {
        if (!script.hasAttribute('crossorigin')) {
            script.setAttribute('crossorigin', 'anonymous');
        }
    });
    
    // Fix YouTube embeds to use youtube-nocookie.com
    document.querySelectorAll('iframe[src*="youtube.com"]').forEach(iframe => {
        const src = iframe.src;
        if (src && src.includes('youtube.com') && !src.includes('youtube-nocookie.com')) {
            iframe.src = src.replace('youtube.com', 'youtube-nocookie.com');
        }
        
        // Add title for accessibility
        if (!iframe.hasAttribute('title')) {
            iframe.setAttribute('title', 'YouTube video player');
        }
        
        // Add loading="lazy" for performance
        if (!iframe.hasAttribute('loading')) {
            iframe.setAttribute('loading', 'lazy');
        }
    });
    
    // Add referrer policy meta tag if not exists
    if (!document.querySelector('meta[name="referrer"]')) {
        const meta = document.createElement('meta');
        meta.name = 'referrer';
        meta.content = 'strict-origin-when-cross-origin';
        document.head.appendChild(meta);
    }
    
    // Set SameSite cookie attributes
    try {
        // Create a helper function to set SameSite cookies
        window.setSameSiteCookie = function(name, value, days) {
            let expires = '';
            if (days) {
                const date = new Date();
                date.setTime(date.getTime() + (days * 24 * 60 * 60 * 1000));
                expires = '; expires=' + date.toUTCString();
            }
            
            // Add SameSite and Secure attributes
            document.cookie = name + '=' + (value || '') + expires + '; path=/; SameSite=Lax; Secure';
        };
    } catch (e) {
        console.error('Error setting up cookie handler:', e);
    }
    
    // Add favicon if missing
    if (!document.querySelector('link[rel="icon"]')) {
        const favicon = document.createElement('link');
        favicon.rel = 'icon';
        favicon.type = 'image/png';
        favicon.href = 'data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAACAAAAAgCAMAAABEpIrGAAAABGdBTUEAALGPC/xhBQAAAAFzUkdCAK7OHOkAAAAJcEhZcwAACxMAAAsTAQCanBgAAABjUExURUdwTAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAF8yqVEAAAAgdFJOUwD1CsX5Ko7iFWdZ3jMJhhHRsdp+LSB3BpwEQ1E4azdxR3+hxgAAALlJREFUOMvNkVcOwzAMBKXuvffm/S/JD8KyI1EG9puxNMPhLkAgJMf1AzJr+TY3o/XN7cif/BS/TXbOO3YJVjsHFJnrHBHk7jkgyFwV5oFBxFATHCUYyqCaIDsEObFvMJImBTkKij0gK8I8QJGdreDYn8DD/c4IvEp1CJ6wZwWuKVoEGlM0ZhWZYYYGTXaYYNMEmyZYnv30WCAoRkJQjLhWC3TfjwpEuQCJKBfQXUBLqwXa93+CL9syGyvyIrPKAAAAAElFTkSuQmCC';
        document.head.appendChild(favicon);
    }
}

/**
 * Detect and fix any hidden or incorrectly embedded videos
 */
function monitorAndFixVideos() {
    // Find all video containers
    const videoContainers = document.querySelectorAll('.video-container');
    
    // Check if there are any video containers
    if (videoContainers.length > 0) {
        console.log(`Found ${videoContainers.length} video container(s) in the page`);
        
        // Process each video container
        videoContainers.forEach((container, index) => {
            // Find the iframe
            const iframe = container.querySelector('iframe');
            
            if (iframe) {
                // Log the video source
                console.log(`Video ${index + 1}: ${iframe.src}`);
                
                // Check if video is visible
                const isVisible = isElementVisible(container);
                if (!isVisible) {
                    console.warn(`Video container ${index + 1} is not visible. Fixing display properties...`);
                    
                    // Fix visibility issues
                    container.style.display = 'block';
                    container.style.visibility = 'visible';
                    container.style.opacity = '1';
                    
                    // Make sure parent containers are visible too
                    let parent = container.parentElement;
                    while (parent && parent !== document.body) {
                        parent.style.display = parent.style.display === 'none' ? 'block' : parent.style.display;
                        parent.style.visibility = 'visible';
                        parent.style.opacity = '1';
                        parent = parent.parentElement;
                    }
                }
                
                // Use youtube-nocookie.com domain
                if (iframe.src.includes('youtube.com') && !iframe.src.includes('youtube-nocookie.com')) {
                    console.log(`Converting YouTube URL to privacy-enhanced mode for video ${index + 1}`);
                    iframe.src = iframe.src.replace('youtube.com', 'youtube-nocookie.com');
                }
                
                // Ensure proper attributes for accessibility and performance
                if (!iframe.hasAttribute('title')) {
                    iframe.setAttribute('title', 'YouTube video player');
                }
                
                if (!iframe.hasAttribute('loading')) {
                    iframe.setAttribute('loading', 'lazy');
                }
                
                // Remove any toolbars that shouldn't be visible in view mode
                const toolbar = container.querySelector('.video-toolbar');
                if (toolbar) {
                    console.log(`Removing editor toolbar from video ${index + 1}`);
                    toolbar.remove();
                }
            } else {
                console.warn(`Video container ${index + 1} has no iframe. It may be incorrectly structured.`);
            }
        });
    } else {
        console.log('No video containers found in the page');
        
        // Check for lone iframes that might be videos
        const iframes = document.querySelectorAll('iframe[src*="youtube"]');
        if (iframes.length > 0) {
            console.log(`Found ${iframes.length} YouTube iframe(s) outside of video containers`);
            
            // Process each iframe
            iframes.forEach((iframe, index) => {
                console.log(`Fixing YouTube iframe ${index + 1}`);
                
                // Create proper video container structure
                const videoContainer = document.createElement('div');
                videoContainer.className = 'video-container';
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
                
                // Set iframe properties
                iframe.style.position = 'absolute';
                iframe.style.top = '0';
                iframe.style.left = '0';
                iframe.style.width = '100%';
                iframe.style.height = '100%';
                
                // Use youtube-nocookie.com domain
                if (iframe.src.includes('youtube.com') && !iframe.src.includes('youtube-nocookie.com')) {
                    iframe.src = iframe.src.replace('youtube.com', 'youtube-nocookie.com');
                }
                
                // Ensure proper attributes
                if (!iframe.hasAttribute('title')) {
                    iframe.setAttribute('title', 'YouTube video player');
                }
                
                if (!iframe.hasAttribute('loading')) {
                    iframe.setAttribute('loading', 'lazy');
                }
                
                // Replace the iframe with the properly structured video container
                const parent = iframe.parentNode;
                parent.insertBefore(videoContainer, iframe);
                wrapper.appendChild(iframe);
                videoContainer.appendChild(wrapper);
            });
        }
    }
}

/**
 * Check if an element is visible in the DOM
 */
function isElementVisible(element) {
    if (!element) return false;
    
    const style = window.getComputedStyle(element);
    
    return !(
        style.display === 'none' ||
        style.visibility === 'hidden' ||
        style.opacity === '0' ||
        element.offsetWidth === 0 ||
        element.offsetHeight === 0
    );
}

// Initialize the fixes at the end of the DOMContentLoaded event
// Add to our existing DOMContentLoaded listener by wrapping it
const originalDOMContentLoaded = document.onreadystatechange;
document.onreadystatechange = function() {
    if (document.readyState === 'interactive' || document.readyState === 'complete') {
        // Apply our JavaScript error fixes
        setTimeout(fixJavaScriptErrors, 100);
        
        // Call original handler if it exists
        if (typeof originalDOMContentLoaded === 'function') {
            originalDOMContentLoaded();
        }
    }
};

// Also add a direct initialization when the script loads
setTimeout(fixJavaScriptErrors, 100); 