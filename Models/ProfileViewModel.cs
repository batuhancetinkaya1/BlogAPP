using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace BlogApp.Models;

public class ProfileViewModel
{
    [Required(ErrorMessage = "Kullanıcı adı zorunludur.")]
    [StringLength(50, MinimumLength = 3, ErrorMessage = "Kullanıcı adı 3-50 karakter arasında olmalıdır.")]
    [Display(Name = "Kullanıcı Adı")]
    public string? UserName { get; set; }

    [Required(ErrorMessage = "E-posta adresi zorunludur.")]
    [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz.")]
    [Display(Name = "E-posta")]
    public string? Email { get; set; }

    [Display(Name = "Yeni Şifre")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "Şifre en az 6 karakter olmalıdır.")]
    [DataType(DataType.Password)]
    public string? NewPassword { get; set; }

    [Display(Name = "Yeni Şifre Tekrar")]
    [DataType(DataType.Password)]
    [Compare("NewPassword", ErrorMessage = "Şifreler eşleşmiyor.")]
    public string? ConfirmNewPassword { get; set; }

    [Display(Name = "Profil Resmi")]
    public string? Image { get; set; }

    [Display(Name = "Profil Resmi")]
    public IFormFile? ImageFile { get; set; }
} 