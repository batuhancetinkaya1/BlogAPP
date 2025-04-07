using BlogApp.Entity;
using BlogApp.Models.ViewModels;
using BlogApp.Data.Abstract;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using System.IO;
using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Diagnostics;
using System.Diagnostics;

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

    public async Task<IActionResult> Index(string tag = null, string sort = "date", int page = 1)
    {
        var posts = await _postRepository.GetAllWithDetailsAsync();
        // Sadece yayında olan yazıları göster
        posts = posts.Where(p => p.Status == PostStatus.Published && p.PublishedOn != null).ToList();

        if (!string.IsNullOrEmpty(tag))
        {
            var tagObj = await _tagRepository.GetByUrlAsync(tag);
            if (tagObj != null)
            {
                posts = posts.Where(p => p.Tags.Any(t => t.TagId == tagObj.TagId)).ToList();
            }
        }

        // Apply sorting
        posts = sort switch
        {
            "title" => posts.OrderBy((Post p) => p.Title).ToList(),
            "likes" => posts.OrderByDescending((Post p) => p.Reactions.Count(r => r.IsLike)).ToList(),
            "comments" => posts.OrderByDescending((Post p) => p.Comments.Count).ToList(),
            _ => posts.OrderByDescending((Post p) => p.CreatedAt).ToList()
        };

        int pageSize = 6;
        var pagedPosts = posts.Skip((page - 1) * pageSize)
                          .Take(pageSize)
                          .ToList();

        var totalPosts = posts.Count;
        var totalPages = (int)Math.Ceiling(totalPosts / (double)pageSize);

        var model = new PostListViewModel
        {
            Posts = pagedPosts,
            CurrentPage = page,
            TotalPages = totalPages,
            CurrentSort = sort,
            CurrentTag = tag
        };

        return View(model);
    }

    [Route("posts/{url}")]
    public async Task<IActionResult> Details(string url)
    {
        // Since we don't have GetByUrlAsync in the interface, we'll need to retrieve all posts
        // and filter by URL
        var posts = await _postRepository.GetAllAsync();
        var post = posts.FirstOrDefault(p => p.Url == url);
        
        if (post == null)
        {
            return NotFound();
        }

        // Eğer resim yoksa varsayılan resmi kullan
        if (string.IsNullOrEmpty(post.Image))
        {
            post.Image = "/img/posts/default.jpg";
        }

        return View(post);
    }

    [Authorize]
    [HttpGet]
    [Route("Posts/Create")]
    public async Task<IActionResult> Create()
    {
        ViewBag.Tags = await _tagRepository.GetAllAsync();
        return View(new Models.PostCreateViewModel());
    }

    [Authorize]
    [HttpPost]
    [Route("Posts/Create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Models.PostCreateViewModel model, bool directPublish = false)
    {
        Console.WriteLine($"Create Post metodu çağrıldı. directPublish: {directPublish}");
        
        if (!ModelState.IsValid)
        {
            ViewBag.Tags = await _tagRepository.GetAllAsync();
            return View(model);
        }

        try
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var user = await _userRepository.GetByIdAsync(userId);

            if (user == null)
            {
                TempData["error"] = "Kullanıcı bulunamadı. Lütfen tekrar giriş yapın.";
                return RedirectToAction("Login", "Users");
            }

            // Eğer directPublish true ise IsDraft false olarak ayarla
            if (directPublish)
            {
                model.IsDraft = false;
            }

            var post = new Post
            {
                Title = model.Title,
                Content = model.Content,
                Description = model.Description ?? "",
                VideoUrl = model.VideoUrl,
                Keywords = model.Keywords,
                ReadTime = model.ReadTime,
                UserId = userId,
                User = user,
                CreatedAt = DateTime.UtcNow,
                Status = directPublish ? PostStatus.Published : (model.IsDraft ? PostStatus.Draft : PostStatus.Published),
                IsActive = model.IsActive
            };

            // Handle URL generation
            if (string.IsNullOrEmpty(model.Url))
            {
                post.Url = GenerateUrl(model.Title);
            }
            else
            {
                post.Url = GenerateUrl(model.Url);
            }

            // Handle scheduling
            if (!directPublish && !model.IsDraft && model.ScheduledPublishTime.HasValue)
            {
                post.Status = PostStatus.Scheduled;
                post.ScheduledPublishTime = model.ScheduledPublishTime;
            }
            else if (directPublish || (!model.IsDraft && !model.ScheduledPublishTime.HasValue))
            {
                post.Status = PostStatus.Published;
                post.PublishedOn = DateTime.UtcNow;
            }

            // Handle image upload
            if (model.ImageFile != null && model.ImageFile.Length > 0)
            {
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "img", "posts");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                var uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(model.ImageFile.FileName);
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await model.ImageFile.CopyToAsync(fileStream);
                }

                post.Image = "/img/posts/" + uniqueFileName;
            }
            else
            {
                // Resim yüklenmezse varsayılan resmi kullan
                post.Image = "/img/posts/default.jpg";
            }

            // Handle tags
            if (model.SelectedTagIds != null && model.SelectedTagIds.Count > 0)
            {
                post.Tags = new List<Tag>();
                var allTags = await _tagRepository.GetAllAsync();
                foreach (var tagId in model.SelectedTagIds)
                {
                    var tag = allTags.FirstOrDefault(t => t.TagId == tagId);
                    if (tag != null)
                    {
                        post.Tags.Add(tag);
                    }
                }
            }

            await _postRepository.AddAsync(post);
            TempData["success"] = "Blog yazısı başarıyla oluşturuldu.";
            return RedirectToAction(nameof(Details), new { url = post.Url });
        }
        catch (Exception ex)
        {
            // Log the error
            Console.WriteLine($"Blog yazısı oluşturma hatası: {ex.Message}");
            
            // Return to form with errors
            TempData["error"] = $"Blog yazısı oluşturulurken bir hata oluştu: {ex.Message}";
            ViewBag.Tags = await _tagRepository.GetAllAsync();
            return View(model);
        }
    }

    [Authorize]
    public async Task<IActionResult> Edit(int id)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        var post = await _postRepository.GetByIdAsync(id);

        if (post == null || post.UserId != userId && !User.IsInRole("Admin"))
        {
            return NotFound();
        }

        var selectedTagIds = post.Tags?.Select(t => t.TagId).ToList() ?? new List<int>();
        
        var model = new Models.PostEditViewModel
        {
            PostId = post.PostId,
            Title = post.Title,
            Content = post.Content,
            Description = post.Description,
            Image = post.Image,
            Url = post.Url,
            IsActive = post.IsActive,
            Status = post.Status,
            ScheduledPublishTime = post.ScheduledPublishTime,
            CreatedAt = post.CreatedAt,
            PublishedOn = post.PublishedOn,
            SelectedTagIds = selectedTagIds
        };
        
        ViewBag.Tags = await _tagRepository.GetAllAsync();
        return View(model);
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> Edit(int id, Models.PostEditViewModel model)
    {
        if (id != model.PostId)
        {
            return NotFound();
        }

        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        var existingPost = await _postRepository.GetByIdAsync(id);

        if (existingPost == null || (existingPost.UserId != userId && !User.IsInRole("Admin")))
        {
            return NotFound();
        }

        if (!ModelState.IsValid)
        {
            ViewBag.Tags = await _tagRepository.GetAllAsync();
            return View(model);
        }

        existingPost.Title = model.Title;
        existingPost.Content = model.Content;
        existingPost.Description = model.Description ?? "";
        existingPost.Url = GenerateUrl(model.Title);
        existingPost.IsActive = model.IsActive;
        existingPost.Status = model.Status;

        // Handle scheduling
        if (model.Status == PostStatus.Scheduled && model.ScheduledPublishTime.HasValue)
        {
            existingPost.ScheduledPublishTime = model.ScheduledPublishTime;
        }
        else if (model.Status == PostStatus.Published && existingPost.Status != PostStatus.Published)
        {
            existingPost.PublishedOn = DateTime.UtcNow;
        }

        // Handle image upload
        if (model.ImageFile != null && model.ImageFile.Length > 0)
        {
            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "img", "posts");
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            // Delete old image if exists and not default image
            if (!string.IsNullOrEmpty(existingPost.Image) && !existingPost.Image.EndsWith("default.jpg"))
            {
                var oldImagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", existingPost.Image.TrimStart('/'));
                if (System.IO.File.Exists(oldImagePath))
                {
                    try
                    {
                        System.IO.File.Delete(oldImagePath);
                    }
                    catch (Exception ex)
                    {
                        // Log error but continue
                        Console.WriteLine($"Eski resim silinirken hata oluştu: {ex.Message}");
                    }
                }
            }

            var uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(model.ImageFile.FileName);
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await model.ImageFile.CopyToAsync(fileStream);
            }

            existingPost.Image = "/img/posts/" + uniqueFileName;
        }
        else if (string.IsNullOrEmpty(existingPost.Image))
        {
            // Eğer mevcut resim yoksa ve yeni resim yüklenmediyse, varsayılan resmi kullan
            existingPost.Image = "/img/posts/default.jpg";
        }

        // Update tags
        if (model.SelectedTagIds != null && model.SelectedTagIds.Count > 0)
        {
            existingPost.Tags = new List<Tag>();
            var allTags = await _tagRepository.GetAllAsync();
            foreach (var tagId in model.SelectedTagIds)
            {
                var tag = allTags.FirstOrDefault(t => t.TagId == tagId);
                if (tag != null)
                {
                    existingPost.Tags.Add(tag);
                }
            }
        }
        else
        {
            existingPost.Tags = new List<Tag>();
        }

        await _postRepository.UpdateAsync(existingPost);
        TempData["success"] = "Yazı başarıyla güncellendi.";
        return RedirectToAction(nameof(Details), new { url = existingPost.Url });
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> Delete(int id)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        var post = await _postRepository.GetByIdAsync(id);

        if (post == null || post.UserId != userId)
        {
            return Json(new { success = false, message = "Yazı bulunamadı veya silme yetkiniz yok." });
        }

        await _postRepository.DeleteAsync(post);
        return Json(new { success = true });
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Publish(int id)
    {
        var post = await _postRepository.GetByIdAsync(id);
        if (post == null)
        {
            return NotFound();
        }

        // Kullanıcının yetkili olup olmadığını kontrol et
        if (!User.IsInRole("Admin") && post.UserId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value))
        {
            return Forbid();
        }

        post.Status = PostStatus.Published;
        post.PublishedOn = DateTime.UtcNow;
        await _postRepository.UpdateAsync(post);

        TempData["success"] = "Yazı başarıyla yayınlandı.";
        return RedirectToAction("Details", "Posts", new { url = post.Url });
    }

    [Authorize]
    public async Task<IActionResult> Archive(int id)
    {
        var post = await _postRepository.GetByIdAsync(id);
        if (post == null)
        {
            return NotFound();
        }

        // Check permissions
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        if (post.UserId != userId && !User.IsInRole("Admin"))
        {
            return Forbid();
        }

        await _postRepository.UpdateAsync(post);

        return RedirectToAction(nameof(Details), new { url = post.Url });
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> BulkDelete(int[] ids)
    {
        if (ids == null || ids.Length == 0)
        {
            return BadRequest("No posts selected.");
        }

        foreach (var postId in ids)
        {
            var post = await _postRepository.GetByIdAsync(postId);
            if (post != null)
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                if (post.UserId == userId || User.IsInRole("Admin"))
                {
                    await _postRepository.DeleteAsync(post);
                }
            }
        }

        return Json(new { success = true });
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> BulkPublish(int[] ids)
    {
        if (ids == null || ids.Length == 0)
        {
            return BadRequest("No posts selected.");
        }

        foreach (var postId in ids)
        {
            var post = await _postRepository.GetByIdAsync(postId);
            if (post != null)
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                if (post.UserId == userId || User.IsInRole("Admin"))
                {
                    post.CreatedAt = DateTime.UtcNow;
                    await _postRepository.UpdateAsync(post);
                }
            }
        }

        return Json(new { success = true });
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> BulkArchive(int[] ids)
    {
        if (ids == null || ids.Length == 0)
        {
            return BadRequest("No posts selected.");
        }

        foreach (var postId in ids)
        {
            var post = await _postRepository.GetByIdAsync(postId);
            if (post != null)
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                if (post.UserId == userId || User.IsInRole("Admin"))
                {
                    await _postRepository.UpdateAsync(post);
                }
            }
        }

        return Json(new { success = true });
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddComment([FromBody] CommentViewModel model)
    {
        if (string.IsNullOrWhiteSpace(model.Content))
        {
            return Json(new { success = false, message = "Yorum içeriği boş olamaz." });
        }

        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        var post = await _postRepository.GetByIdAsync(model.PostId);

        if (post == null)
        {
            return Json(new { success = false, message = "Yazı bulunamadı." });
        }

        var comment = new Comment
        {
            Content = model.Content,
            CreatedAt = DateTime.UtcNow,
            PublishedOn = DateTime.UtcNow,
            PostId = model.PostId,
            UserId = userId,
            IsActive = true
        };

        await _commentRepository.AddAsync(comment);
        return Json(new { success = true, message = "Yorum başarıyla eklendi." });
    }

    public class CommentViewModel
    {
        public int PostId { get; set; }
        public string? Content { get; set; }
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddReaction([FromBody] ReactionViewModel model)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        var post = await _postRepository.GetByIdAsync(model.PostId);

        if (post == null)
        {
            return Json(new { success = false, message = "Yazı bulunamadı." });
        }

        var existingReaction = post.Reactions.FirstOrDefault(r => r.UserId == userId);
        if (existingReaction != null)
        {
            if (existingReaction.IsLike == model.IsLike)
            {
                post.Reactions.Remove(existingReaction);
                await _postRepository.UpdateAsync(post);
                return Json(new { success = true, message = "Tepki kaldırıldı." });
            }
            else
            {
                existingReaction.IsLike = model.IsLike;
            }
        }
        else
        {
            post.Reactions.Add(new PostReaction
            {
                PostId = model.PostId,
                UserId = userId,
                IsLike = model.IsLike,
                CreatedAt = DateTime.UtcNow
            });
        }

        await _postRepository.UpdateAsync(post);
        return Json(new { success = true, message = model.IsLike ? "Yazı beğenildi." : "Yazı beğenilmedi." });
    }

    public class ReactionViewModel
    {
        public int PostId { get; set; }
        public bool IsLike { get; set; }
    }

    [HttpGet]
    [Route("Posts/Search")]
    public async Task<IActionResult> Search(string query, int pageNumber = 1, int pageSize = 10)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                // Boş sorgu durumunda Index action'a yönlendirmek yerine tüm postları gösteriyoruz
                var allPosts = await _postRepository.GetAllAsync();
                var activePosts = allPosts
                    .Where(p => p.IsActive && p.Status == PostStatus.Published)
                    .OrderByDescending(p => p.PublishedOn)
                    .ToList();
                    
                var totalAllPosts = activePosts.Count;
                var paginatedAllPosts = activePosts
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();
                    
                ViewBag.SearchQuery = "";
                ViewBag.TotalPosts = totalAllPosts;
                ViewBag.CurrentPage = pageNumber;
                ViewBag.TotalPages = (int)Math.Ceiling(totalAllPosts / (double)pageSize);
                ViewBag.ShowAllPostsMessage = true;
                
                return View(paginatedAllPosts);
            }

            // Trim search query
            query = query.Trim();
            
            // Türkçe karakterleri normalize et
            var normalizedQuery = query
                .Replace('ı', 'i')
                .Replace('ğ', 'g')
                .Replace('ü', 'u')
                .Replace('ş', 's')
                .Replace('ö', 'o')
                .Replace('ç', 'c')
                .Replace('İ', 'i');

            var posts = await _postRepository.SearchAsync(query);
            
            // Normalize edilmiş sorgu ile de arama yap ve sonuçları birleştir
            if (normalizedQuery != query)
            {
                var normalizedResults = await _postRepository.SearchAsync(normalizedQuery);
                // Sonuçları birleştir ve tekrarları kaldır
                posts = posts.Union(normalizedResults).ToList();
            }
            
            var totalSearchPosts = posts.Count;
            var paginatedSearchPosts = posts
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            ViewBag.SearchQuery = query;
            ViewBag.TotalPosts = totalSearchPosts;
            ViewBag.CurrentPage = pageNumber;
            ViewBag.TotalPages = (int)Math.Ceiling(totalSearchPosts / (double)pageSize);
            
            // Suggest related searches if no results
            if (totalSearchPosts == 0)
            {
                var allTags = await _tagRepository.GetAllAsync();
                ViewBag.SuggestedTags = allTags
                    .Where(t => t.Name.ToLower().Contains(query.ToLower()) || 
                               t.Name.ToLower().Contains(normalizedQuery.ToLower()))
                    .Take(5)
                    .ToList();
                    
                // Burada boş da olsa bir liste döndürüyoruz ki view doğru çalışsın
                return View(new List<Post>());
            }

            return View(paginatedSearchPosts);
        }
        catch (Exception ex)
        {
            // Hata durumunda log tutabilir ve kullanıcıya hata sayfası gösterebiliriz
            return View("Error", new ErrorViewModel 
            { 
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier,
                ErrorMessage = "Arama sırasında bir hata oluştu: " + ex.Message 
            });
        }
    }

    [Authorize]
    public async Task<IActionResult> List(int pageNumber = 1, int pageSize = 10)
    {
        var allPosts = await _postRepository.GetAllAsync();
        var posts = allPosts
            .Where(p => p.IsActive)
            .OrderByDescending(p => p.PublishedOn)
            .ToList();

        var totalPosts = posts.Count();
        var paginatedPosts = posts
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        ViewBag.TotalPosts = totalPosts;
        ViewBag.CurrentPage = pageNumber;
        ViewBag.TotalPages = (int)Math.Ceiling(totalPosts / (double)pageSize);

        return View(paginatedPosts);
    }

    [Authorize]
    public async Task<IActionResult> GetPostsJson()
    {
        var posts = await _postRepository.GetAllAsync();
        var postList = posts.Select(p => new
        {
            id = p.PostId,
            title = p.Title,
            url = p.Url,
            date = p.CreatedAt.ToString("dd/MM/yyyy"),
            author = p.User.UserName
        });
        return Json(postList);
    }

    private string GenerateUrl(string title)
    {
        if (string.IsNullOrEmpty(title))
            return "";

        // Remove diacritics (accents)
        var normalizedString = title.Normalize(System.Text.NormalizationForm.FormD);
        var stringBuilder = new System.Text.StringBuilder();

        foreach (var c in normalizedString)
        {
            var unicodeCategory = System.Globalization.CharUnicodeInfo.GetUnicodeCategory(c);
            if (unicodeCategory != System.Globalization.UnicodeCategory.NonSpacingMark)
            {
                stringBuilder.Append(c);
            }
        }

        // Convert to lowercase and replace spaces with dashes
        var result = stringBuilder.ToString().Normalize(System.Text.NormalizationForm.FormC)
            .ToLowerInvariant()
            .Replace(" ", "-")
            .Replace("ı", "i")
            .Replace("ğ", "g")
            .Replace("ü", "u")
            .Replace("ş", "s")
            .Replace("ö", "o")
            .Replace("ç", "c")
            .Replace("İ", "i");

        // Remove special characters
        result = System.Text.RegularExpressions.Regex.Replace(result, @"[^a-z0-9\-]", "");

        // Replace multiple dashes with a single dash
        result = System.Text.RegularExpressions.Regex.Replace(result, @"-{2,}", "-");

        // Trim dashes from the beginning and end
        result = result.Trim('-');

        return result;
    }
} 