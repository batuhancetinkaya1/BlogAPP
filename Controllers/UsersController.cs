using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BlogApp.Entity;
using BlogApp.Models.ViewModels;
using BlogApp.Models;
using BlogApp.Data.Abstract;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using BCrypt.Net;
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;
using BlogApp.Helpers;
using BlogApp.Services;

namespace BlogApp.Controllers;

public class UsersController : Controller
{
    private readonly IUserRepository _userRepository;
    private readonly IPostRepository _postRepository;
    private readonly ITagRepository _tagRepository;
    private readonly INotificationService _notificationService;

    public UsersController(IUserRepository userRepository, IPostRepository postRepository, ITagRepository tagRepository, INotificationService notificationService)
    {
        _userRepository = userRepository;
        _postRepository = postRepository;
        _tagRepository = tagRepository;
        _notificationService = notificationService;
    }

    [HttpGet]
    [Route("Users/Login")]
    public IActionResult Login()
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToAction("Index", "Home");
        }
        return View();
    }

    [HttpPost]
    [Route("Users/Login")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var user = await _userRepository.GetByEmailAsync(model.Email);
        if (user == null || !BCrypt.Net.BCrypt.Verify(model.Password, user.Password))
        {
            ModelState.AddModelError("", "Geçersiz email veya şifre");
            return View(model);
        }

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
            new Claim(ClaimTypes.Name, user.UserName),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.IsAdmin ? "Admin" : "User"),
            new Claim("Image", user.Image ?? "")
        };

        var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var authProperties = new AuthenticationProperties
        {
            IsPersistent = model.RememberMe,
            ExpiresUtc = DateTimeOffset.UtcNow.AddDays(7)
        };

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(claimsIdentity),
            authProperties);

        // Update user login timestamp
        user.LastLogin = DateTime.UtcNow;
        await _userRepository.UpdateAsync(user);

        TempData["success"] = "Başarıyla giriş yaptınız.";
        return RedirectToAction("Index", "Home");
    }

    [HttpGet]
    [Route("Users/Register")]
    public IActionResult Register()
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToAction("Index", "Home");
        }
        return View();
    }

    [HttpPost]
    [Route("Users/Register")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var exists = await _userRepository.ExistsAsync(model.Email);
        if (exists)
        {
            ModelState.AddModelError("Email", "Bu email adresi zaten kullanımda");
            return View(model);
        }

        var user = new User
        {
            UserName = model.UserName,
            Email = model.Email,
            Password = BCrypt.Net.BCrypt.HashPassword(model.Password),
            CreatedAt = DateTime.UtcNow,
            IsAdmin = false,
            Image = "/img/profiles/default.jpg"
        };

        await _userRepository.AddAsync(user);

        TempData["success"] = "Kayıt işlemi başarılı. Giriş yapabilirsiniz.";
        return RedirectToAction(nameof(Login));
    }

    [Authorize]
    [HttpGet]
    [Route("Users/Logout")]
    public IActionResult LogoutConfirm()
    {
        // For security, redirect to POST method for actual logout
        return View("Logout");
    }

    [Authorize]
    [HttpPost]
    [Route("Users/Logout")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        TempData["success"] = "Başarıyla çıkış yaptınız.";
        return RedirectToAction("Index", "Home");
    }

    [Route("users/{username}")]
    public async Task<IActionResult> Profile(string username)
    {
        var user = await _userRepository.GetByUsernameAsync(username);
        if (user == null)
        {
            return NotFound();
        }

        var viewModel = new UserProfileViewModel
        {
            User = user,
            Posts = await _postRepository.GetByUserIdAsync(user.UserId)
        };

        return View(viewModel);
    }

    [Route("users/profile/{id}")]
    public async Task<IActionResult> ProfileById(int id)
    {
        var user = await _userRepository.GetByIdAsync(id);
        if (user == null)
        {
            return NotFound();
        }
        
        // username route'a yönlendir
        return RedirectToAction("Profile", new { username = user.UserName });
    }

    [Authorize]
    [HttpPost]
    [Route("Users/UpdateProfile")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateProfile(ProfileUpdateModel model)
    {
        try
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var user = await _userRepository.GetByIdAsync(userId);
            
            if (user == null)
            {
                _notificationService.Error("Kullanıcı bulunamadı.");
                return RedirectToAction("Profile", new { username = User.Identity?.Name });
            }

            // Kullanıcı adı güncellemesi
            if (!string.IsNullOrEmpty(model.UserName) && model.UserName != user.UserName)
            {
                // Kullanıcı adı özel karakterler ve boşluk içermemeli
                if (!System.Text.RegularExpressions.Regex.IsMatch(model.UserName, "^[a-zA-Z0-9_-]+$"))
                {
                    _notificationService.Error("Kullanıcı adı sadece harf, rakam, tire ve alt çizgi içerebilir.");
                    return RedirectToAction("Profile", new { username = user.UserName });
                }

                var existingUser = await _userRepository.GetByUsernameAsync(model.UserName);
                if (existingUser != null && existingUser.UserId != userId)
                {
                    _notificationService.Error("Bu kullanıcı adı başka bir kullanıcı tarafından kullanılıyor.");
                    return RedirectToAction("Profile", new { username = user.UserName });
                }
                user.UserName = model.UserName;
            }

            // Email güncellemesi
            if (!string.IsNullOrEmpty(model.Email) && model.Email.Contains("@"))
            {
                var existingUserByEmail = await _userRepository.GetByEmailAsync(model.Email);
                if (existingUserByEmail != null && existingUserByEmail.UserId != userId)
                {
                    _notificationService.Error("Bu e-posta adresi başka bir kullanıcı tarafından kullanılıyor.");
                    return RedirectToAction("Profile", new { username = user.UserName });
                }
                user.Email = model.Email;
            }
            else
            {
                _notificationService.Error("Geçerli bir e-posta adresi giriniz.");
                return RedirectToAction("Profile", new { username = user.UserName });
            }

            // Profil resmi güncelleme
            if (model.Image != null && model.Image.Length > 0)
            {
                try
                {
                    // Eğer kullanıcının mevcut bir resmi varsa onu sil
                    if (!string.IsNullOrEmpty(user.Image) && !user.Image.Contains("default.jpg"))
                    {
                        await ImageHelper.DeleteImageFileAsync(user.Image);
                    }

                    // Yeni profil resmini doğrula ve kaydet
                    user.Image = await ImageHelper.ValidateAndSaveProfileImageAsync(model.Image);
                }
                catch (ImageValidationException ex)
                {
                    _notificationService.Error(ex.Message);
                    return RedirectToAction("Profile", new { username = user.UserName });
                }
                catch (Exception ex)
                {
                    _notificationService.Error($"Resim yükleme hatası: {ex.Message}");
                    return RedirectToAction("Profile", new { username = user.UserName });
                }
            }

            // Şifre güncelleme
            if (!string.IsNullOrEmpty(model.Password))
            {
                if (model.Password.Length < 6)
                {
                    _notificationService.Error("Şifre en az 6 karakter olmalıdır.");
                    return RedirectToAction("Profile", new { username = user.UserName });
                }
                user.Password = BCrypt.Net.BCrypt.HashPassword(model.Password);
            }

            await _userRepository.UpdateAsync(user);
            await UpdateAuthenticationCookie(user);

            _notificationService.Success("Profil başarıyla güncellendi.");
            return RedirectToAction("Profile", new { username = user.UserName });
        }
        catch (Exception ex)
        {
            _notificationService.Error($"Bir hata oluştu: {ex.Message}");
            return RedirectToAction("Profile", new { username = User.Identity?.Name });
        }
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateSettings(SettingsUpdateModel model)
    {
        try
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            Console.WriteLine($"UpdateSettings çağrıldı. UserId: {userId}");
            
            var user = await _userRepository.GetByIdAsync(userId);
            
            if (user == null)
            {
                Console.WriteLine("Kullanıcı bulunamadı");
                return Json(new { success = false, message = "Kullanıcı bulunamadı." });
            }

            var email = model.Email;
            Console.WriteLine($"Gelen email adresi: {email}");
            
            if (string.IsNullOrEmpty(email) || !email.Contains("@"))
            {
                Console.WriteLine("Geçersiz email format");
                return Json(new { success = false, message = "Geçerli bir email adresi giriniz." });
            }

            var existingUser = await _userRepository.GetByEmailAsync(email);
            if (existingUser != null && existingUser.UserId != userId)
            {
                Console.WriteLine("Email adresi başka bir kullanıcı tarafından kullanılıyor");
                return Json(new { success = false, message = "Bu e-posta adresi başka bir kullanıcı tarafından kullanılıyor." });
            }

            user.Email = email;
            await _userRepository.UpdateAsync(user);
            Console.WriteLine("Kullanıcı email adresi güncellendi");
            
            await UpdateAuthenticationCookie(user);
            Console.WriteLine("Kimlik bilgileri güncellendi");

            return Json(new { success = true, message = "Ayarlar başarıyla güncellendi." });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"UpdateSettings hatası: {ex.Message}");
            Console.WriteLine($"Stack Trace: {ex.StackTrace}");
            return Json(new { success = false, message = $"Bir hata oluştu: {ex.Message}" });
        }
    }

    private async Task UpdateAuthenticationCookie(User user)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
            new Claim(ClaimTypes.Name, user.UserName),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.IsAdmin ? "Admin" : "User"),
            new Claim("Image", user.Image ?? "")
        };

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);

        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);
    }

    [Authorize]
    public IActionResult ChangePassword()
    {
        return View();
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        var user = await _userRepository.GetByIdAsync(userId);

        if (user == null)
        {
            return NotFound();
        }

        if (!BCrypt.Net.BCrypt.Verify(model.CurrentPassword, user.Password))
        {
            ModelState.AddModelError("CurrentPassword", "Mevcut şifre hatalı");
            return View(model);
        }

        user.Password = BCrypt.Net.BCrypt.HashPassword(model.NewPassword);
        await _userRepository.UpdateAsync(user);

        TempData["success"] = "Şifreniz başarıyla değiştirildi.";
        return RedirectToAction("Profile", new { username = user.UserName });
    }

    [Authorize]
    [HttpGet]
    [Route("Users/MyPosts")]
    public async Task<IActionResult> MyPosts(int pageNumber = 1, int pageSize = 10)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        var posts = await _postRepository.GetByUserIdAsync(userId);
        
        // Order posts by CreatedAt and apply pagination
        var orderedPosts = posts.OrderByDescending(p => p.CreatedAt).ToList();
        var paginatedPosts = BlogApp.Helpers.PaginatedList<Post>.Create(orderedPosts, pageNumber, pageSize);

        return View(paginatedPosts);
    }

    public IActionResult AccessDenied()
    {
        return View();
    }

    // Profil resmi güncelleme için gelişmiş dosya kontrolü
    private (bool isValid, string? errorMessage) ValidateImage(IFormFile imageFile)
    {
        if (imageFile == null || imageFile.Length == 0)
            return (false, "Geçersiz dosya.");

        // Dosya uzantısı kontrolü
        var extension = Path.GetExtension(imageFile.FileName).ToLowerInvariant();
        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };
        
        if (!allowedExtensions.Contains(extension))
            return (false, "Sadece JPG ve PNG dosyalarına izin verilmektedir.");

        // Dosya boyutu kontrolü (2MB)
        if (imageFile.Length > 2 * 1024 * 1024)
            return (false, "Dosya boyutu 2MB'ı geçemez.");

        return (true, null);
    }
} 