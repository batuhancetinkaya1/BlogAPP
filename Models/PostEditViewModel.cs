using System.ComponentModel.DataAnnotations;
using BlogApp.Entity;
using Microsoft.AspNetCore.Http;

namespace BlogApp.Models;

public class PostEditViewModel
{
    public int PostId { get; set; }

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

    [Display(Name = "Resim")]
    public IFormFile? ImageFile { get; set; }

    public string? Image { get; set; }

    public string Url { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;

    [Display(Name = "Etiketler")]
    public List<int> SelectedTagIds { get; set; } = new();

    [Display(Name = "Yayın Durumu")]
    public PostStatus Status { get; set; } = PostStatus.Draft;

    [Display(Name = "Planlanan Yayın Tarihi")]
    public DateTime? ScheduledPublishTime { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? PublishedOn { get; set; }

    public List<Tag> AvailableTags { get; set; } = new();
} 