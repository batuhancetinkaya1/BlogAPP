using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BlogApp.Entity;

public class User
{
    [Key]
    public int UserId { get; set; }

    [Required(ErrorMessage = "Kullanıcı adı zorunludur.")]
    [StringLength(50, MinimumLength = 3, ErrorMessage = "Kullanıcı adı 3-50 karakter arasında olmalıdır.")]
    public string UserName { get; set; } = string.Empty;

    [Required(ErrorMessage = "E-posta adresi zorunludur.")]
    [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Şifre zorunludur.")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "Şifre en az 6 karakter olmalıdır.")]
    public string Password { get; set; } = string.Empty;

    public string? Image { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public DateTime? LastLogin { get; set; }

    public bool IsActive { get; set; } = true;

    public bool IsAdmin { get; set; }

    public virtual ICollection<Post> Posts { get; set; } = new List<Post>();
    public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();
} 