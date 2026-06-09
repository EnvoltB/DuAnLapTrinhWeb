using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GearDTK.Repositories;

namespace GearDTK.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]  // Chỉ Admin mới được vào
public class DashboardController : Controller
{
    private readonly IProductRepository _productRepository;
    private readonly ICategoryRepository _categoryRepository;

    public DashboardController(IProductRepository productRepository, ICategoryRepository categoryRepository)
    {
        _productRepository = productRepository;
        _categoryRepository = categoryRepository;
    }

    public async Task<IActionResult> Index()
    {
        // Thống kê số lượng
        var productCount = await _productRepository.CountAsync();
        var categoryCount = await _categoryRepository.CountAsync();

        ViewBag.ProductCount = productCount;
        ViewBag.CategoryCount = categoryCount;

        return View();
    }
}