using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using GearDTK.Data;
using GearDTK.ViewModels;

namespace GearDTK.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class UserController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    public UserController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
    {
        _userManager = userManager;
        _roleManager = roleManager;
    }

    // GET: Admin/User
    public async Task<IActionResult> Index()
    {
        var users = _userManager.Users.ToList();
        var userViewModels = new List<UserViewModel>();

        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);
            userViewModels.Add(new UserViewModel
            {
                Id = user.Id,
                Email = user.Email,
                FullName = user.FullName,
                Phone = user.Phone,
                Address = user.Address,
                Role = roles.FirstOrDefault() ?? "Customer",
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt,
                LastLoginAt = user.LastLoginAt
            });
        }

        return View(userViewModels);
    }

    // GET: Admin/User/Create
    public async Task<IActionResult> Create()
    {
        var roles = _roleManager.Roles.Select(r => r.Name).ToList();
        ViewBag.Roles = new SelectList(roles);
        return View();
    }

    // POST: Admin/User/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateUserViewModel model)
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
                // Gán role cho user
                await _userManager.AddToRoleAsync(user, model.SelectedRole);
                TempData["Success"] = $"Tạo tài khoản {model.Email} thành công với vai trò {model.SelectedRole}";
                return RedirectToAction(nameof(Index));
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }

        var roles = _roleManager.Roles.Select(r => r.Name).ToList();
        ViewBag.Roles = new SelectList(roles, model.SelectedRole);
        return View(model);
    }

    // GET: Admin/User/Edit/{id}
    public async Task<IActionResult> Edit(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
        {
            return NotFound();
        }

        var roles = await _userManager.GetRolesAsync(user);
        var allRoles = _roleManager.Roles.Select(r => r.Name).ToList();

        var model = new EditUserViewModel
        {
            Id = user.Id,
            Email = user.Email,
            FullName = user.FullName,
            Phone = user.Phone,
            Address = user.Address,
            SelectedRole = roles.FirstOrDefault() ?? "Customer",
            IsActive = user.IsActive
        };

        ViewBag.Roles = new SelectList(allRoles, model.SelectedRole);
        return View(model);
    }

    // POST: Admin/User/Edit/{id}
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(string id, EditUserViewModel model)
    {
        if (id != model.Id)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            // Cập nhật thông tin
            user.FullName = model.FullName;
            user.Phone = model.Phone;
            user.Address = model.Address;
            user.IsActive = model.IsActive;

            var updateResult = await _userManager.UpdateAsync(user);

            if (updateResult.Succeeded)
            {
                // Cập nhật role
                var currentRoles = await _userManager.GetRolesAsync(user);
                await _userManager.RemoveFromRolesAsync(user, currentRoles);
                await _userManager.AddToRoleAsync(user, model.SelectedRole);

                TempData["Success"] = $"Cập nhật tài khoản {user.Email} thành công";
                return RedirectToAction(nameof(Index));
            }

            foreach (var error in updateResult.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }

        var allRoles = _roleManager.Roles.Select(r => r.Name).ToList();
        ViewBag.Roles = new SelectList(allRoles, model.SelectedRole);
        return View(model);
    }

    // POST: Admin/User/Delete/{id}
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user != null && user.Email != "admin@geardtk.com") // Không cho xóa admin mặc định
        {
            var result = await _userManager.DeleteAsync(user);
            if (result.Succeeded)
            {
                TempData["Success"] = "Xóa tài khoản thành công";
            }
            else
            {
                TempData["Error"] = "Có lỗi xảy ra khi xóa tài khoản";
            }
        }
        else
        {
            TempData["Error"] = "Không thể xóa tài khoản Admin mặc định";
        }

        return RedirectToAction(nameof(Index));
    }

    // POST: Admin/User/ToggleStatus/{id}
    [HttpPost]
    public async Task<IActionResult> ToggleStatus(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user != null)
        {
            user.IsActive = !user.IsActive;
            await _userManager.UpdateAsync(user);
            TempData["Success"] = $"Đã {(user.IsActive ? "kích hoạt" : "vô hiệu hóa")} tài khoản {user.Email}";
        }
        return RedirectToAction(nameof(Index));
    }
}