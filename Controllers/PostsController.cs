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
using BlogApp.Helpers;
using BlogApp.Services;

namespace BlogApp.Controllers;

public class PostsController : Controller
{
    private readonly IPostRepository _postRepository;
    private readonly ITagRepository _tagRepository;
    private readonly ICommentRepository _commentRepository;
    private readonly IUserRepository _userRepository;
    private readonly INotificationService _notificationService;

    public PostsController(
        IPostRepository postRepository,
        ITagRepository tagRepository,
        ICommentRepository commentRepository,
        IUserRepository userRepository,
        INotificationService notificationService)
    {
        _postRepository = postRepository;
        _tagRepository = tagRepository;
        _commentRepository = commentRepository;
        _userRepository = userRepository;
        _notificationService = notificationService;
    }

    public async Task<IActionResult> Index(string? tag = null, string sort = "date", int page = 1)
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
        try
        {
            Console.WriteLine($"Details: Post çağrılıyor URL: {url}");
            var post = await _postRepository.GetByUrlWithCommentsAndReactions(url);
            
            if (post == null)
            {
                return NotFound();
            }

            // Ensure User information is loaded
            if (post.User == null)
            {
                post.User = await _userRepository.GetByIdAsync(post.UserId);
            }

            // Ensure User information is loaded for comments
            foreach (var comment in post.Comments)
            {
                if (comment.User == null)
                {
                    comment.User = await _userRepository.GetByIdAsync(comment.UserId);
                }
            }

            return View(post);
        }
        catch (Exception)
        {
            // Log the error
            return StatusCode(500, "Bir hata oluştu.");
        }
    }

    [Authorize]
    [HttpGet]
    [Route("Posts/Create")]
    public async Task<IActionResult> Create()
    {
        try
        {
            Console.WriteLine("Create GET action başladı");
            var tags = await _tagRepository.GetAllAsync();
            Console.WriteLine($"Toplam {tags.Count()} etiket yüklendi");
            ViewBag.Tags = tags;
            return View(new Models.PostCreateViewModel());
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Create GET action hatası: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
            // Hata durumunda ModelState'e bilgileri ekle
            ModelState.AddModelError("", "Sayfa yüklenirken bir hata oluştu: " + ex.Message);
            
            // ViewBag.Tags boş olursa hata verebilir, boş liste gönderelim
            ViewBag.Tags = new List<Entity.Tag>();
            return View(new Models.PostCreateViewModel());
        }
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    [Route("Posts/Create")]
    public async Task<IActionResult> Create(Models.PostCreateViewModel model, string action)
    {
        try
        {
            Console.WriteLine($"Create POST action başladı. model: {model?.Title}, action: {action}");
            
            // Form submit butonundan gelen değeri kontrol et
            if (string.IsNullOrEmpty(action))
            {
                // Form'da action değeri belirtilmemişse, default olarak taslak kabul et
                action = "draft";
                Console.WriteLine("Action değeri boş, draft olarak varsayıldı");
            }
            
            // Modeli log'a yazdır (hata ayıklama için)
            foreach (var prop in typeof(Models.PostCreateViewModel).GetProperties())
            {
                Console.WriteLine($"Property {prop.Name}: {prop.GetValue(model)}");
            }
            
            // ViewBag.Tags doldur
            ViewBag.Tags = await _tagRepository.GetAllAsync();
            
            if (!ModelState.IsValid)
            {
                Console.WriteLine("Model doğrulama hatası:");
                foreach (var key in ModelState.Keys)
                {
                    var state = ModelState[key];
                    if (state.Errors.Count > 0)
                    {
                        Console.WriteLine($"Key: {key}");
                        foreach (var error in state.Errors)
                        {
                            Console.WriteLine($" - Error: {error.ErrorMessage}");
                        }
                    }
                }
                
                return View(model);
            }

            // Sanitize HTML content to prevent XSS
            model.Content = Helpers.HtmlSanitizerHelper.Sanitize(model.Content);
            
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim?.Value == null)
            {
                return Unauthorized();
            }
            
            var post = new Post
            {
                Title = model.Title ?? string.Empty,
                Content = model.Content ?? string.Empty,
                Description = model.Description ?? string.Empty,
                Url = !string.IsNullOrEmpty(model.Url) ? model.Url : GenerateUrl(model.Title ?? string.Empty),
                CreatedAt = DateTime.UtcNow,
                UserId = int.Parse(userIdClaim.Value),
                IsActive = model.IsActive,
                VideoUrl = model.VideoUrl,
                Keywords = model.Keywords,
                ReadTime = model.ReadTime,
                Status = model.IsDraft || action == "draft" ? PostStatus.Draft : PostStatus.Published
            };
            
            // Yayınlama durumunu belirle
            if (action == "publish" || (!model.IsDraft && action != "draft"))
            {
                post.Status = PostStatus.Published;
                post.PublishedOn = DateTime.UtcNow;
                Console.WriteLine("Post durumu Published olarak ayarlandı");
            }
            else
            {
                post.Status = PostStatus.Draft;
                Console.WriteLine("Post durumu Draft olarak ayarlandı");
            }

            // Dosya yükleme işlemi
            if (model.ImageFile != null)
            {
                var extension = Path.GetExtension(model.ImageFile.FileName).ToLower();
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
                
                if (!allowedExtensions.Contains(extension))
                {
                    ModelState.AddModelError("ImageFile", "Sadece .jpg, .jpeg, .png ve .webp uzantılı dosyalar yüklenebilir.");
                    return View(model);
                }
                
                var fileName = $"{Guid.NewGuid()}{extension}";
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "img", "posts", fileName);
                
                // Dizin kontrolü
                var imageDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "img", "posts");
                if (!Directory.Exists(imageDir))
                {
                    Directory.CreateDirectory(imageDir);
                }

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await model.ImageFile.CopyToAsync(fileStream);
                }

                post.Image = $"/img/posts/{fileName}";
            }
            else
            {
                // Varsayılan bir resim ata
                post.Image = "/img/posts/default.jpg";
            }

            // Post'u kaydet
            await _postRepository.AddAsync(post);
            
            // Etiketleri ekle
            if (model.SelectedTagIds != null && model.SelectedTagIds.Count > 0)
            {
                foreach (var tagId in model.SelectedTagIds)
                {
                    var tag = await _tagRepository.GetByIdAsync(tagId);
                    if (tag != null)
                    {
                        post.Tags.Add(tag);
                    }
                }
                await _postRepository.UpdateAsync(post);
            }
            
            TempData["success"] = action == "publish" 
                ? "Yazınız başarıyla yayınlandı." 
                : "Yazınız taslak olarak kaydedildi.";
            
            return RedirectToAction("Details", new { url = post.Url });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Post oluşturma sırasında hata: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
            ModelState.AddModelError("", $"Bir hata oluştu: {ex.Message}");
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
    [ValidateAntiForgeryToken]
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

        // Sanitize HTML content to prevent XSS
        model.Content = Helpers.HtmlSanitizerHelper.Sanitize(model.Content);

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
    [HttpGet]
    [Route("Posts/Delete/{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        var post = await _postRepository.GetByIdAsync(id);

        if (post == null)
        {
            return NotFound();
        }

        if (post.UserId != userId && !User.IsInRole("Admin"))
        {
            return Forbid();
        }

        return View(post);
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Route("Posts/Delete/{id:int}")]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        var post = await _postRepository.GetByIdAsync(id);

        if (post == null)
        {
            return Request.Headers["X-Requested-With"] == "XMLHttpRequest" 
                ? Json(new { success = false, message = "Yazı bulunamadı." }) 
                : NotFound();
        }

        if (post.UserId != userId && !User.IsInRole("Admin"))
        {
            return Request.Headers["X-Requested-With"] == "XMLHttpRequest" 
                ? Json(new { success = false, message = "Bu yazıyı silme yetkiniz yok." }) 
                : Forbid();
        }

        await _postRepository.DeleteAsync(post);
        
        // Check if request is AJAX
        if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
        {
            return Json(new { success = true, message = "Yazı başarıyla silindi." });
        }
        
        // For direct form submissions, redirect to user's posts page
        _notificationService.Success("Yazı başarıyla silindi.");
        return RedirectToAction("MyPosts", "Users");
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
        string? userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!User.IsInRole("Admin") && userIdStr != null && post.UserId != int.Parse(userIdStr))
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
    [HttpPost]
    [ValidateAntiForgeryToken]
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

        post.Status = PostStatus.Archived;
        await _postRepository.UpdateAsync(post);

        TempData["success"] = "Yazı başarıyla arşivlendi.";
        return RedirectToAction(nameof(Details), new { url = post.Url });
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
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
    [ValidateAntiForgeryToken]
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
    [ValidateAntiForgeryToken]
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
    [Route("/Posts/AddComment")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddComment([FromBody] CommentViewModel model)
    {
        try
        {
            if (model == null || model.PostId <= 0 || string.IsNullOrWhiteSpace(model.Content))
            {
                return Json(new { success = false, message = "Geçersiz istek veya yorum içeriği boş." });
        }

        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            Console.WriteLine($"AddComment: Kullanıcı ID: {userId}, Post ID: {model.PostId}, ParentID: {model.ParentCommentId}");
            
            var user = await _userRepository.GetByIdAsync(userId);
            
            if (user == null)
            {
                Console.WriteLine("AddComment: Kullanıcı bulunamadı");
                return Json(new { success = false, message = "Kullanıcı bulunamadı." });
            }
            
            var post = await _postRepository.GetByIdAsync(model.PostId);
        if (post == null)
        {
                Console.WriteLine("AddComment: Post bulunamadı");
            return Json(new { success = false, message = "Yazı bulunamadı." });
        }

            // Eğer parentCommentId varsa, bu yorumun var olduğunu kontrol et
            if (model.ParentCommentId.HasValue)
            {
                var parentComment = await _commentRepository.GetByIdAsync(model.ParentCommentId.Value);
                if (parentComment == null)
                {
                    Console.WriteLine($"AddComment: Ebeveyn yorum bulunamadı - ID: {model.ParentCommentId.Value}");
                    return Json(new { success = false, message = "Yanıt verilen yorum bulunamadı." });
                }
                
                Console.WriteLine($"AddComment: Ebeveyn yorum bulundu - ID: {parentComment.CommentId}");
        }

        var comment = new Comment
        {
                Content = model.Content.Trim(),
            CreatedAt = DateTime.UtcNow,
            PublishedOn = DateTime.UtcNow,
            PostId = model.PostId,
            UserId = userId,
                IsActive = true,
                ParentCommentId = model.ParentCommentId
        };

            Console.WriteLine("AddComment: Yorum kaydediliyor");
        await _commentRepository.AddAsync(comment);
            
            Console.WriteLine($"AddComment: Yorum başarıyla kaydedildi - ID: {comment.CommentId}");
            
            return Json(new { 
                success = true, 
                message = "Yorum başarıyla eklendi.",
                commentId = comment.CommentId,
                userName = user.UserName,
                userImage = user.Image,
                content = comment.Content,
                formattedDate = comment.CreatedAt.ToString("dd MMM yyyy HH:mm"),
                isParentComment = !comment.ParentCommentId.HasValue,
                parentCommentId = comment.ParentCommentId
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"AddComment error: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
            return Json(new { success = false, message = "Bir hata oluştu. Lütfen daha sonra tekrar deneyin." });
        }
    }

    [Authorize]
    [HttpPost]
    [Route("/Posts/AddCommentReaction")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddCommentReaction([FromBody] CommentReactionViewModel model)
    {
        try
        {
            if (model == null || model.CommentId <= 0)
            {
                return Json(new { success = false, message = "Geçersiz istek." });
            }

            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            Console.WriteLine($"AddCommentReaction: Kullanıcı ID: {userId}, Yorum ID: {model.CommentId}, IsLike: {model.IsLike}");
            
            var comment = await _commentRepository.GetByIdAsync(model.CommentId);

            if (comment == null)
            {
                Console.WriteLine("AddCommentReaction: Yorum bulunamadı");
                return Json(new { success = false, message = "Yorum bulunamadı." });
            }

            string action = "";
            var existingReaction = comment.Reactions.FirstOrDefault(r => r.UserId == userId);
            Console.WriteLine($"AddCommentReaction: Mevcut reaksiyon: {(existingReaction != null ? $"ID: {existingReaction.CommentReactionId}, IsLike: {existingReaction.IsLike}" : "Yok")}");
            
            if (existingReaction != null)
            {
                // Zaten bir reaksiyon var
                if (existingReaction.IsLike == model.IsLike)
                {
                    // Aynı tip reaksiyon - kaldır
                    Console.WriteLine("AddCommentReaction: Aynı tip reaksiyon - kaldırılıyor");
                    comment.Reactions.Remove(existingReaction);
                    action = "removed";
                }
                else
                {
                    // Farklı tip reaksiyon - güncelle
                    Console.WriteLine("AddCommentReaction: Farklı tip reaksiyon - güncelleniyor");
                    existingReaction.IsLike = model.IsLike;
                    action = "updated";
                }
            }
            else
            {
                // Yeni reaksiyon ekle
                Console.WriteLine("AddCommentReaction: Yeni reaksiyon ekleniyor");
                comment.Reactions.Add(new CommentReaction
                {
                    CommentId = model.CommentId,
                    UserId = userId,
                    IsLike = model.IsLike,
                    CreatedAt = DateTime.UtcNow
                });
                action = "added";
            }

            try
            {
                // Değişiklikleri kaydet
                Console.WriteLine("AddCommentReaction: Değişiklikler kaydediliyor");
                await _commentRepository.UpdateAsync(comment);
                
                // Değişikliklerden sonra yorumu tekrar çek ve sayıları güncelle
                comment = await _commentRepository.GetByIdAsync(model.CommentId);
                
                // Gerçek sayıları hesapla
                int likesCount = comment.Reactions.Count(r => r.IsLike);
                int dislikesCount = comment.Reactions.Count(r => !r.IsLike);
                
                Console.WriteLine($"AddCommentReaction: İşlem tamamlandı - Beğeni: {likesCount}, Beğenmeme: {dislikesCount}");

                return Json(new { 
                    success = true, 
                    message = model.IsLike ? "Yorum beğenildi." : "Yorum beğenilmedi.", 
                    action,
                    likesCount,
                    dislikesCount
                });
            }
            catch (DbUpdateException ex)
            {
                // Unique constraint hatası
                if (ex.InnerException?.Message.Contains("UNIQUE constraint failed") == true)
                {
                    Console.WriteLine("AddCommentReaction: Unique constraint hatası - reaksiyon zaten var");
                    
                    // Veritabanından güncel veriyi al
                    comment = await _commentRepository.GetByIdAsync(model.CommentId);
                    
                    // Mevcut reaksiyonu bul
                    existingReaction = comment.Reactions.FirstOrDefault(r => r.UserId == userId);
                    
                    if (existingReaction != null)
                    {
                        // Reaksiyonu güncelle
                        existingReaction.IsLike = model.IsLike;
                        await _commentRepository.UpdateAsync(comment);
                        
                        // Sayıları güncelle
                        int likesCount = comment.Reactions.Count(r => r.IsLike);
                        int dislikesCount = comment.Reactions.Count(r => !r.IsLike);
                        
                        return Json(new { 
                            success = true, 
                            message = model.IsLike ? "Yorum beğenildi." : "Yorum beğenilmedi.", 
                            action = "updated",
                            likesCount,
                            dislikesCount
                        });
                    }
                }
                
                Console.WriteLine($"AddCommentReaction: Değişiklikler kaydedilirken hata oluştu: {ex.Message}");
                return Json(new { success = false, message = "Reaksiyon eklenirken bir hata oluştu." });
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"AddCommentReaction error: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
            return Json(new { success = false, message = "Bir hata oluştu. Lütfen daha sonra tekrar deneyin." });
        }
    }

    [Authorize]
    [HttpPost]
    [Route("/Posts/AddReaction")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddReaction([FromBody] ReactionViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");
        if (userId == 0)
        {
            return Unauthorized();
        }

        var post = await _postRepository.GetByIdAsync(model.PostId);
        if (post == null)
        {
            return NotFound("Post bulunamadı.");
        }

        string action = "";
        var existingReaction = post.Reactions.FirstOrDefault(r => r.UserId == userId);
        Console.WriteLine($"AddReaction: Mevcut reaksiyon: {(existingReaction != null ? $"ID: {existingReaction.PostReactionId}, IsLike: {existingReaction.IsLike}" : "Yok")}");
        
        if (existingReaction != null)
        {
            // Zaten bir reaksiyon var
            if (existingReaction.IsLike == model.IsLike)
            {
                // Aynı tip reaksiyon - kaldır
                Console.WriteLine("AddReaction: Aynı tip reaksiyon - kaldırılıyor");
                post.Reactions.Remove(existingReaction);
                action = "removed";
            }
            else
            {
                // Farklı tip reaksiyon - güncelle
                Console.WriteLine("AddReaction: Farklı tip reaksiyon - güncelleniyor");
                existingReaction.IsLike = model.IsLike;
                action = "updated";
            }
        }
        else
        {
            // Yeni reaksiyon ekle
            Console.WriteLine("AddReaction: Yeni reaksiyon ekleniyor");
            post.Reactions.Add(new PostReaction
            {
                PostId = model.PostId,
                UserId = userId,
                IsLike = model.IsLike,
                CreatedAt = DateTime.UtcNow
            });
            action = "added";
        }

        // Değişiklikleri kaydet
        Console.WriteLine("AddReaction: Değişiklikler kaydediliyor");
        await _postRepository.UpdateAsync(post);
        
        // Değişikliklerden sonra postu tekrar çek ve sayıları güncelle
        post = await _postRepository.GetByIdAsync(model.PostId);
        
        // Gerçek sayıları hesapla
        int likesCount = post.Reactions.Count(r => r.IsLike);
        int dislikesCount = post.Reactions.Count(r => !r.IsLike);
        
        Console.WriteLine($"AddReaction: İşlem tamamlandı - Beğeni: {likesCount}, Beğenmeme: {dislikesCount}");

        return Json(new { 
            success = true, 
            message = model.IsLike ? "Yazı beğenildi." : "Yazı beğenilmedi.", 
            action,
            likesCount,
            dislikesCount
        });
    }

    [Authorize]
    [HttpPost]
    [Route("/Posts/DeleteComment")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteComment([FromBody] CommentIdViewModel model)
    {
        try
        {
            if (model == null || model.CommentId <= 0)
            {
                return Json(new { success = false, message = "Geçersiz istek." });
            }

            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            Console.WriteLine($"DeleteComment: Kullanıcı ID: {userId}, Yorum ID: {model.CommentId}");
            
            var comment = await _commentRepository.GetByIdAsync(model.CommentId);

            if (comment == null)
            {
                Console.WriteLine("DeleteComment: Yorum bulunamadı");
                return Json(new { success = false, message = "Yorum bulunamadı." });
            }

            // Yorum kullanıcıya ait mi kontrol et
            if (comment.UserId != userId && !User.IsInRole("Admin"))
            {
                Console.WriteLine("DeleteComment: Kullanıcının silme yetkisi yok");
                return Json(new { success = false, message = "Bu yorumu silme yetkiniz bulunmuyor." });
            }

            // Ebeveyn yorum ID'sini sakla
            var parentCommentId = comment.ParentCommentId;
            
            Console.WriteLine($"DeleteComment: Yorum siliniyor, ParentID: {parentCommentId}");
            
            try
            {
                // Önce yorumun tüm reaksiyonlarını sil
                if (comment.Reactions != null && comment.Reactions.Any())
                {
                    Console.WriteLine($"DeleteComment: {comment.Reactions.Count} reaksiyon siliniyor");
                    comment.Reactions.Clear();
                    await _commentRepository.UpdateAsync(comment);
                }
                
                // Eğer bu bir ana yorum ise, tüm yanıtları sil
                if (!comment.ParentCommentId.HasValue)
                {
                    var replies = await _commentRepository.GetRepliesByParentIdAsync(comment.CommentId);
                    if (replies != null && replies.Any())
                    {
                        Console.WriteLine($"DeleteComment: {replies.Count} yanıt siliniyor");
                        foreach (var reply in replies)
                        {
                            // Her yanıtın reaksiyonlarını temizle
                            if (reply.Reactions != null && reply.Reactions.Any())
                            {
                                reply.Reactions.Clear();
                                await _commentRepository.UpdateAsync(reply);
                            }
                            
                            // Yanıtı sil
                            await _commentRepository.DeleteAsync(reply);
                        }
                    }
                }
                
                // Yorumu sil
                await _commentRepository.DeleteAsync(comment);
                Console.WriteLine("DeleteComment: Yorum başarıyla silindi");
            }
            catch (DbUpdateException ex)
            {
                Console.WriteLine($"DeleteComment: Yorum silinirken hata oluştu: {ex.Message}");
                Console.WriteLine($"Inner exception: {ex.InnerException?.Message}");
                
                // Foreign key hatası için özel işlem
                if (ex.InnerException?.Message.Contains("FOREIGN KEY constraint failed") == true)
                {
                    // Tüm ilişkili kayıtları manuel olarak silmeyi dene
                    try
                    {
                        // Veritabanı bağlantısını al
                        var dbContext = _commentRepository.GetDbContext();
                        
                        // Reaksiyonları sil
                        var reactions = dbContext.Set<CommentReaction>()
                            .Where(r => r.CommentId == model.CommentId)
                            .ToList();
                        
                        if (reactions.Any())
                        {
                            dbContext.Set<CommentReaction>().RemoveRange(reactions);
                            await dbContext.SaveChangesAsync();
                        }
                        
                        // Yanıtları sil
                        var replies = dbContext.Set<Comment>()
                            .Where(c => c.ParentCommentId == model.CommentId)
                            .ToList();
                        
                        if (replies.Any())
                        {
                            foreach (var reply in replies)
                            {
                                // Yanıtların reaksiyonlarını sil
                                var replyReactions = dbContext.Set<CommentReaction>()
                                    .Where(r => r.CommentId == reply.CommentId)
                                    .ToList();
                                
                                if (replyReactions.Any())
                                {
                                    dbContext.Set<CommentReaction>().RemoveRange(replyReactions);
                                    await dbContext.SaveChangesAsync();
                                }
                                
                                // Yanıtı sil
                                dbContext.Set<Comment>().Remove(reply);
                                await dbContext.SaveChangesAsync();
                            }
                        }
                        
                        // Ana yorumu sil
                        var commentToDelete = dbContext.Set<Comment>()
                            .FirstOrDefault(c => c.CommentId == model.CommentId);
                        
                        if (commentToDelete != null)
                        {
                            dbContext.Set<Comment>().Remove(commentToDelete);
                            await dbContext.SaveChangesAsync();
                            Console.WriteLine("DeleteComment: Yorum ve ilişkili kayıtlar başarıyla silindi");
                            
                            return Json(new { 
                                success = true, 
                                message = "Yorum başarıyla silindi.",
                                parentCommentId = parentCommentId
                            });
                        }
                    }
                    catch (Exception innerEx)
                    {
                        Console.WriteLine($"DeleteComment: Manuel silme işlemi sırasında hata: {innerEx.Message}");
                    }
                }
                
                return Json(new { success = false, message = "Yorum silinirken bir hata oluştu." });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"DeleteComment: Yorum silinirken hata oluştu: {ex.Message}");
                return Json(new { success = false, message = "Yorum silinirken bir hata oluştu." });
            }

            return Json(new { 
                success = true, 
                message = "Yorum başarıyla silindi.",
                parentCommentId = parentCommentId
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"DeleteComment error: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
            return Json(new { success = false, message = "Bir hata oluştu. Lütfen daha sonra tekrar deneyin." });
        }
    }

    public class CommentViewModel
    {
        public int PostId { get; set; }
        public string? Content { get; set; }
        public int? ParentCommentId { get; set; }
    }

    public class CommentIdViewModel
    {
        public int CommentId { get; set; }
    }

    public class CommentReactionViewModel
    {
        public int CommentId { get; set; }
        public bool IsLike { get; set; }
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
        if (string.IsNullOrWhiteSpace(query))
        {
            var allPosts = await _postRepository.GetAllAsync();
            var filteredPosts = allPosts
                .Where(p => p.IsActive && p.Status == PostStatus.Published)
                .OrderByDescending(p => p.PublishedOn)
                .ToList();
                
            var paginatedList = BlogApp.Helpers.PaginatedList<Post>.Create(filteredPosts, pageNumber, pageSize);
            
            // Sidebar için eklentiler
            ViewBag.PopularTags = await _tagRepository.GetAllAsync();
            ViewBag.RecentPosts = filteredPosts.Take(5).ToList();
            
            return View(paginatedList);
        }

        query = query.Trim().ToLower();
        var posts = await _postRepository.GetAllAsync();
        var searchResults = posts
            .Where(p => 
                p.IsActive && 
                p.Status == PostStatus.Published &&
                (p.Title.ToLower().Contains(query) || 
                 p.Description.ToLower().Contains(query) || 
                 p.Content.ToLower().Contains(query) ||
                 p.Tags.Any(t => t.Name.ToLower().Contains(query))))
            .OrderByDescending(p => p.PublishedOn)
            .ToList();
            
        var paginatedResults = BlogApp.Helpers.PaginatedList<Post>.Create(searchResults, pageNumber, pageSize);
        
        // Sidebar için eklentiler
        ViewBag.PopularTags = await _tagRepository.GetAllAsync();
        ViewBag.RecentPosts = posts
            .Where(p => p.IsActive && p.Status == PostStatus.Published)
            .OrderByDescending(p => p.PublishedOn)
            .Take(5)
            .ToList();
        
        return View(paginatedResults);
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

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Route("Posts/UploadImage")]
    public async Task<IActionResult> UploadImage(IFormFile upload)
    {
        if (upload == null || upload.Length == 0)
        {
            return Json(new { uploaded = false, error = new { message = "No file uploaded" } });
        }

        try
        {
            var url = await ImageHelper.ValidateAndSaveContentImageAsync(upload);
            return Json(new { uploaded = true, url });
        }
        catch (ImageValidationException ex)
        {
            return Json(new { uploaded = false, error = new { message = ex.Message } });
        }
        catch (Exception ex)
        {
            // Log the exception
            Console.WriteLine($"Image upload error: {ex.Message}");
            return Json(new { uploaded = false, error = new { message = "An error occurred while uploading the image. Please try again." } });
        }
    }
} 