using GearDTK.Models;

namespace GearDTK.Repositories;

public interface ICartRepository
{
    List<CartItem> GetCartItems();
    void AddToCart(int productId, string productName, string slug, decimal price, string imageUrl, int stockQuantity, int quantity = 1);
    void UpdateQuantity(int productId, int quantity);
    void RemoveFromCart(int productId);
    void ClearCart();
    int GetCartCount();
    decimal GetCartTotal();
}