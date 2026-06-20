using Microsoft.EntityFrameworkCore;
using GearDTK.Data;
using GearDTK.Models;

namespace GearDTK.Repositories;

public class ProductRepository : IProductRepository
{
    private readonly ApplicationDbContext _context;
    private readonly DbSet<Product> _dbSet;

    public ProductRepository(ApplicationDbContext context)
    {
        _context = context;
        _dbSet = context.Products;
    }

    public async Task<IEnumerable<Product>> GetAllAsync()
    {
        return await _dbSet.ToListAsync();
    }

    public async Task<IEnumerable<Product>> GetAllWithCategoryAsync()
    {
        return await _dbSet.Include(p => p.Category).ToListAsync();
    }

    public async Task<Product?> GetByIdAsync(int id)
    {
        return await _dbSet.FindAsync(id);
    }

    public async Task<Product?> GetByIdWithCategoryAsync(int id)
    {
        return await _dbSet.Include(p => p.Category).FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<Product?> GetBySlugAsync(string slug)
    {
        return await _dbSet.FirstOrDefaultAsync(p => p.Slug == slug);
    }

    public async Task<Product?> GetBySlugWithCategoryAsync(string slug)
    {
        return await _dbSet.Include(p => p.Category).FirstOrDefaultAsync(p => p.Slug == slug);
    }
    public async Task<Product?> GetBySlugWithReviewsAsync(string slug)
    {
        return await _context.Products
            .Include(p => p.Category)
            .Include(p => p.Reviews)
            .ThenInclude(r => r.User)
            .FirstOrDefaultAsync(p => p.Slug == slug);
    }
    public async Task<IEnumerable<Product>> GetByCategoryAsync(int categoryId)
    {
        return await _dbSet.Where(p => p.CategoryId == categoryId).ToListAsync();
    }

    public async Task<IEnumerable<Product>> GetByCategorySlugAsync(string categorySlug)
    {
        return await _dbSet
            .Include(p => p.Category)
            .Where(p => p.Category != null && p.Category.Slug == categorySlug)
            .ToListAsync();
    }

    public async Task<IEnumerable<Product>> GetFeaturedProductsAsync(int count = 8)
    {
        return await _dbSet
            .Include(p => p.Category)
            .Where(p => p.IsFeatured && p.StockQuantity > 0)
            .Take(count)
            .ToListAsync();
    }

    public async Task<IEnumerable<Product>> GetNewProductsAsync(int count = 8)
    {
        return await _dbSet
            .Include(p => p.Category)
            .Where(p => p.IsNew && p.StockQuantity > 0)
            .OrderByDescending(p => p.CreatedAt)
            .Take(count)
            .ToListAsync();
    }

    public async Task<IEnumerable<Product>> GetBestSellerProductsAsync(int count = 8)
    {
        return await _dbSet
            .Include(p => p.Category)
            .Where(p => p.IsBestSeller && p.StockQuantity > 0)
            .Take(count)
            .ToListAsync();
    }

    public async Task<IEnumerable<Product>> GetRelatedProductsAsync(int productId, int categoryId, int count = 4)
    {
        return await _dbSet
            .Include(p => p.Category)
            .Where(p => p.CategoryId == categoryId && p.Id != productId && p.StockQuantity > 0)
            .Take(count)
            .ToListAsync();
    }

    public async Task<IEnumerable<Product>> SearchAsync(string searchTerm)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
            return await _dbSet.Include(p => p.Category).ToListAsync();

        return await _dbSet
            .Include(p => p.Category)
            .Where(p => p.Name.Contains(searchTerm) ||
                        p.ShortDescription.Contains(searchTerm) ||
                        (p.Category != null && p.Category.Name.Contains(searchTerm)))
            .ToListAsync();
    }

    public async Task<(IEnumerable<Product> Items, int TotalCount)> GetFilteredPagedAsync(
        int pageIndex, int pageSize, string? searchString = null, string? sortOrder = null, int? categoryId = null)
    {
        IQueryable<Product> query = _dbSet.Include(p => p.Category);

        // Filter by search
        if (!string.IsNullOrEmpty(searchString))
        {
            query = query.Where(p => p.Name.Contains(searchString) ||
                                     p.ShortDescription.Contains(searchString) ||
                                     (p.Category != null && p.Category.Name.Contains(searchString)));
        }

        // Filter by category
        if (categoryId.HasValue && categoryId > 0)
        {
            query = query.Where(p => p.CategoryId == categoryId);
        }

        // Get total count
        var totalCount = await query.CountAsync();

        // Apply sorting
        query = sortOrder switch
        {
            "name_desc" => query.OrderByDescending(p => p.Name),
            "price" => query.OrderBy(p => p.Price),
            "price_desc" => query.OrderByDescending(p => p.Price),
            "newest" => query.OrderByDescending(p => p.CreatedAt),
            _ => query.OrderBy(p => p.Name)
        };

        // Apply pagination
        var items = await query.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToListAsync();

        return (items, totalCount);
    }

    public async Task<int> CountAsync()
    {
        return await _dbSet.CountAsync();
    }

    public async Task<int> CountByCategoryAsync(int categoryId)
    {
        return await _dbSet.CountAsync(p => p.CategoryId == categoryId);
    }

    public async Task<Product> AddAsync(Product product)
    {
        await _dbSet.AddAsync(product);
        return product;
    }

    public async Task UpdateAsync(Product product)
    {
        _dbSet.Update(product);
        await Task.CompletedTask;
    }

    public async Task DeleteAsync(Product product)
    {
        _dbSet.Remove(product);
        await Task.CompletedTask;
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}