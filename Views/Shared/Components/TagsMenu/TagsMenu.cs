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

    public IViewComponentResult Invoke()
    {
        return View(_tagRepository.Tags.ToList());
    }
} 