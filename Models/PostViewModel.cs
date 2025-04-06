using System.ComponentModel.DataAnnotations;
using BlogApp.Entity;

namespace BlogApp.Models;

public class PostViewModel
{
    public IQueryable<Post> Posts { get; set; } = null!;
    public Post Post { get; set; } = null!;
    public string TagUrl { get; set; } = string.Empty;
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPosts { get; set; }
    public int PostId { get; set; }
    public int TotalPages { get; set; }
    public bool HasPreviousPage { get; set; }
    public bool HasNextPage { get; set; }

    [Required(ErrorMessage = "Başlık alanı zorunludur")]
    [StringLength(100, ErrorMessage = "Başlık en fazla 100 karakter olabilir")]
    [Display(Name = "Başlık")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "İçerik alanı zorunludur")]
    [Display(Name = "İçerik")]
    public string Content { get; set; } = string.Empty;

    [StringLength(500, ErrorMessage = "Açıklama en fazla 500 karakter olabilir")]
    [Display(Name = "Açıklama")]
    public string? Description { get; set; }

    public string? Image { get; set; }

    public string Url { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;

    public bool IsDraft { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? PublishedOn { get; set; }

    public DateTime? ScheduledPublishTime { get; set; }

    public PostStatus Status { get; set; }

    public int UserId { get; set; }

    public User? User { get; set; }

    public List<Tag> Tags { get; set; } = new();

    public List<Comment> Comments { get; set; } = new();
} 