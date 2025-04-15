using BlogApp.Data.Abstract;
using BlogApp.Entity;
using BlogApp.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.RegularExpressions;

namespace BlogApp.Controllers
{
    public class TagsController : Controller
    {
        private readonly ITagRepository _tagRepository;
        private readonly IPostRepository _postRepository;

        public TagsController(ITagRepository tagRepository, IPostRepository postRepository)
        {
            _tagRepository = tagRepository;
            _postRepository = postRepository;
        }

        [HttpGet]
        [Route("Tags")]
        [Route("Tags/Index")]
        public async Task<IActionResult> Index()
        {
            var tags = await _tagRepository.GetAllAsync();
            return View(tags);
        }

        [Route("Tags/{url}")]
        public async Task<IActionResult> Detail(string url)
        {
            var tag = await _tagRepository.GetByUrlAsync(url);
            if (tag == null)
            {
                return NotFound();
            }

            var posts = await _postRepository.GetByTagIdAsync(tag.TagId);
            
            var viewModel = new PostListViewModel
            {
                Posts = posts,
                CurrentPage = 1,
                TotalPages = 1,
                CurrentSort = "date",
                CurrentTag = tag.Url
            };

            ViewBag.Tag = tag;
            return View(viewModel);
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        [Route("Tags/Create")]
        public IActionResult Create()
        {
            return View();
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("Tags/Create")]
        public async Task<IActionResult> Create(Tag tag)
        {
            if (!ModelState.IsValid)
            {
                return View(tag);
            }

            if (string.IsNullOrEmpty(tag.Url))
            {
                tag.Url = GenerateUrl(tag.Name);
            }
            
            var existingTag = await _tagRepository.GetByUrlAsync(tag.Url);
            if (existingTag != null)
            {
                ModelState.AddModelError("Url", "Bu URL zaten kullanımda");
                return View(tag);
            }
            
            await _tagRepository.AddAsync(tag);
            TempData["success"] = "Etiket başarıyla oluşturuldu.";
            return RedirectToAction("Index");
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id)
        {
            var tag = await _tagRepository.GetByIdAsync(id);
            if (tag == null)
            {
                return NotFound();
            }

            return View(tag);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Tag tag)
        {
            if (id != tag.TagId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    if (string.IsNullOrEmpty(tag.Url))
                    {
                        tag.Url = GenerateUrl(tag.Name);
                    }
                    
                    var existingTag = await _tagRepository.GetByUrlAsync(tag.Url);
                    if (existingTag != null && existingTag.TagId != id)
                    {
                        ModelState.AddModelError("Url", "Bu URL zaten kullanımda");
                        return View(tag);
                    }

                    await _tagRepository.UpdateAsync(tag);
                    TempData["success"] = "Etiket başarıyla güncellendi.";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Etiket güncellenirken bir hata oluştu: {ex.Message}");
                    return View(tag);
                }
            }

            return View(tag);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var tag = await _tagRepository.GetByIdAsync(id);
            if (tag == null)
            {
                return NotFound();
            }

            return View(tag);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var tag = await _tagRepository.GetByIdAsync(id);
            if (tag == null)
            {
                return NotFound();
            }

            if (tag.Posts.Any())
            {
                TempData["error"] = $"Bu etiket {tag.Posts.Count} yazıda kullanılıyor. Silmeden önce yazılardan kaldırın.";
                return RedirectToAction(nameof(Index));
            }

            await _tagRepository.DeleteAsync(tag);
            TempData["success"] = "Etiket başarıyla silindi.";
            return RedirectToAction(nameof(Index));
        }

        private static string GenerateUrl(string text)
        {
            if (string.IsNullOrEmpty(text))
                return string.Empty;

            text = text.ToLower();
            text = text.Replace('ı', 'i')
                       .Replace('ğ', 'g')
                       .Replace('ü', 'u')
                       .Replace('ş', 's')
                       .Replace('ö', 'o')
                       .Replace('ç', 'c')
                       .Replace('İ', 'i');
                       
            text = Regex.Replace(text, @"[^a-z0-9\s-]", "");
            text = Regex.Replace(text, @"\s+", " ").Trim();
            text = Regex.Replace(text, @"\s", "-");
            return text;
        }
    }
} 