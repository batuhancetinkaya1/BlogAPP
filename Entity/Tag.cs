using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BlogApp.Entity;

public class Tag
{
    public Tag()
    {
        Name = string.Empty;
        Url = string.Empty;
        Posts = new List<Post>();
        CreatedAt = DateTime.UtcNow;
        IsActive = true;
        Color = TagColors.Secondary;
    }

    [Key]
    public int TagId { get; set; }

    [Required(ErrorMessage = "Etiket adı zorunludur")]
    [StringLength(50, ErrorMessage = "Etiket adı en fazla 50 karakter olabilir")]
    [Display(Name = "Etiket Adı")]
    public string Name { get; set; } = string.Empty;

    public string Url { get; set; } = string.Empty;

    [Display(Name = "Etiket Rengi")]
    public TagColors? Color { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public List<Post> Posts { get; set; } = new();
} 