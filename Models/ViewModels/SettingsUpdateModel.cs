using System.ComponentModel.DataAnnotations;

namespace BlogApp.Models.ViewModels;

public class SettingsUpdateModel
{
    [Required(ErrorMessage = "E-posta adresi zorunludur")]
    [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz")]
    public string Email { get; set; } = string.Empty;
} 