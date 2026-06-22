using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GearDTK.Data;
using GearDTK.Repositories;

namespace GearDTK.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class DashboardController : Controller
{
    private readonly IProductRepository _productRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ApplicationDbContext _context;

    public DashboardController(
        IProductRepository productRepository,
        ICategoryRepository categoryRepository,
        UserManager<ApplicationUser> userManager,
        ApplicationDbContext context)
    {
        _productRepository = productRepository;
        _categoryRepository = categoryRepository;
        _userManager = userManager;
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        // ========== LẤY SỐ LIỆU THỰC TẾ ==========
        var productCount = await _productRepository.CountAsync();
        var categoryCount = await _categoryRepository.CountAsync();
        var userCount = _userManager.Users.Count();
        var BannerCount = await _context.Banners.CountAsync(b => b.IsActive);  // ← THÊM DÒNG NÀY

        // ========== THỐNG KÊ ĐƠN HÀNG ==========
        var totalOrders = await _context.Orders.CountAsync();
        var pendingOrders = await _context.Orders.CountAsync(o => o.OrderStatus == "Pending");
        var processingOrders = await _context.Orders.CountAsync(o => o.OrderStatus == "Processing");
        var shippedOrders = await _context.Orders.CountAsync(o => o.OrderStatus == "Shipped");
        var deliveredOrders = await _context.Orders.CountAsync(o => o.OrderStatus == "Delivered");
        var cancelledOrders = await _context.Orders.CountAsync(o => o.OrderStatus == "Cancelled");

        // ========== THỐNG KÊ NGƯỜI DÙNG ==========
        var adminCount = (await _userManager.GetUsersInRoleAsync("Admin")).Count;
        var employeeCount = (await _userManager.GetUsersInRoleAsync("Employee")).Count;
        var supplierCount = (await _userManager.GetUsersInRoleAsync("Supplier")).Count;
        var customerCount = (await _userManager.GetUsersInRoleAsync("Customer")).Count;

        // ========== SẢN PHẨM NỔI BẬT ==========
        var featuredProducts = await _productRepository.GetFeaturedProductsAsync(5);

        // ========== GÁN VÀO ViewBag ==========
        ViewBag.ProductCount = productCount;
        ViewBag.CategoryCount = categoryCount;
        ViewBag.UserCount = userCount;
        ViewBag.BannerCount = BannerCount;  // ← THÊM DÒNG NÀY

        // Đơn hàng
        ViewBag.TotalOrders = totalOrders;
        ViewBag.PendingOrders = pendingOrders;
        ViewBag.ProcessingOrders = processingOrders;
        ViewBag.ShippedOrders = shippedOrders;
        ViewBag.DeliveredOrders = deliveredOrders;
        ViewBag.CancelledOrders = cancelledOrders;

        // Người dùng theo role
        ViewBag.AdminCount = adminCount;
        ViewBag.EmployeeCount = employeeCount;
        ViewBag.SupplierCount = supplierCount;
        ViewBag.CustomerCount = customerCount;

        ViewBag.FeaturedProducts = featuredProducts;

        return View();
    }
}