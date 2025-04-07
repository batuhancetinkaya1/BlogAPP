using BlogApp.Data.Abstract;
using Microsoft.AspNetCore.Mvc;

namespace BlogApp.Views.Shared.Components.PopularPosts;

public class PopularPosts : ViewComponent
{
    private readonly IPostRepository _postRepository;

    public PopularPosts(IPostRepository postRepository)
    {
        _postRepository = postRepository;
    }

    public async Task<IViewComponentResult> InvokeAsync()
    {
        var allPosts = await _postRepository.GetAllAsync();
        var popularPosts = allPosts
            .Where(p => p.IsActive && p.Status == BlogApp.Entity.PostStatus.Published)
            .OrderByDescending(p => p.Reactions.Count(r => r.IsLike) + p.Comments.Count)
            .Take(3)
            .ToList();
        return View(popularPosts);
    }
} 