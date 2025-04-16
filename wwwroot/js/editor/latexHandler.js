/**
 * LaTeX Handler for BlogEditor
 * Handles rendering of mathematical formulas using MathJax
 */

class LaTeXHandler {
    constructor() {
        // Load MathJax if not already loaded
        this.loadMathJax();
        
        // Initialize observer to detect new LaTeX formulas
        this.initObserver();
        
        // Initialize editor content listener
        this.initContentListener();
        
        // Initialize key event listener for backspace on formulas
        this.initBackspaceHandler();
    }
    
    /**
     * Load MathJax if not already loaded
     */
    loadMathJax() {
        if (typeof MathJax !== 'undefined') {
            // MathJax is already loaded, just configure it
            this.configureMathJax();
            return;
        }
        
        // Set MathJax config
        window.MathJax = {
            tex: {
                inlineMath: [['$', '$'], ['\\(', '\\)']],
                displayMath: [['$$', '$$'], ['\\[', '\\]']],
                processEscapes: true
            },
            options: {
                enableMenu: false,
                renderActions: {
                    // Add a custom render action to style LaTeX containers
                    addStyleToLaTeX: [15, (doc) => {
                        for (const node of doc.getElementsByClassName('MathJax')) {
                            const parent = node.closest('.latex-formula');
                            if (parent) {
                                parent.style.display = 'block';
                                parent.style.margin = '1rem 0';
                                parent.style.padding = '0.5rem';
                                parent.style.backgroundColor = '#f8f9fa';
                                parent.style.borderLeft = '4px solid #0d6efd';
                                parent.style.borderRadius = '4px';
                                parent.style.overflowX = 'auto';
                            }
                        }
                    }, {}]
                }
            },
            startup: {
                pageReady: () => {
                    this.renderLaTeX();
                }
            }
        };
        
        // Load MathJax script
        const script = document.createElement('script');
        script.src = 'https://cdn.jsdelivr.net/npm/mathjax@3/es5/tex-mml-chtml.js';
        script.async = true;
        document.head.appendChild(script);
    }
    
    /**
     * Configure MathJax if it's already loaded
     */
    configureMathJax() {
        if (typeof MathJax === 'undefined') return;
        
        // Update configuration
        MathJax.config = {
            ...MathJax.config,
            tex: {
                inlineMath: [['$', '$'], ['\\(', '\\)']],
                displayMath: [['$$', '$$'], ['\\[', '\\]']],
                processEscapes: true
            },
            options: {
                ...MathJax.config.options,
                enableMenu: false
            }
        };
    }
    
    /**
     * Initialize observer to detect new LaTeX elements
     */
    initObserver() {
        // Create mutation observer to detect when LaTeX elements are added
        const observer = new MutationObserver((mutations) => {
            let shouldRender = false;
            
            mutations.forEach((mutation) => {
                if (mutation.type === 'childList' && mutation.addedNodes.length > 0) {
                    mutation.addedNodes.forEach((node) => {
                        // Check if node contains LaTeX
                        if (node.nodeType === Node.ELEMENT_NODE) {
                            if (node.textContent && (
                                node.textContent.includes('$$') || 
                                node.textContent.includes('$') ||
                                node.textContent.includes('\\(') ||
                                node.textContent.includes('\\[')
                            )) {
                                shouldRender = true;
                            }
                        }
                    });
                }
            });
            
            if (shouldRender) {
                this.renderLaTeX();
            }
        });
        
        // Start observing the editor
        const editor = document.querySelector('.blog-editor');
        if (editor) {
            observer.observe(editor, { childList: true, subtree: true });
        }
    }
    
    /**
     * Initialize content change listener
     */
    initContentListener() {
        document.addEventListener('blogEditorContentChange', () => {
            this.renderLaTeX();
        });
    }
    
    /**
     * Initialize backspace handler for formulas
     */
    initBackspaceHandler() {
        const editor = document.querySelector('.blog-editor');
        if (!editor) return;
        
        editor.addEventListener('keydown', (e) => {
            // Check if key is backspace or delete
            if (e.key === 'Backspace' || e.key === 'Delete') {
                const selection = window.getSelection();
                if (selection.rangeCount === 0) return;
                
                const range = selection.getRangeAt(0);
                if (!range.collapsed) return;
                
                // Check if we're right after or before a formula
                const formulaNode = this.findAdjacentFormulaNode(range);
                if (formulaNode) {
                    e.preventDefault();
                    formulaNode.remove();
                    
                    // Trigger content change event
                    document.dispatchEvent(new CustomEvent('blogEditorContentChange'));
                }
            }
        });
    }
    
    /**
     * Find adjacent formula node for backspace handling
     */
    findAdjacentFormulaNode(range) {
        // Check if there is a formula node right before the cursor for backspace
        let node = range.startContainer;
        
        // If we're in a text node, we need to check the previous sibling
        if (node.nodeType === Node.TEXT_NODE) {
            // Only check if cursor is at the beginning of the text node
            if (range.startOffset === 0) {
                // Check previous sibling
                const previousSibling = node.previousSibling;
                if (previousSibling && (
                    previousSibling.classList?.contains('latex-formula') ||
                    previousSibling.querySelector?.('.latex-formula')
                )) {
                    return previousSibling.classList?.contains('latex-formula') 
                        ? previousSibling 
                        : previousSibling.querySelector('.latex-formula');
                }
                
                // If we're at the start of a paragraph, check the previous sibling of the parent
                if (node.parentNode && node.parentNode.previousSibling) {
                    const parentPrevious = node.parentNode.previousSibling;
                    if (parentPrevious.classList?.contains('latex-formula') ||
                        parentPrevious.querySelector?.('.latex-formula')) {
                        return parentPrevious.classList?.contains('latex-formula')
                            ? parentPrevious
                            : parentPrevious.querySelector('.latex-formula');
                    }
                }
            }
        } 
        // If we're in an element node
        else if (node.nodeType === Node.ELEMENT_NODE) {
            // Check if we're inside a formula
            if (node.classList?.contains('latex-formula') || 
                node.closest?.('.latex-formula')) {
                return node.classList?.contains('latex-formula') 
                    ? node 
                    : node.closest('.latex-formula');
            }
            
            // Check if the previous sibling is a formula
            const previousSibling = node.previousSibling;
            if (previousSibling && (
                previousSibling.classList?.contains('latex-formula') ||
                previousSibling.querySelector?.('.latex-formula')
            )) {
                return previousSibling.classList?.contains('latex-formula')
                    ? previousSibling
                    : previousSibling.querySelector('.latex-formula');
            }
        }
        
        return null;
    }
    
    /**
     * Render all LaTeX formulas in the editor
     */
    renderLaTeX() {
        // Wait for MathJax to be fully loaded
        if (typeof MathJax === 'undefined' || !MathJax.typesetPromise) {
            setTimeout(() => this.renderLaTeX(), 500);
            return;
        }
        
        try {
            MathJax.typesetPromise()
                .then(() => {
                    console.log('MathJax typesetting completed');
                    this.styleLaTeXContainers();
                })
                .catch((err) => console.error('MathJax typesetting failed:', err));
        } catch (error) {
            console.error('Error rendering LaTeX:', error);
        }
    }
    
    /**
     * Add styles to LaTeX containers
     */
    styleLaTeXContainers() {
        const editor = document.querySelector('.blog-editor');
        if (!editor) return;
        
        // Style all LaTeX formula containers
        const formulas = editor.querySelectorAll('.latex-formula');
        formulas.forEach(formula => {
            formula.style.display = 'block';
            formula.style.margin = '1rem 0';
            formula.style.padding = '0.5rem';
            formula.style.backgroundColor = '#f8f9fa';
            formula.style.borderLeft = '4px solid #0d6efd';
            formula.style.borderRadius = '4px';
            formula.style.overflowX = 'auto';
            
            // Make formula position relative (but don't add edit button)
            formula.style.position = 'relative';
        });
        
        // Handle dark mode
        const isDarkMode = editor.classList.contains('dark-mode');
        if (isDarkMode) {
            formulas.forEach(formula => {
                formula.style.backgroundColor = '#343a40';
                formula.style.borderLeftColor = '#0d6efd';
            });
        }
    }
    
    /**
     * Insert a new LaTeX formula at cursor position
     */
    insertFormula(editor, formula = 'E = mc^2') {
        const latexHtml = `<span class="latex-formula">$$${formula}$$</span>`;
        document.execCommand('insertHTML', false, latexHtml);
        
        // Render the formula
        setTimeout(() => this.renderLaTeX(), 100);
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
        window.latexHandler = new LaTeXHandler();
        
        // Add event listener for LaTeX button
        const editor = document.querySelector('.blog-editor');
        if (editor) {
            // Listen for LaTeX button click
            document.addEventListener('blogEditorLatexInsert', (e) => {
                window.latexHandler.insertFormula(editor, e.detail?.formula);
            });
        }
    }, 1000);
}); 