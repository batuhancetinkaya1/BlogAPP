using System.ComponentModel.DataAnnotations;

namespace BlogApp.Models;

public class UserEditViewModel
{
    public int UserId { get; set; }

    [Required(ErrorMessage = "Kullanıcı adı zorunludur")]
    [Display(Name = "Kullanıcı Adı")]
    public string UserName { get; set; } = string.Empty;

    [Required(ErrorMessage = "E-posta adresi zorunludur")]
    [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz")]
    [Display(Name = "E-posta")]
    public string Email { get; set; } = string.Empty;

    [Display(Name = "Admin")]
    public bool IsAdmin { get; set; }

    [Display(Name = "Aktif")]
    public bool IsActive { get; set; }
} 