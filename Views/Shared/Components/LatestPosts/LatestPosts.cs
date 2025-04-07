using BlogApp.Data.Abstract;
using BlogApp.Entity;
using Microsoft.AspNetCore.Mvc;

namespace BlogApp.Views.Shared.Components.LatestPosts;

public class LatestPosts : ViewComponent
{
    private readonly IPostRepository _postRepository;

    public LatestPosts(IPostRepository postRepository)
    {
        _postRepository = postRepository;
    }

    public async Task<IViewComponentResult> InvokeAsync()
    {
        var allPosts = await _postRepository.GetAllAsync();
        var latestPosts = allPosts
            .Where(p => p.IsActive && p.Status == PostStatus.Published)
            .OrderByDescending(p => p.PublishedOn)
            .Take(3)
            .ToList();
        return View(latestPosts);
    }
} 