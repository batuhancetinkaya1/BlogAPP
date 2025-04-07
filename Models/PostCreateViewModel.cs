using System.ComponentModel.DataAnnotations;
using BlogApp.Entity;

namespace BlogApp.Models;

public class PostCreateViewModel
{
    [Required(ErrorMessage = "Başlık alanı zorunludur")]
    [StringLength(100, ErrorMessage = "Başlık en fazla 100 karakter olabilir")]
    [Display(Name = "Başlık")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "İçerik alanı zorunludur")]
    [Display(Name = "İçerik")]
    public string Content { get; set; } = string.Empty;

    [StringLength(500, ErrorMessage = "Açıklama en fazla 500 karakter olabilir")]
    [Display(Name = "Açıklama (SEO için önemli)")]
    public string? Description { get; set; }

    [Display(Name = "Kapak Resmi")]
    public IFormFile? ImageFile { get; set; }

    [Display(Name = "YouTube Video URL (opsiyonel)")]
    public string? VideoUrl { get; set; }

    [Display(Name = "Özel URL")]
    public string Url { get; set; } = string.Empty;

    [Display(Name = "Aktif")]
    public bool IsActive { get; set; } = true;

    [Display(Name = "Taslak olarak kaydet")]
    public bool IsDraft { get; set; }

    [Display(Name = "Etiketler")]
    public List<int> SelectedTagIds { get; set; } = new();

    [Display(Name = "Yayın Durumu")]
    public PostStatus Status { get; set; } = PostStatus.Draft;

    [Display(Name = "Planlanan Yayın Tarihi")]
    public DateTime? ScheduledPublishTime { get; set; }

    [Display(Name = "SEO Anahtar Kelimeleri (virgülle ayırın)")]
    public string? Keywords { get; set; }

    [Display(Name = "Okunma Süresi (dakika)")]
    public int? ReadTime { get; set; }
} 