using BlogApp.Data.Abstract;
using BlogApp.Entity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.RegularExpressions;
using System.Linq;

namespace BlogApp.Controllers;

public class TagsController : Controller
{
    private readonly ITagRepository _tagRepository;

    public TagsController(ITagRepository tagRepository)
    {
        _tagRepository = tagRepository;
    }

    public IActionResult Index()
    {
        var tags = _tagRepository.Tags.ToList();
        return View(tags);
    }

    [Authorize(Roles = "Admin")]
    public IActionResult Create()
    {
        return View();
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public IActionResult Create(Tag tag)
    {
        if (!ModelState.IsValid)
        {
            return View(tag);
        }

        tag.Url = GenerateUrl(tag.Name);
        _tagRepository.CreateTag(tag);
        return RedirectToAction("Index");
    }

    [Authorize(Roles = "Admin")]
    public IActionResult Edit(int id)
    {
        var tag = _tagRepository.GetById(id);
        if (tag == null)
        {
            return NotFound();
        }

        return View(tag);
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public IActionResult Edit(int id, Tag tag)
    {
        if (id != tag.TagId)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            // Generate URL from name if not provided
            if (string.IsNullOrEmpty(tag.Url))
            {
                tag.Url = GenerateUrl(tag.Name);
            }
            
            var existingTag = _tagRepository.GetByUrl(tag.Url);
            if (existingTag != null && existingTag.TagId != id)
            {
                ModelState.AddModelError("Url", "Bu URL zaten kullanımda");
                return View(tag);
            }

            _tagRepository.EditTag(tag);
            TempData["Message"] = "Etiket başarıyla güncellendi.";
            TempData["MessageType"] = "success";
            return RedirectToAction(nameof(Index));
        }

        return View(tag);
    }

    [Authorize(Roles = "Admin")]
    public IActionResult Delete(int id)
    {
        var tag = _tagRepository.GetById(id);
        if (tag == null)
        {
            return NotFound();
        }

        return View(tag);
    }

    [Authorize(Roles = "Admin")]
    [HttpPost, ActionName("Delete")]
    public IActionResult DeleteConfirmed(int id)
    {
        var tag = _tagRepository.GetById(id);
        if (tag == null)
        {
            return NotFound();
        }

        _tagRepository.DeleteTag(id);
        TempData["Message"] = "Etiket başarıyla silindi.";
        TempData["MessageType"] = "success";
        return RedirectToAction(nameof(Index));
    }

    private static string GenerateUrl(string text)
    {
        if (string.IsNullOrEmpty(text))
            return string.Empty;

        text = text.ToLower();
        text = Regex.Replace(text, @"[^a-z0-9\s-]", "");
        text = Regex.Replace(text, @"\s+", " ").Trim();
        text = Regex.Replace(text, @"\s", "-");
        return text;
    }
} 