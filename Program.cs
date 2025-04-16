using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.Logging;
using BlogApp.Data;
using BlogApp.Data.Abstract;
using BlogApp.Data.Concrete.EfCore;
using BlogApp.Data.SeedData;
using BlogApp.Entity;
using Microsoft.AspNetCore.Antiforgery;
using BlogApp.Middleware;
using BlogApp.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews()
    .AddJsonOptions(options => {
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
    });

// CSRF token ayarları için AntiForgery hizmetini yapılandırma
builder.Services.AddAntiforgery(options => {
    options.HeaderName = "X-CSRF-TOKEN";
    options.SuppressXFrameOptionsHeader = false;
    options.Cookie.Name = "CSRF-TOKEN";
    options.Cookie.HttpOnly = false; // JavaScript'in tokeni okumasına izin ver
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest; // Allow HTTP in development
    options.Cookie.SameSite = SameSiteMode.Lax; // Less strict for better compatibility
});

// Configure DbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection"));
    // Performance optimizasyonu için query splitting behavior'u ayarla
    options.ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.MultipleCollectionIncludeWarning));
    
    if (builder.Environment.IsDevelopment())
    {
        options.EnableSensitiveDataLogging();
        options.EnableDetailedErrors();
    }
});

// Configure Authentication
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Users/Login";
        options.LogoutPath = "/Users/LogoutConfirm";
        options.AccessDeniedPath = "/Users/AccessDenied";
        options.Cookie.HttpOnly = true;
        options.ExpireTimeSpan = TimeSpan.FromDays(7);
        options.SlidingExpiration = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest; // Allow HTTP in development
        options.Cookie.SameSite = SameSiteMode.Lax; // Less strict for better compatibility
    });

// Configure Authorization
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy =>
        policy.RequireRole("Admin"));
});

// Register Repositories
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IPostRepository, PostRepository>();
builder.Services.AddScoped<ITagRepository, TagRepository>();
builder.Services.AddScoped<ICommentRepository, CommentRepository>();

// Register Services
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<INotificationService, TempDataNotificationService>();

var app = builder.Build();

// Seed the database
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        // Apply migrations and create database if it doesn't exist
        var context = services.GetRequiredService<ApplicationDbContext>();
        context.Database.EnsureCreated();
        
        await SeedData.InitializeAsync(services);
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogInformation("Database seeded successfully.");
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while seeding the database.");
    }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}
else
{
    app.UseDeveloperExceptionPage();
}

// Use custom error handling middleware
app.UseErrorHandling();

// app.UseHttpsRedirection(); // HTTPS yönlendirme devre dışı bırakıldı
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

// AntiForgery özelleştirilmiş middleware
app.Use(async (context, next) =>
{
    var path = context.Request.Path.Value ?? string.Empty;
    if (context.Request.Method == "POST" && 
        (path.Contains("/Posts/AddReaction") || 
         path.Contains("/Posts/AddComment") || 
         path.Contains("/Posts/AddCommentReaction")))
    {
        // CSRF token'ı çek
        var antiforgery = context.RequestServices.GetRequiredService<IAntiforgery>();
        try 
        {
            await antiforgery.ValidateRequestAsync(context);
            Console.WriteLine($"CSRF token doğrulaması başarılı: {path}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"CSRF token doğrulama hatası: {ex.Message}");
            context.Response.StatusCode = 400;
            await context.Response.WriteAsJsonAsync(new { success = false, message = "CSRF token doğrulaması başarısız" });
            return;
        }
    }

    await next(context);
});

// CSRF middleware'ini ekleyelim
app.UseAntiforgery();

// Route configurations
app.MapControllerRoute(
    name: "delete_post",
    pattern: "Posts/Delete/{id:int}",
    defaults: new { controller = "Posts", action = "Delete" });

app.MapControllerRoute(
    name: "archive_post",
    pattern: "Posts/Archive/{id:int}",
    defaults: new { controller = "Posts", action = "Archive" });

app.MapControllerRoute(
    name: "publish_post",
    pattern: "Posts/Publish/{id:int}",
    defaults: new { controller = "Posts", action = "Publish" });

app.MapControllerRoute(
    name: "user_profile",
    pattern: "users/{username}",
    defaults: new { controller = "Users", action = "Profile" });

app.MapControllerRoute(
    name: "tags_create",
    pattern: "tags/create",
    defaults: new { controller = "Tags", action = "Create" });

app.MapControllerRoute(
    name: "tags_edit",
    pattern: "tags/edit/{id:int}",
    defaults: new { controller = "Tags", action = "Edit" });

app.MapControllerRoute(
    name: "tags_delete",
    pattern: "tags/delete/{id:int}",
    defaults: new { controller = "Tags", action = "Delete" });

app.MapControllerRoute(
    name: "tags_detail",
    pattern: "tags/{url}",
    defaults: new { controller = "Tags", action = "Detail" });

app.MapControllerRoute(
    name: "tags_list",
    pattern: "Tags",
    defaults: new { controller = "Tags", action = "Index" });

app.MapControllerRoute(
    name: "tags_index",
    pattern: "tags/index",
    defaults: new { controller = "Tags", action = "Index" });

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "posts",
    pattern: "posts/{url}",
    defaults: new { controller = "Posts", action = "Details" });

app.MapControllerRoute(
    name: "post_reactions",
    pattern: "Posts/AddReaction",
    defaults: new { controller = "Posts", action = "AddReaction" });

app.MapControllerRoute(
    name: "post_comments",
    pattern: "Posts/AddComment",
    defaults: new { controller = "Posts", action = "AddComment" });

app.MapControllerRoute(
    name: "comment_reactions",
    pattern: "Posts/AddCommentReaction",
    defaults: new { controller = "Posts", action = "AddCommentReaction" });

app.MapControllerRoute(
    name: "users_login",
    pattern: "Users/Login",
    defaults: new { controller = "Users", action = "Login" });

app.MapControllerRoute(
    name: "users_register",
    pattern: "Users/Register",
    defaults: new { controller = "Users", action = "Register" });

app.MapControllerRoute(
    name: "users_logout",
    pattern: "Users/Logout",
    defaults: new { controller = "Users", action = "LogoutConfirm" });

app.MapControllerRoute(
    name: "users_myposts",
    pattern: "Users/MyPosts",
    defaults: new { controller = "Users", action = "MyPosts" });

app.MapControllerRoute(
    name: "posts_create",
    pattern: "Posts/Create",
    defaults: new { controller = "Posts", action = "Create" });

app.MapControllerRoute(
    name: "posts_search",
    pattern: "Posts/Search",
    defaults: new { controller = "Posts", action = "Search" });

app.MapControllerRoute(
    name: "posts_bulkdelete",
    pattern: "posts/bulkdelete",
    defaults: new { controller = "Posts", action = "BulkDelete" });

app.MapControllerRoute(
    name: "posts_bulkpublish",
    pattern: "posts/bulkpublish",
    defaults: new { controller = "Posts", action = "BulkPublish" });

app.MapControllerRoute(
    name: "posts_bulkarchive",
    pattern: "posts/bulkarchive",
    defaults: new { controller = "Posts", action = "BulkArchive" });

await app.RunAsync();