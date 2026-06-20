using GearDTK.Models;

namespace GearDTK.Repositories;

public interface IProductRepository
{
    // Lấy tất cả sản phẩm
    Task<IEnumerable<Product>> GetAllAsync();
    Task<IEnumerable<Product>> GetAllWithCategoryAsync();

    // Lấy sản phẩm theo ID
    Task<Product?> GetByIdAsync(int id);
    Task<Product?> GetByIdWithCategoryAsync(int id);

    // Lấy sản phẩm theo Slug (URL)
    Task<Product?> GetBySlugAsync(string slug);
    Task<Product?> GetBySlugWithCategoryAsync(string slug);

    // Lấy sản phẩm theo danh mục
    Task<IEnumerable<Product>> GetByCategoryAsync(int categoryId);
    Task<IEnumerable<Product>> GetByCategorySlugAsync(string categorySlug);

    // Lấy sản phẩm nổi bật
    Task<IEnumerable<Product>> GetFeaturedProductsAsync(int count = 8);

    // Lấy sản phẩm mới
    Task<IEnumerable<Product>> GetNewProductsAsync(int count = 8);

    // Lấy sản phẩm bán chạy
    Task<IEnumerable<Product>> GetBestSellerProductsAsync(int count = 8);

    // Lấy sản phẩm liên quan (cùng danh mục)
    Task<IEnumerable<Product>> GetRelatedProductsAsync(int productId, int categoryId, int count = 4);

    // Tìm kiếm sản phẩm
    Task<IEnumerable<Product>> SearchAsync(string searchTerm);

    // Lọc, sắp xếp, phân trang
    Task<(IEnumerable<Product> Items, int TotalCount)> GetFilteredPagedAsync(
        int pageIndex, int pageSize,
        string? searchString = null,
        string? sortOrder = null,
        int? categoryId = null);

    // Đếm số lượng
    Task<int> CountAsync();
    Task<int> CountByCategoryAsync(int categoryId);

    // CRUD cơ bản
    Task<Product> AddAsync(Product product);
    Task UpdateAsync(Product product);
    Task DeleteAsync(Product product);
    Task SaveChangesAsync();
    Task<Product?> GetBySlugWithReviewsAsync(string slug);
}