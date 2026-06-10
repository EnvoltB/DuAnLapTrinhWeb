using GearDTK.Models;

namespace GearDTK.Repositories;

public interface ICategoryRepository
{
    // Lấy tất cả danh mục
    Task<IEnumerable<Category>> GetAllAsync();

    // Lấy danh mục theo ID
    Task<Category?> GetByIdAsync(int id);

    // Lấy danh mục theo ID kèm sản phẩm
    Task<Category?> GetByIdWithProductsAsync(int id);  // ← THÊM DÒNG NÀY

    // Lấy danh mục theo Slug (URL)
    Task<Category?> GetBySlugAsync(string slug);

    // Lấy danh mục kèm theo sản phẩm
    Task<Category?> GetBySlugWithProductsAsync(string slug);

    // Lấy danh mục hiển thị trên trang chủ
    Task<IEnumerable<Category>> GetHomepageCategoriesAsync();

    // Đếm số lượng danh mục
    Task<int> CountAsync();

    // CRUD cơ bản
    Task<Category> AddAsync(Category category);
    Task UpdateAsync(Category category);
    Task DeleteAsync(Category category);
    Task SaveChangesAsync();
}