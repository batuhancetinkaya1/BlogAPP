using BlogApp.Entity;

namespace BlogApp.Models.ViewModels
{
    public class PostListViewModel
    {
        public PostListViewModel()
        {
            Posts = new List<Post>();
            CurrentPage = 1;
            CurrentSort = "date";
            CurrentTag = string.Empty;
        }

        public List<Post> Posts { get; set; } = new List<Post>();
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public string CurrentSort { get; set; } = string.Empty;
        public string CurrentTag { get; set; } = string.Empty;
    }
} 