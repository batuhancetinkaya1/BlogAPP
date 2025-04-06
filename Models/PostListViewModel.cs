using BlogApp.Entity;

namespace BlogApp.Models;

public class PostListViewModel
{
    public List<Post> Posts { get; set; } = new();
    public PageInfo PageInfo { get; set; } = new();
}

public class PageInfo
{
    public int TotalItems { get; set; }
    public int ItemsPerPage { get; set; }
    public int TotalPages { get; set; }
    public int CurrentPage { get; set; }
    public bool HasPreviousPage => CurrentPage > 1;
    public bool HasNextPage => CurrentPage < TotalPages;
} 