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

    public IViewComponentResult Invoke()
    {
        return View(_postRepository.Posts
            .Where(p => p.IsActive)
            .OrderByDescending(p => p.PublishedOn)
            .Take(5)
            .ToList());
    }
} 