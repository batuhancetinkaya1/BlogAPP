using BlogApp.Entity;
using BlogApp.Models;
using BlogApp.Data.Abstract;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using System.IO;
using System.Linq;
using System.Security.Claims;

namespace BlogApp.Controllers;

public class PostsController : Controller
{
    private readonly IPostRepository _postRepository;
    private readonly ITagRepository _tagRepository;
    private readonly ICommentRepository _commentRepository;
    private readonly IUserRepository _userRepository;

    public PostsController(
        IPostRepository postRepository,
        ITagRepository tagRepository,
        ICommentRepository commentRepository,
        IUserRepository userRepository)
    {
        _postRepository = postRepository;
        _tagRepository = tagRepository;
        _commentRepository = commentRepository;
        _userRepository = userRepository;
    }

    public IActionResult Index(string tag = null)
    {
        var query = _postRepository.Posts
            .Include(p => p.User)
            .Include(p => p.Tags)
            .Include(p => p.Comments)
            .Include(p => p.Reactions)
            .Where(p => p.Status == PostStatus.Published);

        if (!string.IsNullOrEmpty(tag))
        {
            query = query.Where(p => p.Tags.Any(t => t.Url == tag));
        }

        var posts = query.OrderByDescending(p => p.PublishedOn).ToList();
        return View(posts);
    }

    public IActionResult Details(string url)
    {
        var post = _postRepository.Posts
            .Include(p => p.User)
            .Include(p => p.Tags)
            .Include(p => p.Comments)
                .ThenInclude(c => c.User)
            .Include(p => p.Reactions)
            .FirstOrDefault(p => p.Url == url && p.Status == PostStatus.Published);

        if (post == null)
        {
            return NotFound();
        }

        return View(post);
    }

    [Authorize]
    public IActionResult Create()
    {
        ViewBag.Tags = _tagRepository.Tags.ToList();
        return View(new PostCreateViewModel());
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> Create(PostCreateViewModel model, IFormFile? imageFile)
    {
        if (ModelState.IsValid)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var user = _userRepository.GetById(userId);
            if (user == null)
            {
                return NotFound();
            }

            var post = new Post
            {
                Title = model.Title,
                Content = model.Content,
                Description = model.Description,
                Url = GenerateUrl(model.Title),
                UserId = userId,
                CreatedAt = DateTime.UtcNow,
                IsActive = true,
                Status = model.Status,
                ScheduledPublishTime = model.ScheduledPublishTime
            };

            if (model.ScheduledPublishTime.HasValue)
            {
                post.Status = PostStatus.Scheduled;
            }

            // Handle image upload
            if (imageFile != null)
            {
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);
                var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/img", fileName);
                using (var stream = new FileStream(path, FileMode.Create))
                {
                    await imageFile.CopyToAsync(stream);
                }
                post.Image = fileName;
            }

            // Add selected tags
            if (model.SelectedTagIds != null)
            {
                foreach (var tagId in model.SelectedTagIds)
                {
                    var tag = _tagRepository.GetById(tagId);
                    if (tag != null)
                    {
                        post.Tags.Add(tag);
                    }
                }
            }

            _postRepository.CreatePost(post);
            await _postRepository.SaveChangesAsync();

            TempData["success"] = "Yazı başarıyla oluşturuldu.";
            return RedirectToAction(nameof(Details), new { url = post.Url });
        }

        // If we got this far, something failed, redisplay form
        ViewBag.Tags = _tagRepository.Tags.ToList();
        return View(model);
    }

    [Authorize]
    public IActionResult Edit(int id)
    {
        var post = _postRepository.GetById(id);
        if (post == null)
        {
            return NotFound();
        }

        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        if (post.UserId != userId && !User.IsInRole("Admin"))
        {
            return Forbid();
        }

        var model = new PostEditViewModel
        {
            PostId = post.PostId,
            Title = post.Title,
            Content = post.Content,
            Description = post.Description,
            Image = post.Image,
            Url = post.Url,
            Status = post.Status,
            ScheduledPublishTime = post.ScheduledPublishTime,
            SelectedTagIds = post.Tags.Select(t => t.TagId).ToList(),
            CreatedAt = post.CreatedAt,
            PublishedOn = post.PublishedOn,
            IsActive = post.IsActive,
            AvailableTags = _tagRepository.Tags.ToList()
        };

        return View(model);
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> Edit(int id, PostEditViewModel model, IFormFile? imageFile)
    {
        if (ModelState.IsValid)
        {
            var post = _postRepository.GetById(id);
            if (post == null)
            {
                return NotFound();
            }

            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            if (post.UserId != userId && !User.IsInRole("Admin"))
            {
                return Forbid();
            }

            post.Title = model.Title;
            post.Content = model.Content;
            post.Description = model.Description;
            post.Url = model.Url;
            post.Status = model.Status;
            post.IsActive = model.IsActive;
            post.ScheduledPublishTime = model.ScheduledPublishTime;

            if (model.ScheduledPublishTime.HasValue)
            {
                post.Status = PostStatus.Scheduled;
            }

            // Handle image upload
            if (imageFile != null)
            {
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);
                var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/img", fileName);
                using (var stream = new FileStream(path, FileMode.Create))
                {
                    await imageFile.CopyToAsync(stream);
                }
                post.Image = fileName;
            }

            // Update tags
            post.Tags.Clear();
            if (model.SelectedTagIds != null)
            {
                foreach (var tagId in model.SelectedTagIds)
                {
                    var tag = _tagRepository.GetById(tagId);
                    if (tag != null)
                    {
                        post.Tags.Add(tag);
                    }
                }
            }

            _postRepository.EditPost(post);
            await _postRepository.SaveChangesAsync();

            TempData["success"] = "Yazı başarıyla güncellendi.";
            return RedirectToAction(nameof(Details), new { url = post.Url });
        }

        // If we got this far, something failed, redisplay form
        model.AvailableTags = _tagRepository.Tags.ToList();
        return View(model);
    }

    [Authorize]
    public async Task<IActionResult> Delete(int id)
    {
        var post = _postRepository.GetById(id);
        if (post == null)
        {
            return NotFound();
        }

        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        if (post.UserId != userId && !User.IsInRole("Admin"))
        {
            return Forbid();
        }

        _postRepository.DeletePost(id);
        await _postRepository.SaveChangesAsync();

        TempData["success"] = "Yazı başarıyla silindi.";
        return RedirectToAction(nameof(Index));
    }

    [Authorize]
    public async Task<IActionResult> Publish(int id)
    {
        var post = _postRepository.GetById(id);
        if (post == null)
        {
            return NotFound();
        }

        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        if (post.UserId != userId && !User.IsInRole("Admin"))
        {
            return Forbid();
        }

        post.Status = PostStatus.Published;
        post.PublishedOn = DateTime.UtcNow;
        _postRepository.EditPost(post);
        await _postRepository.SaveChangesAsync();

        TempData["success"] = "Yazı başarıyla yayınlandı.";
        return RedirectToAction(nameof(Details), new { url = post.Url });
    }

    [Authorize]
    public async Task<IActionResult> Archive(int id)
    {
        var post = _postRepository.GetById(id);
        if (post == null)
        {
            return NotFound();
        }

        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        if (post.UserId != userId && !User.IsInRole("Admin"))
        {
            return Forbid();
        }

        post.Status = PostStatus.Archived;
        _postRepository.EditPost(post);
        await _postRepository.SaveChangesAsync();

        TempData["success"] = "Yazı başarıyla arşivlendi.";
        return RedirectToAction(nameof(Details), new { url = post.Url });
    }

    [Authorize]
    public async Task<IActionResult> BulkDelete(int[] ids)
    {
        foreach (var id in ids)
        {
            var post = _postRepository.GetById(id);
            if (post != null)
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                if (post.UserId == userId || User.IsInRole("Admin"))
                {
                    _postRepository.DeletePost(id);
                }
            }
        }

        await _postRepository.SaveChangesAsync();
        TempData["success"] = "Seçili yazılar başarıyla silindi.";
        return RedirectToAction(nameof(List));
    }

    [Authorize]
    public async Task<IActionResult> BulkPublish(int[] ids)
    {
        foreach (var id in ids)
        {
            var post = _postRepository.GetById(id);
            if (post != null)
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                if (post.UserId == userId || User.IsInRole("Admin"))
                {
                    post.Status = PostStatus.Published;
                    post.PublishedOn = DateTime.UtcNow;
                    _postRepository.EditPost(post);
                }
            }
        }

        await _postRepository.SaveChangesAsync();
        TempData["success"] = "Seçili yazılar başarıyla yayınlandı.";
        return RedirectToAction(nameof(List));
    }

    [Authorize]
    public async Task<IActionResult> BulkArchive(int[] ids)
    {
        foreach (var id in ids)
        {
            var post = _postRepository.GetById(id);
            if (post != null)
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                if (post.UserId == userId || User.IsInRole("Admin"))
                {
                    post.Status = PostStatus.Archived;
                    _postRepository.EditPost(post);
                }
            }
        }

        await _postRepository.SaveChangesAsync();
        TempData["success"] = "Seçili yazılar başarıyla arşivlendi.";
        return RedirectToAction(nameof(List));
    }

    [HttpPost]
    public async Task<IActionResult> AddComment(int postId, string content)
    {
        if (string.IsNullOrEmpty(content))
        {
            return BadRequest("Yorum boş olamaz.");
        }

        var post = _postRepository.GetById(postId);
        if (post == null)
        {
            return NotFound();
        }

        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        var user = _userRepository.GetById(userId);
        if (user == null)
        {
            return NotFound();
        }

        var comment = new Comment
        {
            Content = content,
            CreatedAt = DateTime.UtcNow,
            IsActive = true,
            PostId = postId,
            UserId = userId
        };

        _commentRepository.CreateComment(comment);
        await _commentRepository.SaveChangesAsync();

        TempData["success"] = "Yorumunuz başarıyla eklendi.";
        return RedirectToAction(nameof(Details), new { url = post.Url });
    }

    public IActionResult Search(string query, int pageNumber = 1, int pageSize = 10)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return RedirectToAction(nameof(Index));
        }

        var posts = _postRepository.Posts
            .Where(p => p.IsActive && (
                p.Title.Contains(query) ||
                p.Content.Contains(query) ||
                p.Description.Contains(query)
            ))
            .OrderByDescending(p => p.PublishedOn)
            .Include(p => p.User)
            .Include(p => p.Tags)
            .Include(p => p.Comments);

        var totalPosts = posts.Count();
        var paginatedPosts = posts
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        ViewBag.SearchQuery = query;
        ViewBag.TotalPosts = totalPosts;
        ViewBag.CurrentPage = pageNumber;
        ViewBag.TotalPages = (int)Math.Ceiling(totalPosts / (double)pageSize);

        return View("Index", paginatedPosts);
    }

    [Authorize]
    public IActionResult List(int pageNumber = 1, int pageSize = 10)
    {
        var posts = _postRepository.Posts
            .Where(p => p.IsActive)
            .OrderByDescending(p => p.PublishedOn)
            .Include(p => p.User)
            .Include(p => p.Tags)
            .Include(p => p.Comments);

        var totalPosts = posts.Count();
        var paginatedPosts = posts
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        ViewBag.TotalPosts = totalPosts;
        ViewBag.CurrentPage = pageNumber;
        ViewBag.TotalPages = (int)Math.Ceiling(totalPosts / (double)pageSize);

        return View("Index", paginatedPosts);
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> React(int postId, bool isLike)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        var existingReaction = await _postRepository.GetReaction(postId, userId);

        if (existingReaction != null)
        {
            if (existingReaction.IsLike == isLike)
            {
                // Remove reaction if clicking the same button
                await _postRepository.RemoveReaction(existingReaction);
            }
            else
            {
                // Change reaction type
                existingReaction.IsLike = isLike;
                _postRepository.EditPost(existingReaction.Post);
            }
        }
        else
        {
            // Add new reaction
            var reaction = new PostReaction
            {
                PostId = postId,
                UserId = userId,
                IsLike = isLike,
                CreatedAt = DateTime.UtcNow
            };
            await _postRepository.AddReaction(reaction);
        }

        await _postRepository.SaveChangesAsync();

        var post = _postRepository.GetById(postId);
        if (post == null)
        {
            return NotFound();
        }

        return Json(new
        {
            likes = post.Reactions.Count(r => r.IsLike),
            dislikes = post.Reactions.Count(r => !r.IsLike),
            userReaction = (await _postRepository.GetReaction(postId, userId))?.IsLike
        });
    }

    private string GenerateUrl(string title)
    {
        if (string.IsNullOrEmpty(title))
        {
            return Guid.NewGuid().ToString();
        }

        var url = title.ToLower()
            .Replace("ı", "i")
            .Replace("ğ", "g")
            .Replace("ü", "u")
            .Replace("ş", "s")
            .Replace("ö", "o")
            .Replace("ç", "c")
            .Replace(" ", "-")
            .Replace(".", "")
            .Replace(",", "")
            .Replace(":", "")
            .Replace(";", "")
            .Replace("!", "")
            .Replace("?", "")
            .Replace("'", "")
            .Replace("\"", "")
            .Replace("(", "")
            .Replace(")", "")
            .Replace("[", "")
            .Replace("]", "")
            .Replace("{", "")
            .Replace("}", "")
            .Replace("/", "")
            .Replace("\\", "")
            .Replace("|", "")
            .Replace("+", "")
            .Replace("=", "")
            .Replace("*", "")
            .Replace("&", "")
            .Replace("%", "")
            .Replace("#", "")
            .Replace("@", "")
            .Replace("$", "")
            .Replace("^", "")
            .Replace("~", "")
            .Replace("`", "")
            .Replace("<", "")
            .Replace(">", "");

        return url;
    }
} 