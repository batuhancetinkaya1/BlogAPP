using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using BlogApp.Models;
using BlogApp.Data.Abstract;
using BlogApp.Entity;
using Microsoft.EntityFrameworkCore;

namespace BlogApp.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly IPostRepository _postRepository;

    public HomeController(ILogger<HomeController> logger, IPostRepository postRepository)
    {
        _logger = logger;
        _postRepository = postRepository;
    }

    public IActionResult Index()
    {
        var posts = _postRepository.Posts
            .Where(p => p.IsActive && p.Status == PostStatus.Published)
            .OrderByDescending(p => p.PublishedOn)
            .Take(10)
            .Include(p => p.User)
            .Include(p => p.Tags)
            .Include(p => p.Comments)
            .ToList();

        return View(posts);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
} 