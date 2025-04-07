using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace BlogApp.Models.ViewModels;

public class ProfileUpdateModel
{
    [Required(ErrorMessage = "Kullanıcı adı zorunludur")]
    [StringLength(50, MinimumLength = 3, ErrorMessage = "Kullanıcı adı 3-50 karakter arasında olmalıdır")]
    public string UserName { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "E-posta zorunludur")]
    [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz")]
    public string Email { get; set; } = string.Empty;
    
    public IFormFile? Image { get; set; }
    
    [StringLength(100, MinimumLength = 6, ErrorMessage = "Şifre en az 6 karakter olmalıdır")]
    public string? Password { get; set; }
} 