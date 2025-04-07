using BlogApp.Data.Abstract;
using Microsoft.AspNetCore.Mvc;

namespace BlogApp.Views.Shared.Components.TagsHome;

public class TagsHome : ViewComponent
{
    private readonly ITagRepository _tagRepository;

    public TagsHome(ITagRepository tagRepository)
    {
        _tagRepository = tagRepository;
    }

    public async Task<IViewComponentResult> InvokeAsync()
    {
        var allTags = await _tagRepository.GetAllAsync();
        var popularTags = allTags
            .Where(t => t.IsActive)
            .OrderByDescending(t => t.Posts.Count)
            .Take(10)
            .ToList();
        return View(popularTags);
    }
} 