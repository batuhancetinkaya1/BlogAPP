/**
 * Video Editor Modal Test Suite
 * 
 * This script tests the video insertion functionality to ensure
 * it works properly and validates our fixes.
 */

// Test VideoUrl extraction function
function testVideoUrlExtraction() {
    console.log('Testing YouTube URL extraction...');
    
    // Ensure editor is initialized
    if (!window.editor || !window.editor.extractYouTubeId) {
        console.error('Editor not initialized. Cannot test YouTube URL extraction.');
        return;
    }
    
    const testCases = [
        { url: 'https://www.youtube.com/watch?v=dQw4w9WgXcQ', expected: 'dQw4w9WgXcQ' },
        { url: 'https://youtu.be/dQw4w9WgXcQ', expected: 'dQw4w9WgXcQ' },
        { url: 'https://www.youtube.com/embed/dQw4w9WgXcQ', expected: 'dQw4w9WgXcQ' },
        { url: 'https://www.youtube.com/v/dQw4w9WgXcQ', expected: 'dQw4w9WgXcQ' },
        { url: 'dQw4w9WgXcQ', expected: 'dQw4w9WgXcQ' }, // Just the ID
        { url: 'https://www.youtube.com/watch?v=dQw4w9WgXcQ&t=123', expected: 'dQw4w9WgXcQ' },
        { url: 'invalid url', expected: null },
        { url: '', expected: null }
    ];
    
    let passed = 0;
    let failed = 0;
    
    testCases.forEach(testCase => {
        const result = window.editor.extractYouTubeId(testCase.url);
        if (result === testCase.expected) {
            console.log(`✅ PASS: "${testCase.url}" -> "${result}"`);
            passed++;
        } else {
            console.error(`❌ FAIL: "${testCase.url}" -> "${result}" (expected "${testCase.expected}")`);
            failed++;
        }
    });
    
    console.log(`${passed} tests passed, ${failed} tests failed.`);
    return { passed, failed };
}

// Test modal functionality
function testVideoModal() {
    console.log('Testing video modal functionality...');
    
    if (!window.editor || !window.editor.insertVideo) {
        console.error('Editor not initialized. Cannot test video modal.');
        return;
    }
    
    // Test with a valid URL
    window.editor.insertVideo('https://www.youtube.com/watch?v=dQw4w9WgXcQ');
    
    // Check if modal is visible
    setTimeout(() => {
        const modal = document.getElementById('videoInsertModal');
        if (!modal) {
            console.error('❌ FAIL: Video modal not found in DOM.');
            return;
        }
        
        if (!modal.classList.contains('show')) {
            console.error('❌ FAIL: Video modal is not showing.');
            return;
        }
        
        console.log('✅ PASS: Video modal is showing correctly.');
        
        // Check if URL was prefilled
        const urlInput = modal.querySelector('#videoUrl');
        if (!urlInput || urlInput.value !== 'https://www.youtube.com/watch?v=dQw4w9WgXcQ') {
            console.error('❌ FAIL: URL input not correctly prefilled.');
            return;
        }
        
        console.log('✅ PASS: URL input prefilled correctly.');
        
        // Check if preview is visible
        const preview = modal.querySelector('#videoPreviewContainer');
        if (!preview || preview.classList.contains('d-none')) {
            console.error('❌ FAIL: Video preview not visible.');
            return;
        }
        
        console.log('✅ PASS: Video preview is visible.');
        
        // Check if insert button is enabled
        const insertBtn = modal.querySelector('#insertVideoBtn');
        if (!insertBtn || insertBtn.disabled) {
            console.error('❌ FAIL: Insert button not enabled.');
            return;
        }
        
        console.log('✅ PASS: Insert button is enabled.');
        
        // Test closing the modal
        setTimeout(() => {
            console.log('Testing modal close functionality...');
            // Try both close button selectors - the combined button should work with both classes
            const closeBtn = modal.querySelector('.btn-close') || modal.querySelector('.close');
            if (closeBtn) {
                closeBtn.click();
                
                // Check if modal is closed
                setTimeout(() => {
                    if (modal.classList.contains('show')) {
                        console.error('❌ FAIL: Modal did not close properly.');
                        
                        // Emergency cleanup
                        if (window.cleanupModalState) {
                            window.cleanupModalState();
                            console.log('Emergency modal cleanup applied.');
                        }
                    } else {
                        console.log('✅ PASS: Modal closed successfully.');
                    }
                }, 500);
            } else {
                console.error('❌ FAIL: Close button not found.');
            }
        }, 1000);
    }, 500);
}

// Run tests when page loads
document.addEventListener('DOMContentLoaded', function() {
    // Allow some time for the editor to initialize
    setTimeout(() => {
        if (window.editor) {
            console.log('Starting video functionality tests...');
            testVideoUrlExtraction();
            
            // Only run modal test if URL extraction passed
            setTimeout(testVideoModal, 1000);
        } else {
            console.error('Editor not initialized. Cannot run tests.');
        }
    }, 2000);
}); 