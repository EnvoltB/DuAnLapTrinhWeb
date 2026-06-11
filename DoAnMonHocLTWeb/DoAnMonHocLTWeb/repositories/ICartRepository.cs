
using GearDTK.Models;

namespace GearDTK.Repositories;

public interface ICartRepository
{
    // Lấy giỏ hàng theo UserId
    Task<Cart?> GetCartByUserIdAsync(string userId);
    Task<List<CartItem>> GetCartItemsAsync(string userId);

    // Thêm sản phẩm vào giỏ
    Task AddToCartAsync(string userId, int productId, int quantity = 1);

    // Cập nhật số lượng
    Task UpdateQuantityAsync(string userId, int productId, int quantity);

    // Xóa sản phẩm khỏi giỏ
    Task RemoveFromCartAsync(string userId, int productId);

    // Xóa toàn bộ giỏ hàng
    Task ClearCartAsync(string userId);

    // Lấy tổng số lượng
    Task<int> GetCartCountAsync(string userId);

    // Lấy tổng tiền
    Task<decimal> GetCartTotalAsync(string userId);
}