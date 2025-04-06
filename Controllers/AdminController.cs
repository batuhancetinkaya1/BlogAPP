using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BlogApp.Data.Abstract;
using BlogApp.Entity;
using BlogApp.Models;

namespace BlogApp.Controllers;

[Authorize(Roles = "Admin")]
public class AdminController : Controller
{
    private readonly IUserRepository _userRepository;
    private readonly IPostRepository _postRepository;
    private readonly ITagRepository _tagRepository;

    public AdminController(
        IUserRepository userRepository,
        IPostRepository postRepository,
        ITagRepository tagRepository)
    {
        _userRepository = userRepository;
        _postRepository = postRepository;
        _tagRepository = tagRepository;
    }

    public IActionResult Index()
    {
        var stats = new DashboardViewModel
        {
            TotalUsers = _userRepository.Users.Count(),
            TotalPosts = _postRepository.Posts.Count(),
            TotalTags = _tagRepository.Tags.Count(),
            RecentPosts = _postRepository.Posts
                .OrderByDescending(p => p.CreatedAt)
                .Take(5)
                .Include(p => p.User)
                .ToList(),
            RecentUsers = _userRepository.Users
                .OrderByDescending(u => u.CreatedAt)
                .Take(5)
                .ToList()
        };

        return View(stats);
    }

    public IActionResult Users(string searchTerm = "", int pageNumber = 1, int pageSize = 10)
    {
        var users = _userRepository.Users.AsQueryable();

        if (!string.IsNullOrEmpty(searchTerm))
        {
            users = users.Where(u => 
                u.UserName.Contains(searchTerm) || 
                u.Email.Contains(searchTerm));
        }

        var totalUsers = users.Count();
        var paginatedUsers = users
            .OrderByDescending(u => u.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        ViewBag.SearchTerm = searchTerm;
        ViewBag.TotalUsers = totalUsers;
        ViewBag.CurrentPage = pageNumber;
        ViewBag.TotalPages = (int)Math.Ceiling(totalUsers / (double)pageSize);

        return View(paginatedUsers);
    }

    public IActionResult EditUser(int id)
    {
        var user = _userRepository.GetById(id);
        if (user == null)
        {
            return NotFound();
        }

        return View(new UserEditViewModel
        {
            UserId = user.UserId,
            UserName = user.UserName,
            Email = user.Email,
            IsAdmin = user.IsAdmin,
            IsActive = user.IsActive
        });
    }

    [HttpPost]
    public async Task<IActionResult> EditUser(int id, UserEditViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var user = _userRepository.GetById(id);
        if (user == null)
        {
            return NotFound();
        }

        user.UserName = model.UserName;
        user.Email = model.Email;
        user.IsAdmin = model.IsAdmin;
        user.IsActive = model.IsActive;

        _userRepository.UpdateUser(user);
        await _userRepository.SaveChangesAsync();

        TempData["success"] = "Kullanıcı başarıyla güncellendi.";
        return RedirectToAction(nameof(Users));
    }

    public async Task<IActionResult> DeleteUser(int id)
    {
        var user = _userRepository.GetById(id);
        if (user == null)
        {
            return NotFound();
        }

        _userRepository.DeleteUser(id);
        await _userRepository.SaveChangesAsync();

        TempData["success"] = "Kullanıcı başarıyla silindi.";
        return RedirectToAction(nameof(Users));
    }

    public IActionResult Posts(string searchTerm = "", int pageNumber = 1, int pageSize = 10)
    {
        var posts = _postRepository.Posts.AsQueryable();

        if (!string.IsNullOrEmpty(searchTerm))
        {
            posts = posts.Where(p => 
                p.Title.Contains(searchTerm) || 
                p.Content.Contains(searchTerm) ||
                p.Description.Contains(searchTerm));
        }

        var totalPosts = posts.Count();
        var paginatedPosts = posts
            .OrderByDescending(p => p.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Include(p => p.User)
            .Include(p => p.Tags)
            .ToList();

        ViewBag.SearchTerm = searchTerm;
        ViewBag.TotalPosts = totalPosts;
        ViewBag.CurrentPage = pageNumber;
        ViewBag.TotalPages = (int)Math.Ceiling(totalPosts / (double)pageSize);

        return View(paginatedPosts);
    }

    public IActionResult Tags(string searchTerm = "", int pageNumber = 1, int pageSize = 10)
    {
        var tags = _tagRepository.Tags.AsQueryable();

        if (!string.IsNullOrEmpty(searchTerm))
        {
            tags = tags.Where(t => t.Name.Contains(searchTerm));
        }

        var totalTags = tags.Count();
        var paginatedTags = tags
            .OrderBy(t => t.Name)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        ViewBag.SearchTerm = searchTerm;
        ViewBag.TotalTags = totalTags;
        ViewBag.CurrentPage = pageNumber;
        ViewBag.TotalPages = (int)Math.Ceiling(totalTags / (double)pageSize);

        return View(paginatedTags);
    }
} 