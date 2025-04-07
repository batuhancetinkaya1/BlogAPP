using System;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace BlogApp.Entity;

public class Comment
{
    public Comment()
    {
        Content = string.Empty;
        CreatedAt = DateTime.UtcNow;
        Replies = new List<Comment>();
        Reactions = new List<CommentReaction>();
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

    // Yorum cevapları için
    public int? ParentCommentId { get; set; }
    public virtual Comment? ParentComment { get; set; }
    public virtual ICollection<Comment> Replies { get; set; }

    // Yorum beğenileri için
    public virtual ICollection<CommentReaction> Reactions { get; set; }
} 