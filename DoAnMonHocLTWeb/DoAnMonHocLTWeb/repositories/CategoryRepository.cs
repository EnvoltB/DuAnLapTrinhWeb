using Microsoft.EntityFrameworkCore;
using GearDTK.Data;
using GearDTK.Models;

namespace GearDTK.Repositories;

public class CategoryRepository : ICategoryRepository
{
    private readonly ApplicationDbContext _context;
    private readonly DbSet<Category> _dbSet;

    public CategoryRepository(ApplicationDbContext context)
    {
        _context = context;
        _dbSet = context.Categories;
    }

    public async Task<IEnumerable<Category>> GetAllAsync()
    {
        return await _dbSet.OrderBy(c => c.DisplayOrder).ToListAsync();
    }

    public async Task<Category?> GetByIdAsync(int id)
    {
        return await _dbSet.FindAsync(id);
    }

    // ========== THÊM METHOD NÀY ==========
    public async Task<Category?> GetByIdWithProductsAsync(int id)
    {
        return await _dbSet
            .Include(c => c.Products)
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<Category?> GetBySlugAsync(string slug)
    {
        return await _dbSet.FirstOrDefaultAsync(c => c.Slug == slug);
    }

    public async Task<Category?> GetBySlugWithProductsAsync(string slug)
    {
        return await _dbSet
            .Include(c => c.Products)
            .FirstOrDefaultAsync(c => c.Slug == slug);
    }

    public async Task<IEnumerable<Category>> GetHomepageCategoriesAsync()
    {
        return await _dbSet
            .Where(c => c.ShowOnHomepage)
            .OrderBy(c => c.DisplayOrder)
            .ToListAsync();
    }

    public async Task<int> CountAsync()
    {
        return await _dbSet.CountAsync();
    }

    public async Task<Category> AddAsync(Category category)
    {
        await _dbSet.AddAsync(category);
        return category;
    }

    public async Task UpdateAsync(Category category)
    {
        _dbSet.Update(category);
        await Task.CompletedTask;
    }

    public async Task DeleteAsync(Category category)
    {
        _dbSet.Remove(category);
        await Task.CompletedTask;
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}