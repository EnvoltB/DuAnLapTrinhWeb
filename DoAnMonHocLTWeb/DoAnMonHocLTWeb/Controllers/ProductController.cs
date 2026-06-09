using GearDTK.Models;
using GearDTK.Repositories;  // ← THÊM DÒNG NÀY
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GearDTK.Controllers;

public class ProductsController : Controller
{
    private readonly IProductRepository _productRepository;
    private readonly ICategoryRepository _categoryRepository;

    // Inject Repository thay vì DbContext
    public ProductsController(IProductRepository productRepository, ICategoryRepository categoryRepository)
    {
        _productRepository = productRepository;
        _categoryRepository = categoryRepository;
    }

    // GET: Products/Index
    public async Task<IActionResult> Index(string searchString, string sortOrder, int? pageNumber)
    {
        ViewData["CurrentFilter"] = searchString;
        ViewData["CurrentSort"] = sortOrder;
        ViewData["NameSortParm"] = string.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
        ViewData["PriceSortParm"] = sortOrder == "price" ? "price_desc" : "price";

        int pageSize = 10;
        int pageIndex = pageNumber ?? 1;

        var (items, totalCount) = await _productRepository.GetFilteredPagedAsync(
            pageIndex, pageSize, searchString, sortOrder);

        var paginatedProducts = new PaginatedList<Product>(items.ToList(), totalCount, pageIndex, pageSize);

        return View(paginatedProducts);
    }

    // GET: Products/Details/{slug}
    [HttpGet("/Products/Details/{slug}")]
    public async Task<IActionResult> Details(string slug)
    {
        if (string.IsNullOrEmpty(slug))
        {
            return NotFound();
        }

        var product = await _productRepository.GetBySlugWithCategoryAsync(slug);

        if (product == null)
        {
            return NotFound();
        }

        var relatedProducts = await _productRepository.GetRelatedProductsAsync(product.Id, product.CategoryId);

        ViewBag.RelatedProducts = relatedProducts;

        return View(product);
    }

    // GET: Products/Category/{slug}
    [HttpGet("/Products/Category/{slug}")]
    public async Task<IActionResult> Category(string slug, string sortOrder, int? pageNumber)
    {
        if (string.IsNullOrEmpty(slug))
        {
            return NotFound();
        }

        var category = await _categoryRepository.GetBySlugAsync(slug);

        if (category == null)
        {
            return NotFound();
        }

        ViewData["CategoryName"] = category.Name;
        ViewData["CurrentSort"] = sortOrder;
        ViewData["NameSortParm"] = string.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
        ViewData["PriceSortParm"] = sortOrder == "price" ? "price_desc" : "price";

        int pageSize = 12;
        int pageIndex = pageNumber ?? 1;

        var (items, totalCount) = await _productRepository.GetFilteredPagedAsync(
            pageIndex, pageSize, null, sortOrder, category.Id);

        var paginatedProducts = new PaginatedList<Product>(items.ToList(), totalCount, pageIndex, pageSize);

        ViewBag.Category = category;

        return View(paginatedProducts);
    }
}

// Helper class cho phân trang (giữ nguyên)
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