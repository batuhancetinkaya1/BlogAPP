using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BlogApp.Entity;
using BlogApp.Models;
using BlogApp.Data.Abstract;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

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

    public IActionResult Login()
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToAction("Index", "Home");
        }
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var user = _userRepository.GetByEmail(model.Email ?? "");
        if (user == null || !BCrypt.Net.BCrypt.Verify(model.Password ?? "", user.Password))
        {
            ModelState.AddModelError("", "Geçersiz email veya şifre");
            return View(model);
        }

        if (!user.IsActive)
        {
            ModelState.AddModelError("", "Hesabınız aktif değil");
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

        // Update last login
        user.LastLogin = DateTime.UtcNow;
        _userRepository.EditUser(user);
        await _userRepository.SaveChangesAsync();

        TempData["Message"] = "Başarıyla giriş yaptınız.";
        TempData["MessageType"] = "success";
        return RedirectToAction("Index", "Home");
    }

    public IActionResult Register()
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToAction("Index", "Home");
        }
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var existingUser = _userRepository.GetByEmail(model.Email ?? "");
        if (existingUser != null)
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
            IsActive = true,
            IsAdmin = false
        };

        _userRepository.CreateUser(user);
        await _userRepository.SaveChangesAsync();

        TempData["Message"] = "Kayıt işlemi başarılı. Giriş yapabilirsiniz.";
        TempData["MessageType"] = "success";
        return RedirectToAction(nameof(Login));
    }

    [Authorize]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        TempData["Message"] = "Başarıyla çıkış yaptınız.";
        TempData["MessageType"] = "success";
        return RedirectToAction("Index", "Home");
    }

    [Authorize]
    public IActionResult Profile()
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        var user = _userRepository.GetById(userId);
        if (user == null)
        {
            return NotFound();
        }

        var model = new ProfileViewModel
        {
            UserName = user.UserName,
            Email = user.Email,
            Image = user.Image
        };

        return View(model);
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> Profile(ProfileViewModel model, IFormFile? imageFile)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        var user = _userRepository.GetById(userId);
        if (user == null)
        {
            return NotFound();
        }

        // Check if email is already in use by another user
        var existingUser = _userRepository.GetByEmail(model.Email ?? "");
        if (existingUser != null && existingUser.UserId != userId)
        {
            ModelState.AddModelError("Email", "Bu email adresi zaten kullanımda");
            return View(model);
        }

        // Handle image upload
        if (imageFile != null)
        {
            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(imageFile.FileName)}";
            var uploadDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "img", "users");
            Directory.CreateDirectory(uploadDir);
            var path = Path.Combine(uploadDir, fileName);
            
            using (var stream = new FileStream(path, FileMode.Create))
            {
                await imageFile.CopyToAsync(stream);
            }

            // Delete old image if exists
            if (!string.IsNullOrEmpty(user.Image))
            {
                var oldPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "img", "users", user.Image);
                if (System.IO.File.Exists(oldPath))
                {
                    System.IO.File.Delete(oldPath);
                }
            }

            user.Image = $"users/{fileName}";
        }

        user.UserName = model.UserName;
        user.Email = model.Email;

        if (!string.IsNullOrEmpty(model.NewPassword))
        {
            user.Password = BCrypt.Net.BCrypt.HashPassword(model.NewPassword);
        }

        _userRepository.EditUser(user);
        await _userRepository.SaveChangesAsync();

        // Update claims if username changed
        if (model.UserName != User.Identity?.Name)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.IsAdmin ? "Admin" : "User"),
                new Claim("Image", user.Image ?? "")
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity));
        }

        TempData["Message"] = "Profil başarıyla güncellendi.";
        TempData["MessageType"] = "success";
        return RedirectToAction(nameof(Profile));
    }

    [Authorize]
    public IActionResult ChangePassword()
    {
        return View(new ChangePasswordViewModel());
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
        var user = _userRepository.GetById(userId);
        if (user == null)
        {
            return NotFound();
        }

        if (!BCrypt.Net.BCrypt.Verify(model.CurrentPassword, user.Password))
        {
            ModelState.AddModelError("CurrentPassword", "Mevcut şifre yanlış");
            return View(model);
        }

        user.Password = BCrypt.Net.BCrypt.HashPassword(model.NewPassword);
        _userRepository.UpdateUser(user);
        await _userRepository.SaveChangesAsync();

        TempData["success"] = "Şifreniz başarıyla değiştirildi.";
        return RedirectToAction(nameof(Profile));
    }

    [Authorize]
    public IActionResult MyPosts(int pageNumber = 1, int pageSize = 10)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        var posts = _postRepository.Posts
            .Where(p => p.UserId == userId)
            .OrderByDescending(p => p.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Include(p => p.Tags)
            .Include(p => p.Comments)
            .ToList();

        ViewBag.TotalPosts = _postRepository.Posts.Count(p => p.UserId == userId);
        ViewBag.CurrentPage = pageNumber;
        ViewBag.TotalPages = (int)Math.Ceiling(ViewBag.TotalPosts / (double)pageSize);

        return View(posts);
    }

    public IActionResult AccessDenied()
    {
        return View();
    }
} 