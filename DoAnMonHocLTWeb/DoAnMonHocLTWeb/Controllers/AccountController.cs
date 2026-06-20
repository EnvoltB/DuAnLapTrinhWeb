using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using GearDTK.Data;
using GearDTK.ViewModels;
using GearDTK.Models;

namespace GearDTK.Controllers;

public class AccountController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly ApplicationDbContext _context;

    public AccountController(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        RoleManager<IdentityRole> roleManager,
        ApplicationDbContext context)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _roleManager = roleManager;
        _context = context;
    }

    // ========== LOGIN ==========
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

    // ========== REGISTER ==========
    [HttpGet]
    public async Task<IActionResult> Register()
    {
        var model = new RegisterViewModel();
        model.SelectedRole = "Customer";
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
                string roleToAssign = model.SelectedRole;

                if (!await _roleManager.RoleExistsAsync(roleToAssign))
                {
                    await _roleManager.CreateAsync(new IdentityRole(roleToAssign));
                }

                await _userManager.AddToRoleAsync(user, roleToAssign);
                await _signInManager.SignInAsync(user, isPersistent: false);

                TempData["Success"] = "Đăng ký tài khoản thành công!";
                return RedirectToAction("Index", "Home");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }

        return View(model);
    }

    // ========== LOGOUT ==========
    [HttpGet]
    public IActionResult Logout()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> LogoutConfirmed()
    {
        await _signInManager.SignOutAsync();
        TempData["Success"] = "Bạn đã đăng xuất thành công!";
        return RedirectToAction("Index", "Home");
    }

    // ========== ACCESS DENIED ==========
    public IActionResult AccessDenied()
    {
        return View();
    }

    // ========== PROFILE ==========
    [Authorize]
    [HttpGet]
    public async Task<IActionResult> Profile()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return NotFound();

        var model = new ProfileViewModel
        {
            Email = user.Email ?? "",
            FullName = user.FullName ?? "",
            Phone = user.Phone ?? "",
            Address = user.Address ?? ""
        };

        return View(model);
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Profile(ProfileViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var user = await _userManager.GetUserAsync(User);
        if (user == null) return NotFound();

        user.FullName = model.FullName;
        user.Phone = model.Phone;
        user.Address = model.Address;

        var result = await _userManager.UpdateAsync(user);

        if (result.Succeeded)
        {
            TempData["Success"] = "Cập nhật thông tin thành công!";
            return RedirectToAction("Profile");
        }

        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(string.Empty, error.Description);
        }

        return View(model);
    }

    // ========== ORDER HISTORY ==========
    [Authorize]
    public async Task<IActionResult> OrderHistory()
    {
        var userEmail = User.Identity?.Name;

        if (string.IsNullOrEmpty(userEmail))
        {
            return RedirectToAction("Login", "Account");
        }

        var orders = await _context.Orders
            .Include(o => o.OrderItems)
            .Where(o => o.Email == userEmail)
            .OrderByDescending(o => o.OrderDate)
            .ToListAsync();

        return View(orders);
    }

    // ========== ORDER DETAIL ==========
    [Authorize]
    public async Task<IActionResult> OrderDetail(int id)
    {
        var userEmail = User.Identity?.Name;

        var order = await _context.Orders
            .Include(o => o.OrderItems)
            .FirstOrDefaultAsync(o => o.Id == id && o.Email == userEmail);

        if (order == null) return NotFound();

        return View(order);
    }

    // ========== CANCEL ORDER ==========
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> CancelOrder(int id)
    {
        var userEmail = User.Identity?.Name;
        var order = await _context.Orders.FirstOrDefaultAsync(o => o.Id == id && o.Email == userEmail);

        if (order == null)
            return Json(new { success = false, message = "Không tìm thấy đơn hàng" });

        if (order.OrderStatus == "Pending" || order.OrderStatus == "Processing")
        {
            order.OrderStatus = "Cancelled";
            await _context.SaveChangesAsync();
            return Json(new { success = true, message = "Đã hủy đơn hàng thành công" });
        }

        return Json(new { success = false, message = "Không thể hủy đơn hàng này" });
    }
    // ========== WISHLIST ==========

    // GET: Account/Wishlist
    [Authorize]
    public async Task<IActionResult> Wishlist()
    {
        var userEmail = User.Identity?.Name;  // ← Dùng UserEmail

        var wishlistItems = await _context.WishlistItems
            .Include(w => w.Product)
            .Where(w => w.UserEmail == userEmail)  // ← Dùng UserEmail
            .OrderByDescending(w => w.AddedDate)
            .ToListAsync();

        return View(wishlistItems);
    }

    // POST: Account/AddToWishlist
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> AddToWishlist(int productId)
    {
        var userEmail = User.Identity?.Name;

        if (string.IsNullOrEmpty(userEmail))
        {
            return Json(new { success = false, message = "Vui lòng đăng nhập để thêm vào yêu thích" });
        }

        // Kiểm tra sản phẩm đã có trong wishlist chưa
        var existingItem = await _context.WishlistItems
            .FirstOrDefaultAsync(w => w.UserEmail == userEmail && w.ProductId == productId);

        if (existingItem != null)
        {
            return Json(new { success = false, message = "Sản phẩm đã có trong danh sách yêu thích" });
        }

        var wishlistItem = new WishlistItem
        {
            UserEmail = userEmail,  // ← Dùng UserEmail
            ProductId = productId,
            AddedDate = DateTime.Now
        };

        _context.WishlistItems.Add(wishlistItem);
        await _context.SaveChangesAsync();

        var wishlistCount = await _context.WishlistItems.CountAsync(w => w.UserEmail == userEmail);

        return Json(new { success = true, message = "Đã thêm vào danh sách yêu thích", count = wishlistCount });
    }

    // POST: Account/RemoveFromWishlist
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> RemoveFromWishlist(int id)
    {
        var userEmail = User.Identity?.Name;
        var item = await _context.WishlistItems
            .FirstOrDefaultAsync(w => w.Id == id && w.UserEmail == userEmail);

        if (item == null)
            return Json(new { success = false, message = "Không tìm thấy sản phẩm" });

        _context.WishlistItems.Remove(item);
        await _context.SaveChangesAsync();

        var wishlistCount = await _context.WishlistItems.CountAsync(w => w.UserEmail == userEmail);

        return Json(new { success = true, message = "Đã xóa khỏi danh sách yêu thích", count = wishlistCount });
    }

    // GET: Account/GetWishlistCount
    [HttpGet]
    [Authorize]
    public async Task<IActionResult> GetWishlistCount()
    {
        var userEmail = User.Identity?.Name;

        if (string.IsNullOrEmpty(userEmail))
        {
            return Json(new { count = 0 });
        }

        var count = await _context.WishlistItems.CountAsync(w => w.UserEmail == userEmail);
        return Json(new { count = count });
    }

    private IActionResult RedirectToLocal(string? returnUrl)
    {
        if (Url.IsLocalUrl(returnUrl))
            return Redirect(returnUrl);
        return RedirectToAction("Index", "Home");
    }
}