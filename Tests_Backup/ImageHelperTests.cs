using BlogApp.Helpers;
using Xunit;
using System.IO;
using System;
using System.Threading.Tasks;

namespace BlogApp.Tests
{
    public class ImageHelperTests
    {
        [Fact]
        public void GetProfileImageUrl_WithNullImage_ReturnsDefaultImage()
        {
            // Arrange
            string? imagePath = null;

            // Act
            var result = ImageHelper.GetProfileImageUrl(imagePath);

            // Assert
            Assert.Equal("/img/profiles/default.jpg", result);
        }

        [Fact]
        public void GetProfileImageUrl_WithEmptyImage_ReturnsDefaultImage()
        {
            // Arrange
            string imagePath = "";

            // Act
            var result = ImageHelper.GetProfileImageUrl(imagePath);

            // Assert
            Assert.Equal("/img/profiles/default.jpg", result);
        }

        [Fact]
        public void GetProfileImageUrl_WithAbsolutePath_ReturnsSamePath()
        {
            // Arrange
            string imagePath = "/img/profiles/test.jpg";

            // Act
            var result = ImageHelper.GetProfileImageUrl(imagePath);

            // Assert
            Assert.Equal("/img/profiles/test.jpg", result);
        }

        [Fact]
        public void GetProfileImageUrl_WithRelativePath_ReturnsFullPath()
        {
            // Arrange
            string imagePath = "test.jpg";

            // Act
            var result = ImageHelper.GetProfileImageUrl(imagePath);

            // Assert
            Assert.Equal("/img/profiles/test.jpg", result);
        }
        
        [Fact]
        public async Task DeleteImageFile_WithDefaultImage_ReturnsFalse()
        {
            // Arrange
            string imagePath = "/img/profiles/default.jpg";

            // Act
            var result = await ImageHelper.DeleteImageFileAsync(imagePath);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task DeleteImageFile_WithNullOrEmpty_ReturnsFalse()
        {
            // Arrange & Act
            var result1 = await ImageHelper.DeleteImageFileAsync(string.Empty);
            var result2 = await ImageHelper.DeleteImageFileAsync(string.Empty);

            // Assert
            Assert.False(result1);
            Assert.False(result2);
        }

        [Fact]
        public async Task DeleteImageFile_WithNonExistentFile_ReturnsFalse()
        {
            // Arrange
            string imagePath = "/img/profiles/nonexistent-file-" + Guid.NewGuid().ToString() + ".jpg";

            // Act
            var result = await ImageHelper.DeleteImageFileAsync(imagePath);

            // Assert
            Assert.False(result);
        }
    }
} 