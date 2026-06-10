using GearDTK.Models;

namespace GearDTK.Repositories;

public interface ICartRepository
{
    // Lấy danh sách sản phẩm trong giỏ
    List<CartItem> GetCartItems();

    void SaveOrderNote(string note);
    string GetOrderNote();

    // Thêm sản phẩm vào giỏ
    void AddToCart(int productId, string productName, string slug, decimal price, string imageUrl, int stockQuantity, int quantity = 1);

    // Cập nhật số lượng
    void UpdateQuantity(int productId, int quantity);

    // Xóa sản phẩm khỏi giỏ
    void RemoveFromCart(int productId);

    // Xóa toàn bộ giỏ hàng
    void ClearCart();

    // Lấy tổng số lượng
    int GetCartCount();

    // Lấy tổng tiền
    decimal GetCartTotal();

}