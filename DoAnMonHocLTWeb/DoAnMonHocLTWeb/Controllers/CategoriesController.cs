using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GearDTK.Data;
using GearDTK.Models;

namespace GearDTK.Controllers;

public class CategoriesController : Controller
{
    private readonly ApplicationDbContext _context;

    public CategoriesController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: Categories/Index
    // Hiển thị tất cả danh mục
    public async Task<IActionResult> Index()
    {
        var categories = await _context.Categories
            .Include(c => c.Products)
            .OrderBy(c => c.DisplayOrder)
            .ToListAsync();

        return View(categories);
    }

    // GET: Categories/Details/{slug}
    // Xem chi tiết danh mục và sản phẩm trong danh mục
    [HttpGet("/categories/{slug}")]
    public async Task<IActionResult> Details(string slug, string sortOrder, int? pageNumber)
    {
        if (string.IsNullOrEmpty(slug))
        {
            return NotFound();
        }

        var category = await _context.Categories
            .FirstOrDefaultAsync(c => c.Slug == slug);

        if (category == null)
        {
            return NotFound();
        }

        ViewData["CategoryName"] = category.Name;
        ViewData["CategoryDescription"] = category.Description;
        ViewData["CurrentSort"] = sortOrder;
        ViewData["NameSortParm"] = string.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
        ViewData["PriceSortParm"] = sortOrder == "price" ? "price_desc" : "price";

        var products = _context.Products
            .Include(p => p.Category)
            .Where(p => p.CategoryId == category.Id)
            .AsQueryable();

        // Sắp xếp
        products = sortOrder switch
        {
            "name_desc" => products.OrderByDescending(p => p.Name),
            "price" => products.OrderBy(p => p.Price),
            "price_desc" => products.OrderByDescending(p => p.Price),
            _ => products.OrderBy(p => p.Name)
        };

        // Phân trang
        int pageSize = 12;
        var paginatedProducts = await PaginatedList<Product>.CreateAsync(products, pageNumber ?? 1, pageSize);

        ViewBag.Category = category;

        return View(paginatedProducts);
    }
}