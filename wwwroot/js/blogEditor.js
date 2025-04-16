// Find the insertVideo method and fix the implementation
function insertVideo() {
    // Create a modal for video insertion
    const modal = document.createElement('div');
    modal.className = 'video-modal';
    modal.id = 'videoModal';
    modal.setAttribute('role', 'dialog');
    modal.setAttribute('aria-labelledby', 'videoModalTitle');
    modal.innerHTML = `
        <div class="video-modal-content">
            <div class="video-modal-header">
                <h5 id="videoModalTitle">Video Ekle</h5>
                <button type="button" class="close-button" aria-label="Kapat">×</button>
            </div>
            <div class="video-modal-body">
                <div class="form-group">
                    <label for="videoUrl">YouTube Video URL</label>
                    <input type="url" class="form-control" id="videoUrl" name="videoUrl" 
                           placeholder="https://www.youtube.com/watch?v=..." required 
                           aria-required="true" aria-describedby="videoUrlHelp">
                    <small id="videoUrlHelp" class="form-text">YouTube video URL'sini girin (örn. https://www.youtube.com/watch?v=abcdef123)</small>
                </div>
                <div class="video-preview-container" style="display: none;">
                    <h6>Önizleme:</h6>
                    <div class="video-preview"></div>
                </div>
            </div>
            <div class="video-modal-footer">
                <button type="button" class="btn btn-secondary cancel-button">İptal</button>
                <button type="button" class="btn btn-primary insert-button" disabled>Ekle</button>
            </div>
        </div>
    `;
    document.body.appendChild(modal);

    // Add event listeners
    const closeButton = modal.querySelector('.close-button');
    const cancelButton = modal.querySelector('.cancel-button');
    const insertButton = modal.querySelector('.insert-button');
    const videoUrlInput = modal.querySelector('#videoUrl');
    const previewContainer = modal.querySelector('.video-preview-container');
    const previewDiv = modal.querySelector('.video-preview');

    // Function to close the modal
    const closeModal = () => {
        document.body.removeChild(modal);
    };

    // Function to extract YouTube ID from URL
    const extractYouTubeId = (url) => {
        const regExp = /^.*((youtu.be\/)|(v\/)|(\/u\/\w\/)|(embed\/)|(watch\?))\??v?=?([^#&?]*).*/;
        const match = url.match(regExp);
        return (match && match[7].length === 11) ? match[7] : false;
    };

    // Function to update preview
    const updatePreview = () => {
        const url = videoUrlInput.value.trim();
        const videoId = extractYouTubeId(url);

        if (videoId) {
            previewContainer.style.display = 'block';
            previewDiv.innerHTML = `
                <iframe width="100%" height="250" 
                    src="https://www.youtube-nocookie.com/embed/${videoId}" 
                    title="YouTube video player" 
                    frameborder="0"
                    allow="accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture" 
                    allowfullscreen
                    loading="lazy"></iframe>
            `;
            insertButton.disabled = false;
        } else {
            previewContainer.style.display = 'none';
            previewDiv.innerHTML = '';
            insertButton.disabled = true;
        }
    };

    // Event listeners
    closeButton.addEventListener('click', closeModal);
    cancelButton.addEventListener('click', closeModal);
    videoUrlInput.addEventListener('input', updatePreview);
    videoUrlInput.addEventListener('paste', () => {
        // Short delay to allow paste to complete
        setTimeout(updatePreview, 100);
    });

    // Handle insert button click
    insertButton.addEventListener('click', () => {
        const url = videoUrlInput.value.trim();
        const videoId = extractYouTubeId(url);

        if (videoId) {
            // Check if we're currently focused in the editor
            const isEditorFocused = document.activeElement && 
                                   (document.activeElement.id === 'editor' || 
                                    document.activeElement.closest('#editor'));
            
            // Force focus back to editor if necessary - this ensures video is inserted in content
            if (!isEditorFocused) {
                document.getElementById('editor').focus();
            }
            
            // Get the current selection range
            const selection = window.getSelection();
            const range = selection.getRangeAt(0);
            
            // Make sure we're inserting in the editor
            const editorElement = document.getElementById('editor');
            if (!editorElement.contains(range.commonAncestorContainer)) {
                // Force insert at the end of the editor
                range.setStart(editorElement, editorElement.childNodes.length);
                range.setEnd(editorElement, editorElement.childNodes.length);
                selection.removeAllRanges();
                selection.addRange(range);
            }
            
            // Create video container with responsive wrapper
            const videoContainer = document.createElement('div');
            videoContainer.className = 'video-container';
            videoContainer.contentEditable = 'false';
            videoContainer.style.position = 'relative';
            videoContainer.style.width = '100%';
            videoContainer.style.marginBottom = '20px';
            videoContainer.style.clear = 'both';

            // Create toolbar
            const toolbar = document.createElement('div');
            toolbar.className = 'video-toolbar';
            toolbar.innerHTML = `
                <button type="button" class="video-delete-btn" title="Videoyu sil">
                    <i class="bi bi-trash"></i>
                </button>
            `;
            videoContainer.appendChild(toolbar);

            // Create responsive wrapper
            const wrapper = document.createElement('div');
            wrapper.style.position = 'relative';
            wrapper.style.paddingBottom = '56.25%'; // 16:9 aspect ratio
            wrapper.style.height = '0';
            wrapper.style.overflow = 'hidden';
            wrapper.style.maxWidth = '100%';

            // Create iframe
            const iframe = document.createElement('iframe');
            iframe.width = '100%';
            iframe.height = '100%';
            iframe.src = `https://www.youtube-nocookie.com/embed/${videoId}`;
            iframe.title = 'YouTube video player';
            iframe.frameBorder = '0';
            iframe.allow = 'accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture';
            iframe.allowFullscreen = true;
            iframe.loading = 'lazy';
            iframe.style.position = 'absolute';
            iframe.style.top = '0';
            iframe.style.left = '0';
            iframe.style.width = '100%';
            iframe.style.height = '100%';

            // Assemble the video container
            wrapper.appendChild(iframe);
            videoContainer.appendChild(wrapper);

            // Insert at cursor position
            try {
                // Delete any existing selection
                range.deleteContents();
                
                // Insert the video container
                range.insertNode(videoContainer);
                
                // Set cursor after the container
                range.setStartAfter(videoContainer);
                range.setEndAfter(videoContainer);
                selection.removeAllRanges();
                selection.addRange(range);
                
                // Add a new paragraph after the video if needed
                const nextElement = videoContainer.nextElementSibling;
                if (!nextElement || 
                    (nextElement.tagName !== 'P' && 
                     nextElement.tagName !== 'DIV' && 
                     !nextElement.classList.contains('video-container'))) {
                    const paragraph = document.createElement('p');
                    paragraph.innerHTML = '<br>';
                    videoContainer.after(paragraph);
                    
                    // Move cursor to the new paragraph
                    range.setStart(paragraph, 0);
                    range.setEnd(paragraph, 0);
                    selection.removeAllRanges();
                    selection.addRange(range);
                }
                
                // Store the video URL in a hidden field or data attribute for form submission
                const videoUrlField = document.getElementById('VideoUrl');
                if (videoUrlField) {
                    videoUrlField.value = url;
                }
            } catch (error) {
                console.error('Error inserting video:', error);
                
                // Fallback: just append to the end of editor
                document.getElementById('editor').appendChild(videoContainer);
            }
            
            // Initialize delete button event
            const deleteBtn = videoContainer.querySelector('.video-delete-btn');
            deleteBtn.addEventListener('click', () => {
                videoContainer.remove();
                // Update hidden field if needed
                const videoUrlField = document.getElementById('VideoUrl');
                if (videoUrlField && videoUrlField.value === url) {
                    videoUrlField.value = '';
                }
            });

            // Close the modal
            closeModal();
        }
    });

    // Show the modal
    modal.style.display = 'block';
    
    // Auto-focus URL input
    setTimeout(() => {
        videoUrlInput.focus();
    }, 100);
} 