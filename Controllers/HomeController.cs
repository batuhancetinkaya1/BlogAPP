using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using BlogApp.Data.Abstract;
using BlogApp.Entity;
using BlogApp.Models.ViewModels;

namespace BlogApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly IPostRepository _postRepository;
        private readonly ITagRepository _tagRepository;
        private readonly IUserRepository _userRepository;

        public HomeController(IPostRepository postRepository, ITagRepository tagRepository, IUserRepository userRepository)
        {
            _postRepository = postRepository;
            _tagRepository = tagRepository;
            _userRepository = userRepository;
        }

        public async Task<IActionResult> Index(string sort = "date", int page = 1)
        {
            var allPosts = await _postRepository.GetAllAsync();
            var allTags = await _tagRepository.GetAllAsync();
            var allUsers = await _userRepository.GetAllAsync();
            
            // Sadece aktif ve yayınlanmış postları göster
            var filteredPosts = allPosts
                .Where(p => p.IsActive && p.Status == PostStatus.Published)
                .ToList();
            
            // Sıralama uygula
            filteredPosts = sort switch
            {
                "title" => filteredPosts.OrderBy(p => p.Title).ToList(),
                "likes" => filteredPosts.OrderByDescending(p => p.Reactions.Count(r => r.IsLike)).ToList(),
                "comments" => filteredPosts.OrderByDescending(p => p.Comments.Count).ToList(),
                _ => filteredPosts.OrderByDescending(p => p.PublishedOn).ToList()
            };

            // Pagination için ayarlar
            int pageSize = 3; // Sayfa başına 3 post
            var totalPosts = filteredPosts.Count;
            var totalPages = (int)Math.Ceiling(totalPosts / (double)pageSize);
            
            // Geçerli sayfadaki postları al
            var pagedPosts = filteredPosts
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var model = new PostListViewModel
            {
                Posts = pagedPosts,
                CurrentPage = page,
                TotalPages = totalPages,
                CurrentSort = sort,
                CurrentTag = string.Empty
            };

            // İstatistikler için ViewBag kullan
            ViewBag.TotalPosts = totalPosts;
            ViewBag.TotalUsers = allUsers.Count;
            ViewBag.TotalTags = allTags.Count;

            return View(model);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [Route("gizlilik")]
        public IActionResult Gizlilik()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [HttpGet]
        public async Task<IActionResult> GetPosts(string sort = "date", int page = 1)
        {
            var allPosts = await _postRepository.GetAllAsync();
            
            // Sadece aktif ve yayınlanmış postları göster
            var filteredPosts = allPosts
                .Where(p => p.IsActive && p.Status == PostStatus.Published)
                .ToList();
            
            // Sıralama uygula
            filteredPosts = sort switch
            {
                "title" => filteredPosts.OrderBy(p => p.Title).ToList(),
                "likes" => filteredPosts.OrderByDescending(p => p.Reactions.Count(r => r.IsLike)).ToList(),
                "comments" => filteredPosts.OrderByDescending(p => p.Comments.Count).ToList(),
                _ => filteredPosts.OrderByDescending(p => p.PublishedOn).ToList()
            };

            // Pagination için ayarlar
            int pageSize = 3; // Sayfa başına 3 post
            var totalPosts = filteredPosts.Count;
            var totalPages = (int)Math.Ceiling(totalPosts / (double)pageSize);
            
            // Geçerli sayfadaki postları al
            var pagedPosts = filteredPosts
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var model = new PostListViewModel
            {
                Posts = pagedPosts,
                CurrentPage = page,
                TotalPages = totalPages,
                CurrentSort = sort,
                CurrentTag = string.Empty
            };

            return PartialView("_PostsList", model);
        }
    }
} 