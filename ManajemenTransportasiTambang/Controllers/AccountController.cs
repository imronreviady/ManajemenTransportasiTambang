using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ManajemenTransportasiTambang.Models;
using ManajemenTransportasiTambang.Services;
using ManajemenTransportasiTambang.ViewModels;
using System.Security.Claims;

namespace ManajemenTransportasiTambang.Controllers;

public class AccountController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly ILogService _logService;

    public AccountController(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        ILogService logService)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _logService = logService;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> Login(string? returnUrl = null)
    {
        await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        
        if (ModelState.IsValid)
        {
            var result = await _signInManager.PasswordSignInAsync(model.Username, model.Password, model.RememberMe, lockoutOnFailure: true);
            
            if (result.Succeeded)
            {
                // Log the successful login
                var user = await _userManager.FindByNameAsync(model.Username);
                await _logService.LogActivityAsync(
                    user!.Id,
                    user.UserName!,
                    "Login",
                    "Authentication",
                    "User logged in",
                    null,
                    HttpContext.Connection.RemoteIpAddress?.ToString()
                );
                
                return RedirectToLocal(returnUrl);
            }
            
            if (result.IsLockedOut)
            {
                return View("Lockout");
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                return View(model);
            }
        }

        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Logout()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var userName = User.Identity?.Name ?? "Unknown";
        
        await _signInManager.SignOutAsync();
        
        // Log the logout
        await _logService.LogActivityAsync(
            userId ?? "Anonymous",
            userName,
            "Logout",
            "Authentication",
            "User logged out",
            null,
            HttpContext.Connection.RemoteIpAddress?.ToString()
        );
        
        return RedirectToAction(nameof(HomeController.Index), "Home");
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> Profile()
    {
        var user = await _userManager.GetUserAsync(User);
        
        if (user == null)
        {
            return NotFound();
        }

        var model = new ProfileViewModel
        {
            Username = user.UserName!,
            Email = user.Email!,
            FullName = user.FullName,
            Department = user.Department,
            Position = user.Position,
            PhoneNumber = user.PhoneNumber
        };

        return View(model);
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Profile(ProfileViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return NotFound();
        }

        // Update user profile information
        user.FullName = model.FullName;
        user.Department = model.Department;
        user.Position = model.Position;
        user.PhoneNumber = model.PhoneNumber;

        var result = await _userManager.UpdateAsync(user);
        if (result.Succeeded)
        {
            // Log activity
            if (_logService != null)
            {
                await _logService.LogActivityAsync(
                    user.Id,
                    user.UserName ?? "Unknown",
                    "Update",
                    "Profile",
                    "Updated personal profile information",
                    null,
                    HttpContext.Connection.RemoteIpAddress?.ToString()
                );
            }

            TempData["StatusMessage"] = "Profile has been updated successfully";
            TempData["StatusType"] = "Success";
        }
        else
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
            TempData["StatusMessage"] = "Error updating profile";
            TempData["StatusType"] = "Error";
        }

        return View(model);
    }

    [HttpGet]
    [Authorize]
    public IActionResult ChangePassword()
    {
        return View();
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return NotFound();
        }

        var result = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);
        
        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
            return View(model);
        }

        await _signInManager.RefreshSignInAsync(user);
        
        // Log the password change
        await _logService.LogActivityAsync(
            user.Id,
            user.UserName!,
            "ChangePassword",
            "Authentication",
            "User changed password",
            null,
            HttpContext.Connection.RemoteIpAddress?.ToString()
        );
        
        TempData["StatusMessage"] = "Your password has been changed successfully.";
        return RedirectToAction(nameof(Profile));
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> AccessDenied()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user != null)
        {
            await _logService.LogActivityAsync(
                user.Id,
                user.UserName!,
                "AccessDenied",
                "Authorization",
                "Access denied to a protected resource",
                null,
                HttpContext.Connection.RemoteIpAddress?.ToString()
            );
        }
        
        return View();
    }

    private IActionResult RedirectToLocal(string? returnUrl)
    {
        if (Url.IsLocalUrl(returnUrl))
        {
            return Redirect(returnUrl);
        }
        else
        {
            return RedirectToAction(nameof(HomeController.Index), "Home");
        }
    }
}
