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
        try
        {
            Console.WriteLine($"Details: Post çağrılıyor URL: {url}");
            var post = await _postRepository.GetByUrlWithCommentsAndReactions(url);
        if (post == null)
        {
                Console.WriteLine("Details: Post bulunamadı!");
            return NotFound();
        }

            Console.WriteLine($"Details: Post bulundu ID: {post.PostId}, Yorumlar: {post.Comments?.Count ?? 0}, Beğeniler: {post.Reactions?.Count ?? 0}");
            
            // Kullanıcı giriş yapmışsa beğenileri kontrol et
            int? userId = null;
            if (User.Identity.IsAuthenticated)
            {
                string userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!string.IsNullOrEmpty(userIdStr) && int.TryParse(userIdStr, out int parsedUserId))
                {
                    userId = parsedUserId;
                    Console.WriteLine($"Details: Giriş yapan kullanıcı ID: {userId}");
                }
            }
            
            ViewBag.UserReaction = userId.HasValue 
                ? post.Reactions?.FirstOrDefault(r => r.UserId == userId)?.IsLike 
                : null;
                
            Console.WriteLine($"Details: Kullanıcının yazıya tepkisi: {ViewBag.UserReaction}");

            var commentViewModel = new Dictionary<int, List<object>>();
            if (post.Comments != null)
            {
                foreach (var comment in post.Comments)
                {
                    Console.WriteLine($"Details: Yorum ID: {comment.CommentId}, Tepkiler: {comment.Reactions?.Count ?? 0}, ParentId: {comment.ParentCommentId}");
                    
                    // Yoruma kullanıcı tepkisi
                    var userCommentReaction = userId.HasValue 
                        ? comment.Reactions?.FirstOrDefault(r => r.UserId == userId)?.IsLike 
                        : null;
                        
                    Console.WriteLine($"Details: Kullanıcının yoruma tepkisi (ID: {comment.CommentId}): {userCommentReaction}");
                }
        }

        return View(post);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Details: Hata oluştu: {ex.Message}");
            return StatusCode(500, "Bir hata oluştu: " + ex.Message);
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

            var post = new Post
            {
                Title = model.Title,
                Content = model.Content,
                Description = model.Description,
                Url = !string.IsNullOrEmpty(model.Url) ? model.Url : GenerateUrl(model.Title),
                CreatedAt = DateTime.UtcNow,
                UserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0"),
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
        try
        {
            if (model == null || model.PostId <= 0)
            {
                return Json(new { success = false, message = "Geçersiz istek." });
            }

        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            Console.WriteLine($"AddReaction: Kullanıcı ID: {userId}, Post ID: {model.PostId}, IsLike: {model.IsLike}");
            
        var post = await _postRepository.GetByIdAsync(model.PostId);

        if (post == null)
        {
                Console.WriteLine("AddReaction: Post bulunamadı");
            return Json(new { success = false, message = "Yazı bulunamadı." });
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
        catch (Exception ex)
        {
            Console.WriteLine($"AddReaction error: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
            return Json(new { success = false, message = "Bir hata oluştu. Lütfen daha sonra tekrar deneyin." });
        }
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
            
            // Yorumu sil
            await _commentRepository.DeleteAsync(comment);
            
            Console.WriteLine("DeleteComment: Yorum başarıyla silindi");

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