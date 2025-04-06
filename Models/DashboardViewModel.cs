using BlogApp.Entity;

namespace BlogApp.Models;

public class DashboardViewModel
{
    public int TotalUsers { get; set; }
    public int TotalPosts { get; set; }
    public int TotalTags { get; set; }
    public List<Post> RecentPosts { get; set; } = new();
    public List<User> RecentUsers { get; set; } = new();
} 