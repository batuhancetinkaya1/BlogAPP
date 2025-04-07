using BlogApp.Data.Abstract;
using BlogApp.Entity;
using Microsoft.AspNetCore.Mvc;

namespace BlogApp.Views.Shared.Components.FeaturedPosts;

public class FeaturedPosts : ViewComponent
{
    private readonly IPostRepository _postRepository;

    public FeaturedPosts(IPostRepository postRepository)
    {
        _postRepository = postRepository;
    }

    public async Task<IViewComponentResult> InvokeAsync()
    {
        var allPosts = await _postRepository.GetAllAsync();
        var featuredPosts = allPosts
            .Where(p => p.IsActive && p.Status == PostStatus.Published)
            .OrderByDescending(p => (p.Reactions.Count(r => r.IsLike) * 2) + p.Comments.Count)
            .Take(6)
            .ToList();
        return View(featuredPosts);
    }
} 