using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BlogApp.Entity;

public enum PostStatus
{
    Draft,
    Scheduled,
    Published,
    Archived
}

public class Post
{
    public Post()
    {
        Title = string.Empty;
        Content = string.Empty;
        Description = string.Empty;
        Url = string.Empty;
        Image = string.Empty;
        CreatedAt = DateTime.UtcNow;
        IsActive = true;
        Tags = new List<Tag>();
        Comments = new List<Comment>();
        Reactions = new List<PostReaction>();
    }

    [Key]
    public int PostId { get; set; }

    [Required(ErrorMessage = "Başlık alanı zorunludur")]
    [StringLength(100, ErrorMessage = "Başlık en fazla 100 karakter olabilir")]
    public string Title { get; set; }

    [Required(ErrorMessage = "İçerik alanı zorunludur")]
    public string Content { get; set; }

    [StringLength(500, ErrorMessage = "Açıklama en fazla 500 karakter olabilir")]
    public string Description { get; set; }

    [Required(ErrorMessage = "URL zorunludur.")]
    [StringLength(200, MinimumLength = 5, ErrorMessage = "URL 5-200 karakter arasında olmalıdır.")]
    public string Url { get; set; }

    public string Image { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? PublishedOn { get; set; }

    public DateTime? ScheduledPublishTime { get; set; }

    public PostStatus Status { get; set; }

    public bool IsActive { get; set; }

    public int UserId { get; set; }
    public virtual User User { get; set; } = null!;

    public virtual ICollection<Tag> Tags { get; set; }
    public virtual ICollection<Comment> Comments { get; set; }
    public virtual ICollection<PostReaction> Reactions { get; set; }
} 