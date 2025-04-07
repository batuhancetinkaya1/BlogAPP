using BlogApp.Data.Abstract;
using Microsoft.AspNetCore.Mvc;

namespace BlogApp.Views.Shared.Components.TagsCloud;

public class TagsCloud : ViewComponent
{
    private readonly ITagRepository _tagRepository;

    public TagsCloud(ITagRepository tagRepository)
    {
        _tagRepository = tagRepository;
    }

    public async Task<IViewComponentResult> InvokeAsync()
    {
        var allTags = await _tagRepository.GetAllAsync();
        var popularTags = allTags
            .Where(t => t.IsActive)
            .OrderByDescending(t => t.Posts.Count)
            .Take(20)
            .ToList();
        return View(popularTags);
    }
} 