using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.Logging;
using BlogApp.Data;
using BlogApp.Data.Abstract;
using BlogApp.Data.Concrete.EfCore;
using BlogApp.Data.SeedData;
using BlogApp.Entity;
using Microsoft.AspNetCore.Antiforgery;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// CSRF token ayarları için AntiForgery hizmetini yapılandırma
builder.Services.AddAntiforgery(options => 
{
    options.HeaderName = "X-CSRF-TOKEN";
    options.SuppressXFrameOptionsHeader = false;
    options.Cookie.Name = "CSRF-TOKEN";
    options.Cookie.HttpOnly = false; // JavaScript'in tokeni okumasına izin ver
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
    options.Cookie.SameSite = SameSiteMode.Lax;
});

// Configure DbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection"));
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

// app.UseHttpsRedirection(); // HTTPS yönlendirme devre dışı bırakıldı
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

// AntiForgery özelleştirilmiş middleware
app.Use(async (context, next) =>
{
    var path = context.Request.Path.Value;
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

// Tüm controller route'larından önce "{controller=Home}/{action=Index}/{id?}" pattern'ı ekleyelim
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
    name: "tags",
    pattern: "tags/{url}",
    defaults: new { controller = "Tags", action = "Detail" });

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

await app.RunAsync();