using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using BlogApp.Models;

namespace BlogApp.Helpers
{
    public static class ImageHelper
    {
        public const int MaxFileSize = 2 * 1024 * 1024; // 2MB
        public static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png" };

        public static string GetProfileImageUrl(string? imagePath)
        {
            if (string.IsNullOrEmpty(imagePath))
            {
                return "/img/profiles/default.jpg";
            }

            return imagePath.StartsWith("http") ? imagePath : imagePath;
        }

        public static async Task<string> ValidateAndSaveProfileImageAsync(IFormFile imageFile)
        {
            if (imageFile == null || imageFile.Length == 0)
                throw new ArgumentException("Geçersiz dosya.");

            var extension = Path.GetExtension(imageFile.FileName).ToLowerInvariant();
            if (!AllowedExtensions.Contains(extension))
                throw new ArgumentException("Sadece JPG ve PNG dosyalarına izin verilmektedir.");

            if (imageFile.Length > MaxFileSize)
                throw new ArgumentException("Dosya boyutu 2MB'ı geçemez.");

            var fileName = $"{Guid.NewGuid()}{extension}";
            var filePath = Path.Combine("wwwroot", "img", "profiles", fileName);

            Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await imageFile.CopyToAsync(stream);
            }

            return $"/img/profiles/{fileName}";
        }

        public static async Task<string> ValidateAndSavePostImageAsync(IFormFile imageFile)
        {
            if (imageFile == null || imageFile.Length == 0)
                throw new ArgumentException("Geçersiz dosya.");

            var extension = Path.GetExtension(imageFile.FileName).ToLowerInvariant();
            if (!AllowedExtensions.Contains(extension))
                throw new ArgumentException("Sadece JPG ve PNG dosyalarına izin verilmektedir.");

            if (imageFile.Length > MaxFileSize)
                throw new ArgumentException("Dosya boyutu 2MB'ı geçemez.");

            var fileName = $"{Guid.NewGuid()}{extension}";
            var filePath = Path.Combine("wwwroot", "img", "posts", fileName);

            Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await imageFile.CopyToAsync(stream);
            }

            return $"/img/posts/{fileName}";
        }

        public static async Task<string> ValidateAndSaveContentImageAsync(IFormFile imageFile)
        {
            if (imageFile == null || imageFile.Length == 0)
                throw new ArgumentException("Geçersiz dosya.");

            var extension = Path.GetExtension(imageFile.FileName).ToLowerInvariant();
            if (!AllowedExtensions.Contains(extension))
                throw new ArgumentException("Sadece JPG ve PNG dosyalarına izin verilmektedir.");

            if (imageFile.Length > MaxFileSize)
                throw new ArgumentException("Dosya boyutu 2MB'ı geçemez.");

            var fileName = $"{Guid.NewGuid()}{extension}";
            var filePath = Path.Combine("wwwroot", "img", "content", fileName);

            Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await imageFile.CopyToAsync(stream);
            }

            return $"/img/content/{fileName}";
        }

        public static async Task<bool> DeleteImageFileAsync(string? imagePath)
        {
            if (string.IsNullOrEmpty(imagePath) || imagePath.StartsWith("http"))
                return false;

            try
            {
                var filePath = Path.Combine("wwwroot", imagePath.TrimStart('/'));
                if (File.Exists(filePath))
                {
                    await Task.Run(() => File.Delete(filePath));
                    return true;
                }
            }
            catch
            {
                // Log error if needed
            }

            return false;
        }
    }

    public class ImageValidationException : Exception
    {
        public ImageValidationException(string message) : base(message) { }
    }
} 