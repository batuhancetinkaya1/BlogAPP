using BlogApp.Entity;

namespace BlogApp.Models.ViewModels
{
    public class DashboardViewModel
    {
        public int UserCount { get; set; }
        public int PostCount { get; set; }
        public int TagCount { get; set; }
        public List<Post> RecentPosts { get; set; } = new List<Post>();
        public List<User> RecentUsers { get; set; } = new List<User>();
    }
} 