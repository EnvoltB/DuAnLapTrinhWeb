using GearDTK.Data;
using GearDTK.Models;
using GearDTK.Repositories;  // ← THÊM DÒNG NÀY
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GearDTK.Controllers;

public class ProductsController : Controller
{
    private readonly IProductRepository _productRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ApplicationDbContext _context;

    // Inject Repository thay vì DbContext
    public ProductsController(
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

    [HttpGet("/Products/Details/{slug}")]
    public async Task<IActionResult> Details(string slug)
    {
        if (string.IsNullOrEmpty(slug))
        {
            return NotFound();
        }

        var product = await _productRepository
            .GetBySlugWithReviewsAsync(slug);

        if (product == null)
        {
            return NotFound();
        }

        var reviews = product.Reviews.ToList();

        ViewBag.TotalReviews = reviews.Count;

        ViewBag.AverageRating =
            reviews.Any()
                ? reviews.Average(x => x.Rating)
                : 0;

        ViewBag.FiveStar = reviews.Count(x => x.Rating == 5);
        ViewBag.FourStar = reviews.Count(x => x.Rating == 4);
        ViewBag.ThreeStar = reviews.Count(x => x.Rating == 3);
        ViewBag.TwoStar = reviews.Count(x => x.Rating == 2);
        ViewBag.OneStar = reviews.Count(x => x.Rating == 1);

        ViewBag.RelatedProducts =
            await _productRepository.GetRelatedProductsAsync(
                product.Id,
                product.CategoryId);

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
    public async Task<IActionResult> Search(string keyword, string sortOrder, int? pageNumber)
    {
        ViewData["CurrentKeyword"] = keyword;
        ViewData["CurrentSort"] = sortOrder;
        ViewData["NameSortParm"] = string.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
        ViewData["PriceSortParm"] = sortOrder == "price" ? "price_desc" : "price";

        int pageSize = 12;
        int pageIndex = pageNumber ?? 1;

        var (items, totalCount) = await _productRepository.GetFilteredPagedAsync(
            pageIndex, pageSize, keyword, sortOrder);

        var paginatedProducts = new PaginatedList<Product>(items.ToList(), totalCount, pageIndex, pageSize);

        ViewBag.Keyword = keyword;
        ViewBag.TotalCount = totalCount;

        return View(paginatedProducts);
    }
    [Authorize]
    [HttpPost]
    public async Task<IActionResult> AddReview([FromBody] ReviewVM model)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var userId = _userManager.GetUserId(User);

        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        var existed = await _context.ProductReviews
            .FirstOrDefaultAsync(x =>
                x.ProductId == model.ProductId &&
                x.UserId == userId);

        if (existed != null)
        {
            existed.Rating = model.Rating;
            existed.Comment = model.Comment;
            existed.CreatedAt = DateTime.Now;
        }
        else
        {
            _context.ProductReviews.Add(new ProductReview
            {
                ProductId = model.ProductId,
                UserId = userId,
                Rating = model.Rating,
                Comment = model.Comment,
                CreatedAt = DateTime.Now
            });
        }

        await _context.SaveChangesAsync();

        return Json(new
        {
            success = true,
            message = "Đánh giá thành công"
        });
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

