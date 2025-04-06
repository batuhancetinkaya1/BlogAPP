using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;
using BlogApp.Data;
using BlogApp.Data.Abstract;
using BlogApp.Data.Concrete;
using BlogApp.Data.SeedData;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Add DbContext
builder.Services.AddDbContext<BlogContext>(options =>
    options.UseSqlite("Data Source=blog.db"));

// Add Repositories
builder.Services.AddScoped<IUserRepository, EfUserRepository>();
builder.Services.AddScoped<IPostRepository, EfPostRepository>();
builder.Services.AddScoped<ITagRepository, EfTagRepository>();
builder.Services.AddScoped<ICommentRepository, EfCommentRepository>();

// Add Authentication
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Users/Login";
        options.LogoutPath = "/Users/Logout";
        options.AccessDeniedPath = "/Users/AccessDenied";
        options.Cookie.Name = "BlogApp.Auth";
        options.Cookie.HttpOnly = true;
        options.ExpireTimeSpan = TimeSpan.FromDays(7);
        options.SlidingExpiration = true;
    });

// Add Authorization
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
});

var app = builder.Build();

// Ensure required directories exist
EnsureDirectoriesExist();

// Seed the database
await SeedData.SeedAsync(app.Services);

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

await app.RunAsync();

// Helper method to ensure required directories exist
void EnsureDirectoriesExist()
{
    var imgPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "img");
    if (!Directory.Exists(imgPath))
    {
        Directory.CreateDirectory(imgPath);
    }
}