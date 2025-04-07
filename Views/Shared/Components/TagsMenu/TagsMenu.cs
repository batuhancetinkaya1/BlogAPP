using BlogApp.Data.Abstract;
using Microsoft.AspNetCore.Mvc;

namespace BlogApp.Views.Shared.Components.TagsMenu;

public class TagsMenu : ViewComponent
{
    private readonly ITagRepository _tagRepository;

    public TagsMenu(ITagRepository tagRepository)
    {
        _tagRepository = tagRepository;
    }

    public async Task<IViewComponentResult> InvokeAsync()
    {
        var tags = await _tagRepository.GetAllAsync();
        return View(tags);
    }
} 