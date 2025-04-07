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
using BlogApp.Data.Abstract;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using BCrypt.Net;
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace BlogApp.Controllers;

public class UsersController : Controller
{
    private readonly IUserRepository _userRepository;
    private readonly IPostRepository _postRepository;

    public UsersController(IUserRepository userRepository, IPostRepository postRepository)
    {
        _userRepository = userRepository;
        _postRepository = postRepository;
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
            IsAdmin = false
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

        // Kullanıcı resmi yoksa varsayılan resmi kullan
        if (string.IsNullOrEmpty(user.Image))
        {
            user.Image = "/img/profiles/default.jpg";
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
        if (!ModelState.IsValid)
        {
            foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
            {
                TempData["error"] = error.ErrorMessage;
            }
            
            var username = User.Identity?.Name;
            return RedirectToAction("Profile", new { username });
        }

        try
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var user = await _userRepository.GetByIdAsync(userId);
            
            if (user == null)
            {
                TempData["error"] = "Kullanıcı bulunamadı.";
                return RedirectToAction("Index", "Home");
            }

            // Kullanıcı adı güncelleme
            if (!string.IsNullOrEmpty(model.UserName) && model.UserName.Length >= 3 && model.UserName.Length <= 50)
            {
                // Kullanıcı adı başka bir kullanıcı tarafından kullanılıyor mu kontrol et
                var existingUser = await _userRepository.GetByUsernameAsync(model.UserName);
                if (existingUser != null && existingUser.UserId != userId)
                {
                    TempData["error"] = "Bu kullanıcı adı başka bir kullanıcı tarafından kullanılıyor.";
                    return RedirectToAction("Profile", new { username = user.UserName });
                }
                user.UserName = model.UserName;
            }
            else
            {
                TempData["error"] = "Kullanıcı adı 3-50 karakter arasında olmalıdır.";
                return RedirectToAction("Profile", new { username = user.UserName });
            }

            // Email güncelleme
            if (!string.IsNullOrEmpty(model.Email) && model.Email.Contains("@"))
            {
                // Email başka bir kullanıcı tarafından kullanılıyor mu kontrol et
                var existingUserByEmail = await _userRepository.GetByEmailAsync(model.Email);
                if (existingUserByEmail != null && existingUserByEmail.UserId != userId)
                {
                    TempData["error"] = "Bu e-posta adresi başka bir kullanıcı tarafından kullanılıyor.";
                    return RedirectToAction("Profile", new { username = user.UserName });
                }
                user.Email = model.Email;
            }
            else
            {
                TempData["error"] = "Geçerli bir e-posta adresi giriniz.";
                return RedirectToAction("Profile", new { username = user.UserName });
            }

            // Profil resmi güncelleme
            if (model.Image != null && model.Image.Length > 0)
            {
                try
                {
                    // Görüntü dosyasının uzantısını kontrol et
                    var extension = Path.GetExtension(model.Image.FileName).ToLowerInvariant();
                    if (extension != ".jpg" && extension != ".jpeg" && extension != ".png")
                    {
                        TempData["error"] = "Sadece JPG ve PNG dosyalarına izin verilmektedir.";
                        return RedirectToAction("Profile", new { username = user.UserName });
                    }

                    // Dosya boyutu kontrolü
                    if (model.Image.Length > 2 * 1024 * 1024) // 2MB
                    {
                        TempData["error"] = "Dosya boyutu 2MB'ı geçemez.";
                        return RedirectToAction("Profile", new { username = user.UserName });
                    }

                    // Dosya adı oluştur
                    var fileName = $"{Guid.NewGuid()}{extension}";
                    var uploadsDirectory = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "img", "profiles");
                    if (!Directory.Exists(uploadsDirectory))
                    {
                        Directory.CreateDirectory(uploadsDirectory);
                    }

                    var filePath = Path.Combine(uploadsDirectory, fileName);

                    // Eğer kullanıcının mevcut bir resmi varsa onu sil
                    if (!string.IsNullOrEmpty(user.Image) && !user.Image.Contains("default.jpg"))
                    {
                        try 
                        {
                            var oldImagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", user.Image.TrimStart('/'));
                            Console.WriteLine($"Silinecek eski dosya yolu: {oldImagePath}");
                            
                            if (System.IO.File.Exists(oldImagePath))
                            {
                                System.IO.File.Delete(oldImagePath);
                                Console.WriteLine("Eski dosya silindi");
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Eski dosya silinirken hata: {ex.Message} - devam edilecek");
                        }
                    }

                    // Yeni dosyayı kaydet
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        model.Image.CopyTo(fileStream);
                        Console.WriteLine("Yeni resim dosyaya kopyalandı");
                    }

                    // Veritabanında kullanıcı nesnesini güncelle
                    var newImagePath = $"/img/profiles/{fileName}";
                    Console.WriteLine($"Yeni resim yolu: {newImagePath}");
                    user.Image = newImagePath;
                }
                catch (Exception ex) 
                {
                    Console.WriteLine($"Resim işleme hatası: {ex.Message}");
                    TempData["error"] = $"Resim yükleme hatası: {ex.Message}";
                    return RedirectToAction("Profile", new { username = user.UserName });
                }
            }

            // Şifre güncelleme
            if (!string.IsNullOrEmpty(model.Password))
            {
                if (model.Password.Length < 6)
                {
                    TempData["error"] = "Şifre en az 6 karakter olmalıdır.";
                    return RedirectToAction("Profile", new { username = user.UserName });
                }
                user.Password = BCrypt.Net.BCrypt.HashPassword(model.Password);
            }

            await _userRepository.UpdateAsync(user);
            await UpdateAuthenticationCookie(user);

            TempData["success"] = "Profil başarıyla güncellendi.";
            return RedirectToAction("Profile", new { username = user.UserName });
        }
        catch (Exception ex)
        {
            TempData["error"] = $"Bir hata oluştu: {ex.Message}";
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
        
        var totalPosts = posts.Count;
        var paginatedPosts = posts
            .OrderByDescending(p => p.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        ViewBag.TotalPosts = totalPosts;
        ViewBag.CurrentPage = pageNumber;
        ViewBag.TotalPages = (int)Math.Ceiling(totalPosts / (double)pageSize);

        return View(paginatedPosts);
    }

    public IActionResult AccessDenied()
    {
        return View();
    }
} 