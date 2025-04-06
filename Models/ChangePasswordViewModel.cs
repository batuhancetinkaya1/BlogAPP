using System.ComponentModel.DataAnnotations;

namespace BlogApp.Models;

public class ChangePasswordViewModel
{
    [Required(ErrorMessage = "Mevcut şifre alanı zorunludur")]
    [Display(Name = "Mevcut Şifre")]
    [DataType(DataType.Password)]
    public string CurrentPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "Yeni şifre alanı zorunludur")]
    [Display(Name = "Yeni Şifre")]
    [DataType(DataType.Password)]
    [MinLength(6, ErrorMessage = "Şifre en az 6 karakter olmalıdır")]
    public string NewPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "Şifre tekrar alanı zorunludur")]
    [Display(Name = "Yeni Şifre (Tekrar)")]
    [DataType(DataType.Password)]
    [Compare("NewPassword", ErrorMessage = "Şifreler eşleşmiyor")]
    public string ConfirmPassword { get; set; } = string.Empty;
}