using BlogApp.Data.Abstract;
using BlogApp.Entity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace BlogApp.Data.SeedData;

public static class SeedData
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var services = scope.ServiceProvider;
        var logger = services.GetRequiredService<ILogger<Program>>();
        
        try
        {
            var userManager = services.GetRequiredService<IUserRepository>();
            var tagManager = services.GetRequiredService<ITagRepository>();
            var postManager = services.GetRequiredService<IPostRepository>();

            // Create admin user if not exists
            await SeedAdminUserAsync(userManager);
            
            // Create test user if not exists
            await SeedTestUserAsync(userManager);
            
            // Create default tags if not exists
            await SeedDefaultTagsAsync(tagManager);
            
            // Create sample posts if not exists
            await SeedSamplePostsAsync(userManager, tagManager, postManager);
            
            logger.LogInformation("Database seeded successfully.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while seeding the database.");
            throw;
        }
    }

    private static async Task SeedAdminUserAsync(IUserRepository userManager)
    {
        if (!userManager.Users.Any(u => u.Email == "admin@example.com"))
        {
            var adminUser = new User
            {
                UserName = "Admin",
                Email = "admin@example.com",
                Password = BCrypt.Net.BCrypt.HashPassword("Admin123!"),
                IsAdmin = true,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
            userManager.CreateUser(adminUser);
            await userManager.SaveChangesAsync();
        }
    }

    private static async Task SeedTestUserAsync(IUserRepository userManager)
    {
        if (!userManager.Users.Any(u => u.Email == "test@example.com"))
        {
            var testUser = new User
            {
                UserName = "Test User",
                Email = "test@example.com",
                Password = BCrypt.Net.BCrypt.HashPassword("Test123!"),
                IsAdmin = false,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
            userManager.CreateUser(testUser);
            await userManager.SaveChangesAsync();
        }
    }

    private static async Task SeedDefaultTagsAsync(ITagRepository tagManager)
    {
        var defaultTags = new[]
        {
            new Tag { Name = "Teknoloji", Url = "teknoloji", Color = "primary", IsActive = true, CreatedAt = DateTime.UtcNow },
            new Tag { Name = "Yazılım", Url = "yazilim", Color = "success", IsActive = true, CreatedAt = DateTime.UtcNow },
            new Tag { Name = "Web Geliştirme", Url = "web-gelistirme", Color = "info", IsActive = true, CreatedAt = DateTime.UtcNow },
            new Tag { Name = "Mobil", Url = "mobil", Color = "warning", IsActive = true, CreatedAt = DateTime.UtcNow },
            new Tag { Name = "Veri Bilimi", Url = "veri-bilimi", Color = "danger", IsActive = true, CreatedAt = DateTime.UtcNow }
        };

        foreach (var tag in defaultTags)
        {
            if (!tagManager.Tags.Any(t => t.Name == tag.Name))
            {
                tagManager.CreateTag(tag);
            }
        }
        await tagManager.SaveChangesAsync();
    }

    private static async Task SeedSamplePostsAsync(IUserRepository userManager, ITagRepository tagManager, IPostRepository postManager)
    {
        if (!postManager.Posts.Any())
        {
            var adminUser = userManager.GetByEmail("admin@example.com");
            var testUserForPosts = userManager.GetByEmail("test@example.com");
            var allTags = tagManager.Tags.ToList();

            var samplePosts = new[]
            {
                new Post
                {
                    Title = "Hoş Geldiniz",
                    Content = "<p>Blog sitemize hoş geldiniz! Bu blog, teknoloji ve yazılım dünyasındaki deneyimlerimizi paylaşmak için oluşturulmuştur.</p>",
                    Description = "Blog sitemize hoş geldiniz mesajı",
                    Image = "welcome.jpg",
                    CreatedAt = DateTime.UtcNow.AddDays(-10),
                    PublishedOn = DateTime.UtcNow.AddDays(-10),
                    Status = PostStatus.Published,
                    IsActive = true,
                    User = adminUser,
                    Tags = new List<Tag> { allTags.First(t => t.Name == "Teknoloji") }
                },
                new Post
                {
                    Title = "Modern Web Geliştirme Teknolojileri",
                    Content = "<p>Modern web geliştirme dünyasında kullanılan popüler teknolojiler ve framework'ler hakkında detaylı bir inceleme.</p>",
                    Description = "Web geliştirme teknolojileri hakkında kapsamlı bir rehber",
                    Image = "web-dev.jpg",
                    CreatedAt = DateTime.UtcNow.AddDays(-8),
                    PublishedOn = DateTime.UtcNow.AddDays(-8),
                    Status = PostStatus.Published,
                    IsActive = true,
                    User = adminUser,
                    Tags = new List<Tag> { allTags.First(t => t.Name == "Web Geliştirme"), allTags.First(t => t.Name == "Yazılım") }
                },
                new Post
                {
                    Title = "Mobil Uygulama Geliştirme İpuçları",
                    Content = "<p>Mobil uygulama geliştirirken dikkat edilmesi gereken önemli noktalar ve best practice'ler.</p>",
                    Description = "Mobil uygulama geliştirme sürecinde faydalı ipuçları",
                    Image = "mobile-dev.jpg",
                    CreatedAt = DateTime.UtcNow.AddDays(-4),
                    PublishedOn = DateTime.UtcNow.AddDays(-4),
                    Status = PostStatus.Published,
                    IsActive = true,
                    User = testUserForPosts,
                    Tags = new List<Tag> { allTags.First(t => t.Name == "Mobil"), allTags.First(t => t.Name == "Yazılım") }
                }
            };

            foreach (var post in samplePosts)
            {
                postManager.CreatePost(post);
            }
            await postManager.SaveChangesAsync();
        }
    }
} 