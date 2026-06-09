using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Rendering;
using GearDTK.Data;
using GearDTK.ViewModels;

namespace GearDTK.Controllers;

public class AccountController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    public AccountController(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        RoleManager<IdentityRole> roleManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _roleManager = roleManager;
    }

    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;

        if (ModelState.IsValid)
        {
            var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, lockoutOnFailure: false);

            if (result.Succeeded)
            {
                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user != null)
                {
                    user.LastLoginAt = DateTime.UtcNow;
                    await _userManager.UpdateAsync(user);
                }

                var roles = await _userManager.GetRolesAsync(user!);

                if (roles.Contains("Admin"))
                    return RedirectToAction("Index", "Dashboard", new { area = "Admin" });
                if (roles.Contains("Employee"))
                    return RedirectToAction("Index", "Dashboard", new { area = "Employee" });
                if (roles.Contains("Supplier"))
                    return RedirectToAction("Index", "Dashboard", new { area = "Supplier" });

                return RedirectToLocal(returnUrl);
            }

            ModelState.AddModelError(string.Empty, "Email hoặc mật khẩu không đúng.");
        }

        return View(model);
    }

    [HttpGet]
    [HttpGet]
    public async Task<IActionResult> Register()
    {
        var model = new RegisterViewModel();

        // Nếu là Admin thì mới hiển thị full role selection
        if (User.Identity.IsAuthenticated && User.IsInRole("Admin"))
        {
            model.SelectedRole = "Customer";
        }
        else
        {
            model.SelectedRole = "Customer"; // Mặc định là Customer
        }

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (ModelState.IsValid)
        {
            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                EmailConfirmed = true,
                FullName = model.FullName,
                Phone = model.Phone,
                Address = model.Address,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                // Xác định role
                string roleToAssign = "Customer";

                // Chỉ Admin mới có thể chọn role khác
                if (User.Identity.IsAuthenticated && User.IsInRole("Admin"))
                {
                    roleToAssign = model.SelectedRole;
                }

                // Kiểm tra và tạo role nếu chưa tồn tại
                if (!await _roleManager.RoleExistsAsync(roleToAssign))
                {
                    await _roleManager.CreateAsync(new IdentityRole(roleToAssign));
                }

                await _userManager.AddToRoleAsync(user, roleToAssign);

                // Nếu không phải Admin tạo, tự động đăng nhập
                if (!(User.Identity.IsAuthenticated && User.IsInRole("Admin")))
                {
                    await _signInManager.SignInAsync(user, isPersistent: false);
                }

                TempData["Success"] = $"Tạo tài khoản thành công! Vai trò: {roleToAssign}";
                return RedirectToAction("Index", "Home");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }

        return View(model);
    }

    [HttpGet]
    public IActionResult Logout()
    {
        return View();
    }

    // POST: Account/Logout
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> LogoutConfirmed()
    {
        await _signInManager.SignOutAsync();
        TempData["Success"] = "Bạn đã đăng xuất thành công!";
        return RedirectToAction("Index", "Home");
    }


    public IActionResult AccessDenied()
    {
        return View();
    }

    private IActionResult RedirectToLocal(string? returnUrl)
    {
        if (Url.IsLocalUrl(returnUrl))
            return Redirect(returnUrl);
        return RedirectToAction("Index", "Home");
    }
}