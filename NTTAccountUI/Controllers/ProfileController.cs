using Microsoft.AspNetCore.Mvc;
using NTTAccountUI.Business.Validators;
using NTTAccountUI.Data.Repositories;
using NTTAccountUI.Models.ViewModels;
using NTTAccountUI.Security;
using NTTAccountUI.Services;

namespace NTTAccountUI.Controllers;

public class ProfileController : AdminBaseController
{
    private readonly IUserRepository _userRepo;
    private readonly ILogService _logService;
    private const string ProtectedEmail = "admin@dogukankosan.com";
    public ProfileController(
        ISiteSettingsRepository siteSettingsRepository,
        IContactRepository contactRepository,
        IUserRepository userRepository,
        ILogService logService)
        : base(siteSettingsRepository, contactRepository, userRepository)
    {
        _userRepo = userRepository;
        _logService = logService;
    }

    // GET: /Profile
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        int userId = (int)(HttpContext.Items["AdminUserId"] ?? 0);
        int roleId = HttpContext.Items["AdminRoleId"] is byte r ? (int)r : 2;
        var user = await _userRepo.GetByIdAsync(userId);
        if (user == null) return RedirectToAction("Index", "AdminLogin");
        // Admin profil view
        if (roleId == 1)
        {
            AdminProfileViewModel model = new AdminProfileViewModel
            {
                Id = user.Id,
                Email = user.Email,
                RoleId = user.RoleId,
                Note = user.Note,
                ProfileImage = user.ProfileImage,
                IsProtected = user.Email.ToLower() == ProtectedEmail.ToLower()
            };
            return View("AdminProfile", model);
        }
        else
        {
            UserProfileViewModel model = new UserProfileViewModel
            {
                Id = user.Id,
                Email = user.Email,
                RoleId = user.RoleId,
                IsActive = user.IsActive,
                Note = user.Note,
                ProfileImage = user.ProfileImage
            };
            return View("UserProfile", model);
        }
    }
    // POST: /Profile/UpdateAdmin
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateAdmin(AdminProfileViewModel model)
    {
        int userId = (int)(HttpContext.Items["AdminUserId"] ?? 0);
        try
        {
            if (model.ProfileImageFile != null && model.ProfileImageFile.Length > 0)
                model.ProfileImage = await ToBase64(model.ProfileImageFile);
            ProfileValidator validator = new ProfileValidator()
                .ValidatePassword(model.NewPassword, model.ConfirmPassword)
                .ValidateEmail(model.NewEmail);
            if (!validator.IsValid)
            {
                foreach (string error in validator.Errors)
                    ModelState.AddModelError(string.Empty, error);
                return View("AdminProfile", model);
            }
            // Korumalı kullanıcıya email değiştirme yok
            if (!model.IsProtected && !string.IsNullOrEmpty(model.NewEmail))
            {
                if (await _userRepo.EmailExistsAsync(model.NewEmail, model.Id))
                {
                    ModelState.AddModelError(string.Empty, "Bu email zaten kullanılıyor.");
                    return View("AdminProfile", model);
                }
            }
            // Profil güncelle
            var user = await _userRepo.GetByIdAsync(model.Id);
            if (user == null) return RedirectToAction("Index");
            user.Note = model.Note;
            user.ProfileImage = model.ProfileImage ?? user.ProfileImage;
            // Korumalı değilse email güncellenebilir
            if (!model.IsProtected && !string.IsNullOrEmpty(model.NewEmail))
                user.Email = model.NewEmail.Trim().ToLowerInvariant();
            await _userRepo.AdminUpdateAsync(user);
            // Şifre değişikliği
            if (!model.IsProtected && !string.IsNullOrEmpty(model.NewPassword))
                await _userRepo.AdminUpdatePasswordAsync(model.Id, PasswordHasher.Hash(model.NewPassword));
            TempData["Success"] = "Profiliniz başarıyla güncellendi.";
        }
        catch (Exception ex)
        {
            await _logService.LogErrorAsync(ex, "ProfileController.UpdateAdmin", userId);
            TempData["Error"] = "Bir hata oluştu.";
        }
        return RedirectToAction("Index");
    }
    // POST: /Profile/UpdateUser
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateUser(UserProfileViewModel model)
    {
        int userId = (int)(HttpContext.Items["AdminUserId"] ?? 0);
        try
        {
            if (model.ProfileImageFile != null && model.ProfileImageFile.Length > 0)
                model.ProfileImage = await ToBase64(model.ProfileImageFile);
            ProfileValidator validator = new ProfileValidator()
                .ValidatePassword(model.NewPassword, model.ConfirmPassword);
            if (!validator.IsValid)
            {
                foreach (var error in validator.Errors)
                    ModelState.AddModelError(string.Empty, error);
                return View("UserProfile", model);
            }
            string? passwordHash = string.IsNullOrEmpty(model.NewPassword)
                ? null
                : PasswordHasher.Hash(model.NewPassword);
            await _userRepo.UserUpdateAsync(
                userId,
                passwordHash,
                model.Note,
                model.ProfileImage
            );
            TempData["Success"] = "Profiliniz başarıyla güncellendi.";
        }
        catch (Exception ex)
        {
            await _logService.LogErrorAsync(ex, "ProfileController.UpdateUser", userId);
            TempData["Error"] = "Bir hata oluştu.";
        }
        return RedirectToAction("Index");
    }
    private static async Task<string> ToBase64(IFormFile file)
    {
        using MemoryStream ms = new MemoryStream();
        await file.CopyToAsync(ms);
        return $"data:{file.ContentType};base64,{Convert.ToBase64String(ms.ToArray())}";
    }
}