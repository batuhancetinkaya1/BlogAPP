using Microsoft.EntityFrameworkCore;
using BlogApp.Entity;
using BCrypt.Net;

namespace BlogApp.Data.SeedData
{
    public static class SeedData
    {
        public static async Task InitializeAsync(IServiceProvider serviceProvider)
        {
            using var context = new ApplicationDbContext(
                serviceProvider.GetRequiredService<DbContextOptions<ApplicationDbContext>>());

            // Check if the database has been seeded
            if (await context.Users.AnyAsync())
            {
                return; // Database has been seeded
            }

            // Add admin user
            var adminUser = new User
            {
                UserName = "admin",
                Email = "admin@example.com",
                Password = BCrypt.Net.BCrypt.HashPassword("admin123"),
                IsAdmin = true,
                CreatedAt = DateTime.UtcNow,
                Image = "/img/profiles/default.jpg"
            };
            context.Users.Add(adminUser);

            // Add regular user
            var regularUser = new User
            {
                UserName = "user",
                Email = "user@example.com",
                Password = BCrypt.Net.BCrypt.HashPassword("user123"),
                IsAdmin = false,
                CreatedAt = DateTime.UtcNow,
                Image = "/img/profiles/default.jpg"
            };
            context.Users.Add(regularUser);

            // Add another user
            var anotherUser = new User
            {
                UserName = "developer",
                Email = "developer@example.com",
                Password = BCrypt.Net.BCrypt.HashPassword("dev123"),
                IsAdmin = false,
                CreatedAt = DateTime.UtcNow,
                Image = "/img/profiles/default.jpg"
            };
            context.Users.Add(anotherUser);

            // Add tags
            var tags = new List<Tag>
            {
                new Tag { Name = "ASP.NET Core", Url = "aspnet-core", Color = TagColors.Primary, IsActive = true, CreatedAt = DateTime.UtcNow },
                new Tag { Name = "JavaScript", Url = "javascript", Color = TagColors.Warning, IsActive = true, CreatedAt = DateTime.UtcNow },
                new Tag { Name = "C#", Url = "csharp", Color = TagColors.Success, IsActive = true, CreatedAt = DateTime.UtcNow },
                new Tag { Name = "Entity Framework", Url = "entity-framework", Color = TagColors.Info, IsActive = true, CreatedAt = DateTime.UtcNow },
                new Tag { Name = "SQL", Url = "sql", Color = TagColors.Danger, IsActive = true, CreatedAt = DateTime.UtcNow },
                new Tag { Name = "HTML", Url = "html", Color = TagColors.Primary, IsActive = true, CreatedAt = DateTime.UtcNow },
                new Tag { Name = "CSS", Url = "css", Color = TagColors.Info, IsActive = true, CreatedAt = DateTime.UtcNow },
                new Tag { Name = "React", Url = "react", Color = TagColors.Info, IsActive = true, CreatedAt = DateTime.UtcNow },
                new Tag { Name = "Angular", Url = "angular", Color = TagColors.Danger, IsActive = true, CreatedAt = DateTime.UtcNow },
                new Tag { Name = "Vue.js", Url = "vuejs", Color = TagColors.Success, IsActive = true, CreatedAt = DateTime.UtcNow }
            };
            context.Tags.AddRange(tags);

            // Save changes to get IDs
            await context.SaveChangesAsync();

            // Add some sample posts
            var posts = new List<Post>
            {
                new Post
                {
                    Title = "Getting Started with ASP.NET Core MVC",
                    Content = "<p>This is a sample post about ASP.NET Core MVC.</p><p>ASP.NET Core MVC is a framework for building web applications in .NET Core.</p><p>ASP.NET Core MVC provides a patterns-based way to build dynamic websites that enables a clean separation of concerns. It includes all the features you need to build enterprise web applications. It's built on top of ASP.NET Core, so it benefits from all the performance, security, and reliability improvements in ASP.NET Core.</p>",
                    Description = "Learn how to build web applications with ASP.NET Core MVC",
                    Url = "getting-started-with-aspnet-core-mvc",
                    CreatedAt = DateTime.UtcNow,
                    PublishedOn = DateTime.UtcNow,
                    Status = PostStatus.Published,
                    IsActive = true,
                    UserId = adminUser.UserId,
                    Tags = new List<Tag> { tags[0], tags[2] }
                },
                new Post
                {
                    Title = "Entity Framework Core Basics",
                    Content = "<p>This is a sample post about Entity Framework Core.</p><p>Entity Framework Core is an object-relational mapper for .NET Core.</p><p>It allows developers to work with a database using .NET objects, and eliminates the need for most of the data-access code that developers usually need to write.</p>",
                    Description = "Learn the basics of Entity Framework Core",
                    Url = "entity-framework-core-basics",
                    CreatedAt = DateTime.UtcNow.AddDays(-1),
                    PublishedOn = DateTime.UtcNow.AddDays(-1),
                    Status = PostStatus.Published,
                    IsActive = true,
                    UserId = adminUser.UserId,
                    Tags = new List<Tag> { tags[0], tags[3], tags[4] }
                },
                new Post
                {
                    Title = "JavaScript Fundamentals",
                    Content = "<p>This is a sample post about JavaScript.</p><p>JavaScript is a programming language used for web development.</p><p>JavaScript is a high-level, often just-in-time compiled language that conforms to the ECMAScript specification. It has dynamic typing, prototype-based object-orientation, and first-class functions. It is multi-paradigm, supporting event-driven, functional, and imperative programming styles.</p>",
                    Description = "Learn the fundamentals of JavaScript",
                    Url = "javascript-fundamentals",
                    CreatedAt = DateTime.UtcNow.AddDays(-2),
                    PublishedOn = DateTime.UtcNow.AddDays(-2),
                    Status = PostStatus.Published,
                    IsActive = true,
                    UserId = regularUser.UserId,
                    Tags = new List<Tag> { tags[1] }
                },
                new Post
                {
                    Title = "Introduction to React",
                    Content = "<p>React is a JavaScript library for building user interfaces.</p><p>It is maintained by Facebook and a community of individual developers and companies.</p><p>React can be used as a base in the development of single-page or mobile applications. However, React is only concerned with rendering data to the DOM, and so creating React applications usually requires the use of additional libraries for state management and routing.</p>",
                    Description = "Learn the basics of React library",
                    Url = "introduction-to-react",
                    CreatedAt = DateTime.UtcNow.AddDays(-3),
                    PublishedOn = DateTime.UtcNow.AddDays(-3),
                    Status = PostStatus.Published,
                    IsActive = true,
                    UserId = anotherUser.UserId,
                    Tags = new List<Tag> { tags[1], tags[7] }
                },
                new Post
                {
                    Title = "CSS Tips and Tricks",
                    Content = "<p>CSS (Cascading Style Sheets) is a stylesheet language used for describing the presentation of a document written in HTML.</p><p>CSS is designed to enable the separation of presentation and content, including layout, colors, and fonts.</p><p>This separation can improve content accessibility, provide more flexibility and control in the specification of presentation characteristics, enable multiple web pages to share formatting, and reduce complexity and repetition in the structural content.</p>",
                    Description = "Improve your CSS skills with these tips",
                    Url = "css-tips-and-tricks",
                    CreatedAt = DateTime.UtcNow.AddDays(-4),
                    PublishedOn = DateTime.UtcNow.AddDays(-4),
                    Status = PostStatus.Published,
                    IsActive = true,
                    UserId = regularUser.UserId,
                    Tags = new List<Tag> { tags[6] }
                }
            };
            context.Posts.AddRange(posts);

            // Save changes to get IDs
            await context.SaveChangesAsync();

            // Add some comments
            var comments = new List<Comment>
            {
                new Comment
                {
                    Content = "Great post! Very informative.",
                    CreatedAt = DateTime.UtcNow,
                    PublishedOn = DateTime.UtcNow,
                    IsActive = true,
                    PostId = posts[0].PostId,
                    UserId = regularUser.UserId
                },
                new Comment
                {
                    Content = "Thanks for sharing this information.",
                    CreatedAt = DateTime.UtcNow,
                    PublishedOn = DateTime.UtcNow,
                    IsActive = true,
                    PostId = posts[0].PostId,
                    UserId = adminUser.UserId
                },
                new Comment
                {
                    Content = "This helped me a lot. Thanks!",
                    CreatedAt = DateTime.UtcNow,
                    PublishedOn = DateTime.UtcNow,
                    IsActive = true,
                    PostId = posts[1].PostId,
                    UserId = regularUser.UserId
                },
                new Comment
                {
                    Content = "Looking forward to more content like this!",
                    CreatedAt = DateTime.UtcNow,
                    PublishedOn = DateTime.UtcNow,
                    IsActive = true,
                    PostId = posts[2].PostId,
                    UserId = adminUser.UserId
                },
                new Comment
                {
                    Content = "Very well explained. I've been looking for this kind of explanation for a while!",
                    CreatedAt = DateTime.UtcNow,
                    PublishedOn = DateTime.UtcNow,
                    IsActive = true,
                    PostId = posts[3].PostId,
                    UserId = regularUser.UserId
                }
            };
            context.Comments.AddRange(comments);

            // Add some reactions
            var reactions = new List<PostReaction>
            {
                new PostReaction
                {
                    IsLike = true,
                    CreatedAt = DateTime.UtcNow,
                    PostId = posts[0].PostId,
                    UserId = regularUser.UserId
                },
                new PostReaction
                {
                    IsLike = true,
                    CreatedAt = DateTime.UtcNow,
                    PostId = posts[1].PostId,
                    UserId = regularUser.UserId
                },
                new PostReaction
                {
                    IsLike = false,
                    CreatedAt = DateTime.UtcNow,
                    PostId = posts[2].PostId,
                    UserId = adminUser.UserId
                },
                new PostReaction
                {
                    IsLike = true,
                    CreatedAt = DateTime.UtcNow,
                    PostId = posts[3].PostId,
                    UserId = adminUser.UserId
                },
                new PostReaction
                {
                    IsLike = true,
                    CreatedAt = DateTime.UtcNow,
                    PostId = posts[3].PostId,
                    UserId = regularUser.UserId
                },
                new PostReaction
                {
                    IsLike = true,
                    CreatedAt = DateTime.UtcNow,
                    PostId = posts[4].PostId,
                    UserId = anotherUser.UserId
                }
            };
            context.PostReactions.AddRange(reactions);

            // Save all changes
            await context.SaveChangesAsync();
        }
    }
} 