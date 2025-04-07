using BlogApp.Data.Abstract;
using Microsoft.AspNetCore.Mvc;

namespace BlogApp.Views.Shared.Components.NewPosts;

public class NewPosts : ViewComponent
{
    private readonly IPostRepository _postRepository;

    public NewPosts(IPostRepository postRepository)
    {
        _postRepository = postRepository;
    }

    public async Task<IViewComponentResult> InvokeAsync()
    {
        var allPosts = await _postRepository.GetAllAsync();
        var recentPosts = allPosts
            .OrderByDescending(p => p.CreatedAt)
            .Take(3)
            .ToList();
        return View(recentPosts);
    }
} 