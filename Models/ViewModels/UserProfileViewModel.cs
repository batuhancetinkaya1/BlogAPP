using BlogApp.Entity;

namespace BlogApp.Models.ViewModels
{
    public class UserProfileViewModel
    {
        public User User { get; set; } = null!;
        public List<Post> Posts { get; set; } = new List<Post>();
    }
} 