using Microsoft.EntityFrameworkCore;
using BlogApp.Entity;
using BCrypt.Net;
using System;

namespace BlogApp.Data.SeedData
{
    public static class SeedData
    {
        // Use a fixed base date for consistent testing
        private static readonly DateTime BaseDate = new DateTime(2023, 10, 1, 12, 0, 0, DateTimeKind.Utc);
        
        public static async Task InitializeAsync(IServiceProvider serviceProvider)
        {
            using var context = new ApplicationDbContext(
                serviceProvider.GetRequiredService<DbContextOptions<ApplicationDbContext>>());

            // Check if the database has been seeded
            if (await context.Users.AnyAsync() && await context.Comments.AnyAsync() && await context.PostReactions.AnyAsync())
            {
                return; // Database has been fully seeded
            }

            // Clear existing data if partial seeding occurred
            if (await context.Users.AnyAsync() && (!await context.Comments.AnyAsync() || !await context.PostReactions.AnyAsync()))
            {
                // Database was only partially seeded - clear all data to fully reseed
                context.CommentReactions.RemoveRange(await context.CommentReactions.ToListAsync());
                context.Comments.RemoveRange(await context.Comments.ToListAsync());
                context.PostReactions.RemoveRange(await context.PostReactions.ToListAsync());
                context.Posts.RemoveRange(await context.Posts.ToListAsync());
                context.Tags.RemoveRange(await context.Tags.ToListAsync());
                await context.SaveChangesAsync();
            }
            
            // If no users, add all seed data
            if (!await context.Users.AnyAsync())
            {
                // Add admin user
                var adminUser = new User
                {
                    UserName = "admin",
                    Email = "admin@example.com",
                    Password = BCrypt.Net.BCrypt.HashPassword("admin123"),
                    IsAdmin = true,
                    CreatedAt = BaseDate,
                    Image = "/img/profiles/default1_male.jpg"
                };
                context.Users.Add(adminUser);

                // Add regular user
                var regularUser = new User
                {
                    UserName = "user",
                    Email = "user@example.com",
                    Password = BCrypt.Net.BCrypt.HashPassword("user123"),
                    IsAdmin = false,
                    CreatedAt = BaseDate,
                    Image = "/img/profiles/default2_female.jpg"
                };
                context.Users.Add(regularUser);

                // Add another user with admin role
                var anotherUser = new User
                {
                    UserName = "developer",
                    Email = "developer@example.com",
                    Password = BCrypt.Net.BCrypt.HashPassword("dev123"),
                    IsAdmin = true,
                    CreatedAt = BaseDate,
                    Image = "/img/profiles/default3_male.jpg"
                };
                context.Users.Add(anotherUser);
                
                // Add more sample users
                var additionalUsers = new List<User>
                {
                    new User
                    {
                        UserName = "john_doe",
                        Email = "john@example.com",
                        Password = BCrypt.Net.BCrypt.HashPassword("password123"),
                        IsAdmin = false,
                        CreatedAt = BaseDate.AddDays(-5),
                        Image = "/img/profiles/default2_male.jpg"
                    },
                    new User
                    {
                        UserName = "jane_smith",
                        Email = "jane@example.com",
                        Password = BCrypt.Net.BCrypt.HashPassword("password123"),
                        IsAdmin = false,
                        CreatedAt = BaseDate.AddDays(-4),
                        Image = "/img/profiles/default3_female.jpg"
                    },
                    new User
                    {
                        UserName = "blogger",
                        Email = "blogger@example.com",
                        Password = BCrypt.Net.BCrypt.HashPassword("password123"),
                        IsAdmin = false,
                        CreatedAt = BaseDate.AddDays(-3),
                        Image = "/img/profiles/default4_male.jpg"
                    },
                    new User
                    {
                        UserName = "techwriter",
                        Email = "techwriter@example.com",
                        Password = BCrypt.Net.BCrypt.HashPassword("password123"),
                        IsAdmin = false,
                        CreatedAt = BaseDate.AddDays(-10),
                        Image = "/img/profiles/default4_female.jpg"
                    },
                    new User
                    {
                        UserName = "coder_guru",
                        Email = "coder_guru@example.com",
                        Password = BCrypt.Net.BCrypt.HashPassword("password123"),
                        IsAdmin = false,
                        CreatedAt = BaseDate.AddDays(-15),
                        Image = "/img/profiles/default.jpg"
                    },
                    new User
                    {
                        UserName = "designpro",
                        Email = "designpro@example.com",
                        Password = BCrypt.Net.BCrypt.HashPassword("password123"),
                        IsAdmin = false,
                        CreatedAt = BaseDate.AddDays(-12),
                        Image = "/img/profiles/default1_female.jpg"
                    }
                };
                context.Users.AddRange(additionalUsers);

                // Add tags
                var tags = new List<Tag>
                {
                    new Tag { Name = "ASP.NET Core", Url = "aspnet-core", Color = TagColors.Primary, IsActive = true, CreatedAt = BaseDate },
                    new Tag { Name = "JavaScript", Url = "javascript", Color = TagColors.Warning, IsActive = true, CreatedAt = BaseDate },
                    new Tag { Name = "C#", Url = "csharp", Color = TagColors.Success, IsActive = true, CreatedAt = BaseDate },
                    new Tag { Name = "Entity Framework", Url = "entity-framework", Color = TagColors.Purple, IsActive = true, CreatedAt = BaseDate },
                    new Tag { Name = "SQL", Url = "sql", Color = TagColors.Danger, IsActive = true, CreatedAt = BaseDate },
                    new Tag { Name = "HTML", Url = "html", Color = TagColors.Orange, IsActive = true, CreatedAt = BaseDate },
                    new Tag { Name = "CSS", Url = "css", Color = TagColors.Purple, IsActive = true, CreatedAt = BaseDate },
                    new Tag { Name = "React", Url = "react", Color = TagColors.Primary, IsActive = true, CreatedAt = BaseDate },
                    new Tag { Name = "Angular", Url = "angular", Color = TagColors.Danger, IsActive = true, CreatedAt = BaseDate },
                    new Tag { Name = "Vue.js", Url = "vuejs", Color = TagColors.Success, IsActive = true, CreatedAt = BaseDate },
                    new Tag { Name = "Python", Url = "python", Color = TagColors.Primary, IsActive = true, CreatedAt = BaseDate },
                    new Tag { Name = "Java", Url = "java", Color = TagColors.Danger, IsActive = true, CreatedAt = BaseDate },
                    new Tag { Name = "Docker", Url = "docker", Color = TagColors.Primary, IsActive = true, CreatedAt = BaseDate },
                    new Tag { Name = "DevOps", Url = "devops", Color = TagColors.Warning, IsActive = true, CreatedAt = BaseDate },
                    new Tag { Name = "Machine Learning", Url = "machine-learning", Color = TagColors.Success, IsActive = true, CreatedAt = BaseDate },
                    new Tag { Name = "Data Science", Url = "data-science", Color = TagColors.Primary, IsActive = true, CreatedAt = BaseDate },
                    new Tag { Name = "Cloud Computing", Url = "cloud-computing", Color = TagColors.Orange, IsActive = true, CreatedAt = BaseDate },
                    new Tag { Name = "Cybersecurity", Url = "cybersecurity", Color = TagColors.Danger, IsActive = true, CreatedAt = BaseDate },
                    new Tag { Name = "Mobile Development", Url = "mobile-development", Color = TagColors.Dark, IsActive = true, CreatedAt = BaseDate },
                    new Tag { Name = "UX/UI Design", Url = "ux-ui-design", Color = TagColors.Purple, IsActive = true, CreatedAt = BaseDate },
                    new Tag { Name = "Game Development", Url = "game-development", Color = TagColors.Warning, IsActive = true, CreatedAt = BaseDate },
                    new Tag { Name = "Blockchain", Url = "blockchain", Color = TagColors.Secondary, IsActive = true, CreatedAt = BaseDate },
                    new Tag { Name = "IoT", Url = "internet-of-things", Color = TagColors.Success, IsActive = true, CreatedAt = BaseDate },
                    new Tag { Name = "Big Data", Url = "big-data", Color = TagColors.Primary, IsActive = true, CreatedAt = BaseDate }
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
                        Image = "/img/posts/default1.jpg",
                        CreatedAt = BaseDate,
                        PublishedOn = BaseDate,
                        Status = PostStatus.Published,
                        IsActive = true,
                        UserId = adminUser.UserId,
                        ReadTime = 7,
                        Keywords = "ASP.NET Core, MVC, Web Development, Tutorial",
                        Tags = new List<Tag> { tags[0], tags[2] }
                    },
                    new Post
                    {
                        Title = "Entity Framework Core Basics",
                        Content = "<p>This is a sample post about Entity Framework Core.</p><p>Entity Framework Core is an object-relational mapper for .NET Core.</p><p>It allows developers to work with a database using .NET objects, and eliminates the need for most of the data-access code that developers usually need to write.</p>",
                        Description = "Learn the basics of Entity Framework Core",
                        Url = "entity-framework-core-basics",
                        Image = "/img/posts/default2.jpg",
                        CreatedAt = BaseDate.AddDays(-1),
                        PublishedOn = BaseDate.AddDays(-1),
                        Status = PostStatus.Published,
                        IsActive = true,
                        UserId = adminUser.UserId,
                        ReadTime = 5,
                        Keywords = "Entity Framework, Database, ORM, .NET Core",
                        Tags = new List<Tag> { tags[0], tags[3], tags[4] }
                    },
                    new Post
                    {
                        Title = "JavaScript Fundamentals",
                        Content = "<p>This is a sample post about JavaScript.</p><p>JavaScript is a programming language used for web development.</p><p>JavaScript is a high-level, often just-in-time compiled language that conforms to the ECMAScript specification. It has dynamic typing, prototype-based object-orientation, and first-class functions. It is multi-paradigm, supporting event-driven, functional, and imperative programming styles.</p>",
                        Description = "Learn the fundamentals of JavaScript",
                        Url = "javascript-fundamentals",
                        Image = "/img/posts/default3.jpg",
                        CreatedAt = BaseDate.AddDays(-2),
                        PublishedOn = BaseDate.AddDays(-2),
                        Status = PostStatus.Published,
                        IsActive = true,
                        UserId = regularUser.UserId,
                        ReadTime = 6,
                        Keywords = "JavaScript, Frontend, Web Development, Programming",
                        Tags = new List<Tag> { tags[1] }
                    },
                    new Post
                    {
                        Title = "Introduction to React",
                        Content = "<p>React is a JavaScript library for building user interfaces.</p><p>It is maintained by Facebook and a community of individual developers and companies.</p><p>React can be used as a base in the development of single-page or mobile applications. However, React is only concerned with rendering data to the DOM, and so creating React applications usually requires the use of additional libraries for state management and routing.</p>",
                        Description = "Learn the basics of React library",
                        Url = "introduction-to-react",
                        Image = "/img/posts/default4.jpg",
                        CreatedAt = BaseDate.AddDays(-3),
                        PublishedOn = BaseDate.AddDays(-3),
                        Status = PostStatus.Published,
                        IsActive = true,
                        UserId = anotherUser.UserId,
                        ReadTime = 8,
                        Keywords = "React, JavaScript, Frontend, UI Library",
                        Tags = new List<Tag> { tags[1], tags[7] }
                    },
                    new Post
                    {
                        Title = "CSS Tips and Tricks",
                        Content = "<p>CSS (Cascading Style Sheets) is a stylesheet language used for describing the presentation of a document written in HTML.</p><p>CSS is designed to enable the separation of presentation and content, including layout, colors, and fonts.</p><p>This separation can improve content accessibility, provide more flexibility and control in the specification of presentation characteristics, enable multiple web pages to share formatting, and reduce complexity and repetition in the structural content.</p>",
                        Description = "Improve your CSS skills with these tips",
                        Url = "css-tips-and-tricks",
                        Image = "/img/posts/default5.jpg",
                        CreatedAt = BaseDate.AddDays(-4),
                        PublishedOn = BaseDate.AddDays(-4),
                        Status = PostStatus.Published,
                        IsActive = true,
                        UserId = regularUser.UserId,
                        ReadTime = 5,
                        Keywords = "CSS, Web Design, Frontend, Styling",
                        Tags = new List<Tag> { tags[6] }
                    }
                };
                context.Posts.AddRange(posts);
                
                // Add more sample posts
                var additionalPosts = new List<Post>
                {
                    new Post
                    {
                        Title = "Introduction to Python Programming",
                        Content = "<p>Python is a high-level, interpreted programming language known for its simplicity and readability.</p><p>Python's syntax allows programmers to express concepts in fewer lines of code than languages like C++ or Java. It supports multiple programming paradigms, including procedural, object-oriented, and functional programming.</p><p>Python is widely used in data science, machine learning, web development, and automation.</p>",
                        Description = "Learn the basics of Python programming language",
                        Url = "introduction-to-python-programming",
                        Image = "/img/posts/default6.jpg",
                        CreatedAt = BaseDate.AddDays(-5),
                        PublishedOn = BaseDate.AddDays(-5),
                        Status = PostStatus.Published,
                        IsActive = true,
                        UserId = additionalUsers[0].UserId,
                        ReadTime = 7,
                        Keywords = "Python, Programming, Data Science, Beginner",
                        Tags = new List<Tag> { tags[10] }
                    },
                    new Post
                    {
                        Title = "Docker for Beginners",
                        Content = "<p>Docker is a platform that enables developers to build, package, and deploy applications in containers.</p><p>Containers are lightweight, portable, and self-sufficient environments that can run on any machine with Docker installed. This makes it easier to develop and deploy applications consistently across different environments.</p><p>In this post, we'll explore the basics of Docker and how to get started with containerization.</p>",
                        Description = "Get started with Docker containerization",
                        Url = "docker-for-beginners",
                        Image = "/img/posts/default7.jpg",
                        CreatedAt = BaseDate.AddDays(-6),
                        PublishedOn = BaseDate.AddDays(-6),
                        Status = PostStatus.Published,
                        IsActive = true,
                        UserId = additionalUsers[1].UserId,
                        ReadTime = 9,
                        Keywords = "Docker, Containerization, DevOps, Development",
                        Tags = new List<Tag> { tags[12], tags[13] }
                    },
                    new Post
                    {
                        Title = "Getting Started with Machine Learning",
                        Content = "<p>Machine Learning is a subset of artificial intelligence that enables computers to learn and improve from experience without being explicitly programmed.</p><p>In this post, we'll cover the basics of machine learning, including supervised and unsupervised learning, common algorithms, and tools for getting started.</p><p>We'll also explore how to prepare your data, train models, and evaluate their performance.</p>",
                        Description = "An introduction to machine learning concepts and techniques",
                        Url = "getting-started-with-machine-learning",
                        Image = "/img/posts/default8.jpg",
                        CreatedAt = BaseDate.AddDays(-7),
                        PublishedOn = BaseDate.AddDays(-7),
                        Status = PostStatus.Published,
                        IsActive = true,
                        UserId = additionalUsers[2].UserId,
                        ReadTime = 12,
                        Keywords = "Machine Learning, AI, Data Science, Algorithms",
                        Tags = new List<Tag> { tags[14], tags[15], tags[10] }
                    },
                    new Post
                    {
                        Title = "Modern Web Development with React",
                        Content = "<p>React has revolutionized the way we build web applications, offering a component-based architecture that makes UI development more efficient and maintainable.</p><p>In this post, we'll explore key concepts like components, props, state, hooks, and the virtual DOM. We'll also look at best practices for structuring React applications and managing application state.</p><p>By the end, you'll have a good understanding of what makes React so powerful for building modern web applications.</p>",
                        Description = "Explore modern web development techniques with React",
                        Url = "modern-web-development-with-react",
                        Image = "/img/posts/default9.jpg",
                        CreatedAt = BaseDate.AddDays(-8),
                        PublishedOn = BaseDate.AddDays(-8),
                        Status = PostStatus.Published,
                        IsActive = true,
                        UserId = adminUser.UserId,
                        ReadTime = 10,
                        Keywords = "React, Frontend, JavaScript, Web Development",
                        Tags = new List<Tag> { tags[7], tags[1], tags[5] }
                    },
                    new Post
                    {
                        Title = "Cloud Computing Essentials",
                        Content = "<p>Cloud computing has transformed how businesses deploy and scale applications and services.</p><p>This post explores the fundamental concepts of cloud computing, including IaaS, PaaS, and SaaS models. We'll discuss the benefits of cloud migration, key considerations for choosing a cloud provider, and common cloud services.</p><p>Whether you're a developer, IT professional, or business leader, understanding cloud computing is essential in today's digital landscape.</p>",
                        Description = "Learn the fundamentals of cloud computing and its business applications",
                        Url = "cloud-computing-essentials",
                        Image = "/img/posts/default10.jpg",
                        CreatedAt = BaseDate.AddDays(-9),
                        PublishedOn = BaseDate.AddDays(-9),
                        Status = PostStatus.Published,
                        IsActive = true,
                        UserId = additionalUsers[4].UserId,
                        ReadTime = 9,
                        Keywords = "Cloud Computing, AWS, Azure, IaaS, PaaS, SaaS",
                        Tags = new List<Tag> { tags[16], tags[13] }
                    },
                    new Post
                    {
                        Title = "Cybersecurity Best Practices",
                        Content = "<p>In an increasingly digital world, cybersecurity has become a critical concern for individuals and organizations alike.</p><p>This post covers essential cybersecurity best practices, including password management, multi-factor authentication, data encryption, and threat detection. We'll also discuss common attack vectors and how to protect against them.</p><p>By implementing these practices, you can significantly reduce your risk of security breaches and data loss.</p>",
                        Description = "Protect your digital assets with these cybersecurity best practices",
                        Url = "cybersecurity-best-practices",
                        Image = "/img/posts/default11.jpg",
                        CreatedAt = BaseDate.AddDays(-10),
                        PublishedOn = BaseDate.AddDays(-10),
                        Status = PostStatus.Published,
                        IsActive = true,
                        UserId = additionalUsers[3].UserId,
                        ReadTime = 11,
                        Keywords = "Cybersecurity, Security, Privacy, Encryption, Protection",
                        Tags = new List<Tag> { tags[17] }
                    }
                };
                context.Posts.AddRange(additionalPosts);

                // Save changes to get IDs
                await context.SaveChangesAsync();

                // Always make sure to seed comments and reactions
                if (!await context.Comments.AnyAsync())
                {
                    // We need to fetch users and posts from the database
                    var dbUsers = await context.Users.ToListAsync();
                    var dbPosts = await context.Posts.ToListAsync();
                    
                    // If there are users and posts to work with
                    if (dbUsers.Any() && dbPosts.Any())
                    {
                        var dbAdminUser = dbUsers.FirstOrDefault(u => u.UserName == "admin");
                        var dbRegularUser = dbUsers.FirstOrDefault(u => u.UserName == "user");
                        var dbDeveloperUser = dbUsers.FirstOrDefault(u => u.UserName == "developer");
                        var dbAdditionalUsers = dbUsers.Where(u => u.UserName != "admin" && u.UserName != "user" && u.UserName != "developer").ToList();
                        
                        // Ensure we have the required users
                        if (dbAdminUser != null && dbRegularUser != null && dbAdditionalUsers.Count >= 4)
                        {
                            // Add basic comments first (without parent references)
                            var comments = new List<Comment>
                            {
                                // Post 1 comments
                                new Comment
                                {
                                    Content = "Great post! Very informative and well-structured. Looking forward to your next article on this topic.",
                                    CreatedAt = BaseDate.AddHours(2),
                                    PublishedOn = BaseDate.AddHours(2),
                                    IsActive = true,
                                    PostId = dbPosts[0].PostId,
                                    UserId = dbRegularUser.UserId
                                },
                                new Comment
                                {
                                    Content = "Thanks for sharing this detailed information. The examples you provided made concepts much clearer for me.",
                                    CreatedAt = BaseDate.AddHours(4),
                                    PublishedOn = BaseDate.AddHours(4),
                                    IsActive = true,
                                    PostId = dbPosts[0].PostId,
                                    UserId = dbAdminUser.UserId
                                },
                                
                                // Post 2 comments
                                new Comment
                                {
                                    Content = "This helped me solve a problem I've been stuck on for days. Thanks for the clear explanation!",
                                    CreatedAt = BaseDate.AddDays(-1).AddHours(3),
                                    PublishedOn = BaseDate.AddDays(-1).AddHours(3),
                                    IsActive = true,
                                    PostId = dbPosts[1].PostId,
                                    UserId = dbRegularUser.UserId
                                },
                                
                                // Post 3 comments
                                new Comment
                                {
                                    Content = "Looking forward to more content like this! Your tutorials are always so practical and easy to follow.",
                                    CreatedAt = BaseDate.AddDays(-2).AddHours(5),
                                    PublishedOn = BaseDate.AddDays(-2).AddHours(5),
                                    IsActive = true,
                                    PostId = dbPosts[2].PostId,
                                    UserId = dbAdminUser.UserId
                                },
                                
                                // Post 4 comments
                                new Comment
                                {
                                    Content = "Very well explained. I've been looking for this kind of comprehensive guide for a while!",
                                    CreatedAt = BaseDate.AddDays(-3).AddHours(2),
                                    PublishedOn = BaseDate.AddDays(-3).AddHours(2),
                                    IsActive = true,
                                    PostId = dbPosts[3].PostId,
                                    UserId = dbRegularUser.UserId
                                },
                                new Comment
                                {
                                    Content = "I disagree with some points in the article. In my experience, the approach mentioned in the third paragraph doesn't scale well for larger applications.",
                                    CreatedAt = BaseDate.AddDays(-3).AddHours(4),
                                    PublishedOn = BaseDate.AddDays(-3).AddHours(4),
                                    IsActive = true,
                                    PostId = dbPosts[3].PostId,
                                    UserId = dbAdditionalUsers[0].UserId
                                },
                                
                                // Post 5 comments
                                new Comment
                                {
                                    Content = "These CSS tips saved me hours of debugging! Especially the part about flexbox alignment.",
                                    CreatedAt = BaseDate.AddDays(-4).AddHours(3),
                                    PublishedOn = BaseDate.AddDays(-4).AddHours(3),
                                    IsActive = true,
                                    PostId = dbPosts[4].PostId,
                                    UserId = dbAdditionalUsers[2].UserId
                                },
                                
                                // Post 6 comments
                                new Comment
                                {
                                    Content = "Python is definitely my favorite language for data science projects! The ecosystem of libraries is unmatched.",
                                    CreatedAt = BaseDate.AddDays(-5).AddHours(2),
                                    PublishedOn = BaseDate.AddDays(-5).AddHours(2),
                                    IsActive = true,
                                    PostId = dbPosts[5].PostId,
                                    UserId = dbRegularUser.UserId
                                },
                                new Comment
                                {
                                    Content = "Can you recommend some good Python libraries for machine learning beyond the basics? I'm looking to expand my toolkit.",
                                    CreatedAt = BaseDate.AddDays(-5).AddHours(3),
                                    PublishedOn = BaseDate.AddDays(-5).AddHours(3),
                                    IsActive = true,
                                    PostId = dbPosts[5].PostId,
                                    UserId = dbAdditionalUsers[2].UserId
                                },
                                
                                // Post 7 comments
                                new Comment
                                {
                                    Content = "Docker has made my deployment process so much easier! No more 'works on my machine' problems.",
                                    CreatedAt = BaseDate.AddDays(-6).AddHours(1),
                                    PublishedOn = BaseDate.AddDays(-6).AddHours(1),
                                    IsActive = true,
                                    PostId = dbPosts[6].PostId,
                                    UserId = dbAdminUser.UserId
                                },
                                new Comment
                                {
                                    Content = "Do you have any tips for optimizing Docker images? My builds are getting quite large.",
                                    CreatedAt = BaseDate.AddDays(-6).AddHours(3),
                                    PublishedOn = BaseDate.AddDays(-6).AddHours(3),
                                    IsActive = true,
                                    PostId = dbPosts[6].PostId,
                                    UserId = dbAdditionalUsers[3].UserId
                                },
                                
                                // Post 8 comments
                                new Comment
                                {
                                    Content = "Great introduction to machine learning concepts! Could you cover neural networks in more detail in a future post?",
                                    CreatedAt = BaseDate.AddDays(-7).AddHours(6),
                                    PublishedOn = BaseDate.AddDays(-7).AddHours(6),
                                    IsActive = true,
                                    PostId = dbPosts[7].PostId,
                                    UserId = dbAdditionalUsers[0].UserId
                                },
                                
                                // Post 9 comments
                                new Comment
                                {
                                    Content = "React has changed the way I think about front-end development. The component-based approach makes so much sense for modern UIs.",
                                    CreatedAt = BaseDate.AddDays(-8).AddHours(2),
                                    PublishedOn = BaseDate.AddDays(-8).AddHours(2),
                                    IsActive = true,
                                    PostId = dbPosts[8].PostId,
                                    UserId = dbAdditionalUsers[1].UserId
                                },
                                
                                // Post 10 comments
                                new Comment
                                {
                                    Content = "I found the cloud provider comparison particularly helpful. I've been debating between AWS and Azure for our startup.",
                                    CreatedAt = BaseDate.AddDays(-9).AddHours(5),
                                    PublishedOn = BaseDate.AddDays(-9).AddHours(5),
                                    IsActive = true,
                                    PostId = dbPosts[9].PostId,
                                    UserId = dbAdditionalUsers[2].UserId
                                },
                                
                                // Post 11 comments
                                new Comment
                                {
                                    Content = "The password management section was eye-opening. I've immediately enabled 2FA on all my accounts after reading this.",
                                    CreatedAt = BaseDate.AddDays(-10).AddHours(8),
                                    PublishedOn = BaseDate.AddDays(-10).AddHours(8),
                                    IsActive = true,
                                    PostId = dbPosts[10].PostId,
                                    UserId = dbAdditionalUsers[3].UserId
                                },
                                new Comment
                                {
                                    Content = "Would you recommend a specific password manager for teams? We're looking to implement better security across our organization.",
                                    CreatedAt = BaseDate.AddDays(-10).AddHours(10),
                                    PublishedOn = BaseDate.AddDays(-10).AddHours(10),
                                    IsActive = true,
                                    PostId = dbPosts[10].PostId,
                                    UserId = dbAdditionalUsers[0].UserId
                                }
                            };
                            
                            context.Comments.AddRange(comments);
                            await context.SaveChangesAsync();
                            
                            // Now get all comments to work with their IDs
                            var allComments = await context.Comments.ToListAsync();
                            
                            // Add replies (with parent comment references)
                            var replies = new List<Comment>();
                            
                            // Reply to comment on Post 4
                            var parentComment1 = allComments.FirstOrDefault(c => 
                                c.PostId == dbPosts[3].PostId && 
                                c.UserId == dbAdditionalUsers[0].UserId);
                                
                            if (parentComment1 != null)
                            {
                                replies.Add(new Comment
                                {
                                    Content = "Could you elaborate more on that point? I'm curious about your alternative solution for larger applications.",
                                    CreatedAt = BaseDate.AddDays(-3).AddHours(5),
                                    PublishedOn = BaseDate.AddDays(-3).AddHours(5),
                                    IsActive = true,
                                    PostId = dbPosts[3].PostId,
                                    UserId = dbDeveloperUser.UserId,
                                    ParentCommentId = parentComment1.CommentId
                                });
                            }
                            
                            // Reply to comment on Post 6
                            var parentComment2 = allComments.FirstOrDefault(c => 
                                c.PostId == dbPosts[5].PostId && 
                                c.UserId == dbAdditionalUsers[2].UserId);
                                
                            if (parentComment2 != null)
                            {
                                replies.Add(new Comment
                                {
                                    Content = "I'd recommend starting with scikit-learn for traditional ML, TensorFlow or PyTorch for deep learning, and XGBoost for gradient boosting. Depending on your specific needs, libraries like NLTK (for NLP) or OpenCV (for computer vision) could also be valuable additions.",
                                    CreatedAt = BaseDate.AddDays(-5).AddHours(4),
                                    PublishedOn = BaseDate.AddDays(-5).AddHours(4),
                                    IsActive = true,
                                    PostId = dbPosts[5].PostId,
                                    UserId = dbAdditionalUsers[0].UserId,
                                    ParentCommentId = parentComment2.CommentId
                                });
                            }
                            
                            // Reply to comment on Post 7
                            var parentComment3 = allComments.FirstOrDefault(c => 
                                c.PostId == dbPosts[6].PostId && 
                                c.UserId == dbAdditionalUsers[3].UserId);
                                
                            if (parentComment3 != null)
                            {
                                replies.Add(new Comment
                                {
                                    Content = "Try using multi-stage builds, smaller base images like Alpine, and be careful with what you copy into the image. Also, combine RUN commands with && to reduce layers.",
                                    CreatedAt = BaseDate.AddDays(-6).AddHours(4),
                                    PublishedOn = BaseDate.AddDays(-6).AddHours(4),
                                    IsActive = true,
                                    PostId = dbPosts[6].PostId,
                                    UserId = dbAdditionalUsers[1].UserId,
                                    ParentCommentId = parentComment3.CommentId
                                });
                            }
                            
                            context.Comments.AddRange(replies);
                            await context.SaveChangesAsync();
                            
                            // Get all comments again including replies
                            allComments = await context.Comments.ToListAsync();
                            
                            // Add post reactions
                            var postReactions = new List<PostReaction>();
                            
                            // Combine all posts
                            var allPosts = await context.Posts.ToListAsync();
                            
                            // Add reactions for each post
                            foreach (var post in allPosts)
                            {
                                // Add some likes and dislikes with different patterns
                                if (post.PostId % 3 == 0)
                                {
                                    // Pattern 1: Admin likes, regular user likes, another user dislikes
                                    postReactions.Add(new PostReaction
                                    {
                                        IsLike = true,
                                        CreatedAt = BaseDate.AddDays(-1),
                                        PostId = post.PostId,
                                        UserId = dbAdminUser.UserId
                                    });
                                    
                                    postReactions.Add(new PostReaction
                                    {
                                        IsLike = true,
                                        CreatedAt = BaseDate.AddDays(-2),
                                        PostId = post.PostId,
                                        UserId = dbRegularUser.UserId
                                    });
                                    
                                    postReactions.Add(new PostReaction
                                    {
                                        IsLike = false,
                                        CreatedAt = BaseDate.AddDays(-3),
                                        PostId = post.PostId,
                                        UserId = dbAdditionalUsers[0].UserId
                                    });
                                }
                                else if (post.PostId % 3 == 1)
                                {
                                    // Pattern 2: Regular user likes, additional users like
                                    postReactions.Add(new PostReaction
                                    {
                                        IsLike = true,
                                        CreatedAt = BaseDate.AddDays(-1),
                                        PostId = post.PostId,
                                        UserId = dbRegularUser.UserId
                                    });
                                    
                                    postReactions.Add(new PostReaction
                                    {
                                        IsLike = true,
                                        CreatedAt = BaseDate.AddDays(-2),
                                        PostId = post.PostId,
                                        UserId = dbAdditionalUsers[1].UserId
                                    });
                                    
                                    postReactions.Add(new PostReaction
                                    {
                                        IsLike = true,
                                        CreatedAt = BaseDate.AddDays(-3),
                                        PostId = post.PostId,
                                        UserId = dbAdditionalUsers[2].UserId
                                    });
                                }
                                else
                                {
                                    // Pattern 3: Mix of likes and dislikes
                                    postReactions.Add(new PostReaction
                                    {
                                        IsLike = true,
                                        CreatedAt = BaseDate.AddDays(-1),
                                        PostId = post.PostId,
                                        UserId = dbAdditionalUsers[3].UserId
                                    });
                                    
                                    postReactions.Add(new PostReaction
                                    {
                                        IsLike = false,
                                        CreatedAt = BaseDate.AddDays(-2),
                                        PostId = post.PostId,
                                        UserId = dbDeveloperUser.UserId
                                    });
                                    
                                    postReactions.Add(new PostReaction
                                    {
                                        IsLike = true,
                                        CreatedAt = BaseDate.AddDays(-3),
                                        PostId = post.PostId,
                                        UserId = dbAdminUser.UserId
                                    });
                                }
                            }
                            
                            context.PostReactions.AddRange(postReactions);
                            await context.SaveChangesAsync();
                            
                            // Add comment reactions
                            var commentReactions = new List<CommentReaction>();
                            
                            // Add reactions to some comments
                            for (int i = 0; i < Math.Min(allComments.Count, 10); i++)
                            {
                                // Add a few reactions to comments
                                commentReactions.Add(new CommentReaction
                                {
                                    IsLike = true,
                                    CreatedAt = BaseDate.AddDays(-i % 5),
                                    CommentId = allComments[i].CommentId,
                                    UserId = i % 2 == 0 ? dbAdminUser.UserId : dbRegularUser.UserId
                                });
                                
                                // Add some additional reactions to make it more realistic
                                if (i % 3 == 0)
                                {
                                    commentReactions.Add(new CommentReaction
                                    {
                                        IsLike = i % 2 == 0,
                                        CreatedAt = BaseDate.AddDays(-(i+1) % 5),
                                        CommentId = allComments[i].CommentId,
                                        UserId = dbAdditionalUsers[i % dbAdditionalUsers.Count].UserId
                                    });
                                }
                            }
                            
                            context.CommentReactions.AddRange(commentReactions);
                            await context.SaveChangesAsync();
                            
                            Console.WriteLine("Seed data successfully added: Comments, PostReactions, and CommentReactions");
                        }
                        else
                        {
                            Console.WriteLine("Could not find required users for seeding comments and reactions");
                        }
                    }
                }
            }
        }
    }
} 