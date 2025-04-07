using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BlogApp.Data.Abstract;
using BlogApp.Entity;
using BlogApp.Models.ViewModels;

namespace BlogApp.Controllers
{
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

        public async Task<IActionResult> Index()
        {
            var users = await _userRepository.GetAllAsync();
            var posts = await _postRepository.GetAllAsync();
            var tags = await _tagRepository.GetAllAsync();

            ViewBag.UserCount = users.Count;
            ViewBag.PostCount = posts.Count;
            ViewBag.TagCount = tags.Count;
            ViewBag.RecentPosts = posts
                .OrderByDescending(p => p.CreatedAt)
                .Take(5)
                .ToList();
            ViewBag.RecentUsers = users
                .OrderByDescending(u => u.CreatedAt)
                .Take(5)
                .ToList();

            return View();
        }

        public async Task<IActionResult> Users(string searchTerm = "", int pageNumber = 1, int pageSize = 10)
        {
            var users = await _userRepository.GetAllAsync();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                users = users.Where(u => 
                    u.UserName.Contains(searchTerm) || 
                    u.Email.Contains(searchTerm)).ToList();
            }

            var totalUsers = users.Count;
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

        public async Task<IActionResult> EditUser(int id)
        {
            var user = await _userRepository.GetByIdAsync(id);
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

            var user = await _userRepository.GetByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            user.UserName = model.UserName;
            user.Email = model.Email;
            user.IsAdmin = model.IsAdmin;
            user.IsActive = model.IsActive;

            await _userRepository.UpdateAsync(user);

            TempData["success"] = "Kullanıcı başarıyla güncellendi.";
            return RedirectToAction(nameof(Users));
        }

        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            await _userRepository.DeleteAsync(user);

            TempData["success"] = "Kullanıcı başarıyla silindi.";
            return RedirectToAction(nameof(Users));
        }

        public async Task<IActionResult> Posts(string searchTerm = "", int pageNumber = 1, int pageSize = 10)
        {
            var posts = await _postRepository.GetAllAsync();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                posts = posts.Where(p => 
                    p.Title.Contains(searchTerm) || 
                    p.Content.Contains(searchTerm) ||
                    p.Description.Contains(searchTerm)).ToList();
            }

            var totalPosts = posts.Count;
            var paginatedPosts = posts
                .OrderByDescending(p => p.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            ViewBag.SearchTerm = searchTerm;
            ViewBag.TotalPosts = totalPosts;
            ViewBag.CurrentPage = pageNumber;
            ViewBag.TotalPages = (int)Math.Ceiling(totalPosts / (double)pageSize);

            return View(paginatedPosts);
        }

        public async Task<IActionResult> Tags(string searchTerm = "", int pageNumber = 1, int pageSize = 10)
        {
            var tags = await _tagRepository.GetAllAsync();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                tags = tags.Where(t => t.Name.Contains(searchTerm)).ToList();
            }

            var totalTags = tags.Count;
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
} 