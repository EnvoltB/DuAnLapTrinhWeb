using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GearDTK.Data;
using GearDTK.Models;

namespace GearDTK.Controllers;

public class ProductsController : Controller
{
    private readonly ApplicationDbContext _context;

    public ProductsController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: Products/Index
    // Hiển thị tất cả sản phẩm
    public async Task<IActionResult> Index(string searchString, string sortOrder, int? pageNumber)
    {
        ViewData["CurrentFilter"] = searchString;
        ViewData["CurrentSort"] = sortOrder;
        ViewData["NameSortParm"] = string.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
        ViewData["PriceSortParm"] = sortOrder == "price" ? "price_desc" : "price";

        var products = _context.Products
            .Include(p => p.Category)
            .AsQueryable();

        // Tìm kiếm
        if (!string.IsNullOrEmpty(searchString))
        {
            products = products.Where(p => p.Name.Contains(searchString) ||
                                           p.ShortDescription.Contains(searchString) ||
                                           (p.Category != null && p.Category.Name.Contains(searchString)));
        }

        // Sắp xếp
        products = sortOrder switch
        {
            "name_desc" => products.OrderByDescending(p => p.Name),
            "price" => products.OrderBy(p => p.Price),
            "price_desc" => products.OrderByDescending(p => p.Price),
            _ => products.OrderBy(p => p.Name)
        };

        // Phân trang (10 sản phẩm mỗi trang)
        int pageSize = 10;
        var paginatedProducts = await PaginatedList<Product>.CreateAsync(products, pageNumber ?? 1, pageSize);

        return View(paginatedProducts);
    }

    // GET: Products/Details/{slug}
    // Xem chi tiết sản phẩm
    [HttpGet("/Products/Details/{slug}")]
    public async Task<IActionResult> Details(string slug)
    {
        if (string.IsNullOrEmpty(slug))
        {
            return NotFound();
        }

        var product = await _context.Products
            .Include(p => p.Category)
            .FirstOrDefaultAsync(p => p.Slug == slug);

        if (product == null)
        {
            return NotFound();
        }

        // Lấy sản phẩm liên quan (cùng danh mục)
        var relatedProducts = await _context.Products
            .Include(p => p.Category)
            .Where(p => p.CategoryId == product.CategoryId && p.Id != product.Id)
            .Take(4)
            .ToListAsync();

        ViewBag.RelatedProducts = relatedProducts;

        return View(product);
    }

    // GET: Products/Category/{slug}
    // Hiển thị sản phẩm theo danh mục
    public async Task<IActionResult> Category(string slug, string sortOrder, int? pageNumber)
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

// Helper class cho phân trang
public class PaginatedList<T> : List<T>
{
    public int PageIndex { get; private set; }
    public int TotalPages { get; private set; }

    public PaginatedList(List<T> items, int count, int pageIndex, int pageSize)
    {
        PageIndex = pageIndex;
        TotalPages = (int)Math.Ceiling(count / (double)pageSize);

        AddRange(items);
    }

    public bool HasPreviousPage => PageIndex > 1;
    public bool HasNextPage => PageIndex < TotalPages;

    public static async Task<PaginatedList<T>> CreateAsync(IQueryable<T> source, int pageIndex, int pageSize)
    {
        var count = await source.CountAsync();
        var items = await source.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToListAsync();
        return new PaginatedList<T>(items, count, pageIndex, pageSize);
    }
}