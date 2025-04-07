using System;
using System.ComponentModel.DataAnnotations;

namespace BlogApp.Entity;

public class Comment
{
    public Comment()
    {
        Content = string.Empty;
        CreatedAt = DateTime.UtcNow;
    }

    [Key]
    public int CommentId { get; set; }

    [Required(ErrorMessage = "Yorum içeriği zorunludur")]
    [StringLength(1000, ErrorMessage = "Yorum en fazla 1000 karakter olabilir")]
    [Display(Name = "Yorum")]
    public string Content { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }

    public DateTime? PublishedOn { get; set; }

    public bool IsActive { get; set; } = true;

    public int PostId { get; set; }
    public virtual Post Post { get; set; } = null!;

    public int UserId { get; set; }
    public virtual User User { get; set; } = null!;
} 