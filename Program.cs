using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.Logging;
using BlogApp.Data;
using BlogApp.Data.Abstract;
using BlogApp.Data.Concrete.EfCore;
using BlogApp.Data.SeedData;
using BlogApp.Entity;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

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

app.MapControllerRoute(
    name: "posts",
    pattern: "posts/{url}",
    defaults: new { controller = "Posts", action = "Details" });

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

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

await app.RunAsync();