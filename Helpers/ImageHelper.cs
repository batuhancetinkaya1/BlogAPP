using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using BlogApp.Models;

namespace BlogApp.Helpers
{
    public static class ImageHelper
    {
        public static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".webp" };
        public const int MaxFileSizeBytes = 2 * 1024 * 1024; // 2MB

        public static string GetProfileImageUrl(string? imagePath)
        {
            if (string.IsNullOrEmpty(imagePath))
                return "/img/profiles/default.jpg";

            return imagePath.StartsWith("/") ? imagePath : $"/img/profiles/{imagePath}";
        }

        public static async Task<string> ValidateAndSaveProfileImageAsync(IFormFile file)
        {
            if (file == null)
                throw new ImageValidationException("No file was uploaded.");

            // Validate file size
            if (file.Length > MaxFileSizeBytes)
                throw new ImageValidationException($"File size exceeds the limit of {MaxFileSizeBytes / 1024 / 1024}MB.");

            // Validate file extension
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!Array.Exists(AllowedExtensions, ext => ext.Equals(extension)))
                throw new ImageValidationException($"File type not allowed. Allowed types: {string.Join(", ", AllowedExtensions)}");

            // Create unique filename
            var fileName = $"{Guid.NewGuid()}{extension}";
            var uploadsDirectory = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "img", "profiles");
            
            if (!Directory.Exists(uploadsDirectory))
                Directory.CreateDirectory(uploadsDirectory);

            var filePath = Path.Combine(uploadsDirectory, fileName);

            // Save file asynchronously
            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }

            return $"/img/profiles/{fileName}";
        }

        public static async Task<string> ValidateAndSavePostImageAsync(IFormFile file)
        {
            if (file == null)
                throw new ImageValidationException("No file was uploaded.");

            // Validate file size
            if (file.Length > MaxFileSizeBytes)
                throw new ImageValidationException($"File size exceeds the limit of {MaxFileSizeBytes / 1024 / 1024}MB.");

            // Validate file extension
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!Array.Exists(AllowedExtensions, ext => ext.Equals(extension)))
                throw new ImageValidationException($"File type not allowed. Allowed types: {string.Join(", ", AllowedExtensions)}");

            // Create unique filename
            var fileName = $"{Guid.NewGuid()}{extension}";
            var uploadsDirectory = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "img", "posts");
            
            if (!Directory.Exists(uploadsDirectory))
                Directory.CreateDirectory(uploadsDirectory);

            var filePath = Path.Combine(uploadsDirectory, fileName);

            // Save file asynchronously
            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }

            return $"/img/posts/{fileName}";
        }

        public static async Task<string> ValidateAndSaveContentImageAsync(IFormFile file)
        {
            if (file == null)
                throw new ImageValidationException("No file was uploaded.");

            // Validate file size
            if (file.Length > MaxFileSizeBytes)
                throw new ImageValidationException($"File size exceeds the limit of {MaxFileSizeBytes / 1024 / 1024}MB.");

            // Validate file extension
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!Array.Exists(AllowedExtensions, ext => ext.Equals(extension)))
                throw new ImageValidationException($"File type not allowed. Allowed types: {string.Join(", ", AllowedExtensions)}");

            // Create unique filename
            var fileName = $"{Guid.NewGuid()}{extension}";
            var uploadsDirectory = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "img", "content");
            
            if (!Directory.Exists(uploadsDirectory))
                Directory.CreateDirectory(uploadsDirectory);

            var filePath = Path.Combine(uploadsDirectory, fileName);

            // Save file asynchronously
            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }

            return $"/img/content/{fileName}";
        }

        public static async Task<bool> DeleteImageFileAsync(string imagePath)
        {
            if (string.IsNullOrEmpty(imagePath) || imagePath.Contains("default.jpg"))
                return false;

            try
            {
                var fullPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", imagePath.TrimStart('/'));
                if (File.Exists(fullPath))
                {
                    await Task.Run(() => File.Delete(fullPath));
                    return true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting image: {ex.Message}");
                // Consider using a proper logging mechanism instead of Console.WriteLine
            }

            return false;
        }
        
        // Keep the synchronous method for backward compatibility but mark it as obsolete
        [Obsolete("Use DeleteImageFileAsync instead")]
        public static bool DeleteImageFile(string imagePath)
        {
            return DeleteImageFileAsync(imagePath).GetAwaiter().GetResult();
        }
    }

    public class ImageValidationException : Exception
    {
        public ImageValidationException(string message) : base(message) { }
    }
} 