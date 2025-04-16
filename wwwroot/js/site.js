// Handle comment form submission
$(document).ready(function() {
    $('#comment-form').on('submit', function(e) {
        e.preventDefault();
        
        const $form = $(this);
        const $submitButton = $('#submit-comment');
        const $spinner = $submitButton.find('.spinner-border');
        const $alert = $('#comment-alert');
        
        // Show loading state
        $submitButton.prop('disabled', true);
        $spinner.removeClass('d-none');
        
        $.ajax({
            url: '/posts/AddComment',
            type: 'POST',
            data: {
                postId: $('#postId').val(),
                text: $('#commentText').val()
            },
            success: function(response) {
                if (response.success) {
                    // Add new comment to the list
                    const commentHtml = `
                        <div class="comment mb-3">
                            <div class="d-flex">
                                <img src="/img/${response.userImage}" class="rounded-circle me-2" alt="${response.userName}" style="width: 40px; height: 40px; object-fit: cover;">
                                <div>
                                    <h6 class="mb-0">${response.userName}</h6>
                                    <small class="text-muted">${new Date(response.publishedOn).toLocaleString('tr-TR')}</small>
                                    <p class="mb-0">${response.text}</p>
                                </div>
                            </div>
                        </div>
                    `;
                    
                    if ($('#comments-list .alert').length) {
                        $('#comments-list').empty();
                    }
                    
                    $('#comments-list').prepend(commentHtml);
                    
                    // Update comment count
                    const count = parseInt($('#comment-count').text()) + 1;
                    $('#comment-count').text(count);
                    
                    // Clear form and show success message
                    $form[0].reset();
                    $alert.removeClass('d-none alert-danger').addClass('alert-success')
                        .html('<i class="bi bi-check-circle me-2"></i>Yorumunuz başarıyla eklendi.');
                    
                    // Remove success message after 3 seconds
                    setTimeout(function() {
                        $alert.addClass('d-none');
                    }, 3000);
                } else {
                    $alert.removeClass('d-none alert-success').addClass('alert-danger')
                        .html('<i class="bi bi-exclamation-circle me-2"></i>Yorum eklenirken bir hata oluştu.');
                }
            },
            error: function() {
                $alert.removeClass('d-none alert-success').addClass('alert-danger')
                    .html('<i class="bi bi-exclamation-circle me-2"></i>Yorum eklenirken bir hata oluştu.');
            },
            complete: function() {
                // Reset loading state
                $submitButton.prop('disabled', false);
                $spinner.addClass('d-none');
            }
        });
    });
});

/**
 * Cookie utilities for handling cookies securely
 */
const CookieUtil = {
    /**
     * Set a cookie with secure attributes
     * @param {string} name - Cookie name
     * @param {string} value - Cookie value
     * @param {number} days - Expiration in days
     * @param {boolean} secure - Whether to set secure flag
     * @param {string} sameSite - SameSite attribute (Strict, Lax, None)
     */
    setCookie: function(name, value, days = 7, secure = true, sameSite = 'Strict') {
        let cookie = `${name}=${encodeURIComponent(value)}`;
        
        if (days) {
            const expiry = new Date();
            expiry.setDate(expiry.getDate() + days);
            cookie += `; expires=${expiry.toUTCString()}`;
        }
        
        cookie += '; path=/';
        
        if (secure) {
            cookie += '; secure';
        }
        
        cookie += `; samesite=${sameSite}`;
        
        document.cookie = cookie;
    },
    
    /**
     * Get a cookie value by name
     * @param {string} name - Cookie name
     * @returns {string|null} - Cookie value or null if not found
     */
    getCookie: function(name) {
        const nameEQ = `${name}=`;
        const cookies = document.cookie.split(';');
        
        for (let i = 0; i < cookies.length; i++) {
            let cookie = cookies[i].trim();
            if (cookie.indexOf(nameEQ) === 0) {
                return decodeURIComponent(cookie.substring(nameEQ.length));
            }
        }
        
        return null;
    },
    
    /**
     * Delete a cookie by name
     * @param {string} name - Cookie name
     */
    deleteCookie: function(name) {
        this.setCookie(name, '', -1);
    }
};

// Replace any direct document.cookie uses with these utility functions 