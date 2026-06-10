using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using GearDTK.Data;
using GearDTK.Repositories;

namespace GearDTK.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class DashboardController : Controller
{
    private readonly IProductRepository _productRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly UserManager<ApplicationUser> _userManager;  // ← THÊM DÒNG NÀY

    public DashboardController(
        IProductRepository productRepository,
        ICategoryRepository categoryRepository,
        UserManager<ApplicationUser> userManager)  // ← THÊM VÀO CONSTRUCTOR
    {
        _productRepository = productRepository;
        _categoryRepository = categoryRepository;
        _userManager = userManager;  // ← THÊM DÒNG NÀY
    }

    public async Task<IActionResult> Index()
    {
        var productCount = await _productRepository.CountAsync();
        var categoryCount = await _categoryRepository.CountAsync();
        var userCount = _userManager.Users.Count();  // ← GIỜ ĐÃ CÓ _userManager

        // Thống kê theo role
        var adminCount = (await _userManager.GetUsersInRoleAsync("Admin")).Count;
        var employeeCount = (await _userManager.GetUsersInRoleAsync("Employee")).Count;
        var supplierCount = (await _userManager.GetUsersInRoleAsync("Supplier")).Count;
        var customerCount = (await _userManager.GetUsersInRoleAsync("Customer")).Count;

        // Sản phẩm nổi bật
        var featuredProducts = await _productRepository.GetFeaturedProductsAsync(5);

        ViewBag.ProductCount = productCount;
        ViewBag.CategoryCount = categoryCount;
        ViewBag.UserCount = userCount;
        ViewBag.AdminCount = adminCount;
        ViewBag.EmployeeCount = employeeCount;
        ViewBag.SupplierCount = supplierCount;
        ViewBag.CustomerCount = customerCount;
        ViewBag.FeaturedProducts = featuredProducts;

        return View();
    }
}