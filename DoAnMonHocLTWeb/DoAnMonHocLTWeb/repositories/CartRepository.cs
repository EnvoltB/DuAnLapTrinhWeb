using System.Text.Json;
using GearDTK.Models;

namespace GearDTK.Repositories;

public class CartRepository : ICartRepository
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private const string CartSessionKey = "ShoppingCart";

    public CartRepository(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    private ISession Session => _httpContextAccessor.HttpContext!.Session;

    // Lấy danh sách sản phẩm trong giỏ
    public List<CartItem> GetCartItems()
    {
        var cartJson = Session.GetString(CartSessionKey);
        if (string.IsNullOrEmpty(cartJson))
        {
            return new List<CartItem>();
        }
        return JsonSerializer.Deserialize<List<CartItem>>(cartJson) ?? new List<CartItem>();
    }

    // Lưu giỏ hàng vào session
    private void SaveCart(List<CartItem> cart)
    {
        var cartJson = JsonSerializer.Serialize(cart);
        Session.SetString(CartSessionKey, cartJson);
    }

    // Thêm sản phẩm vào giỏ
    public void AddToCart(int productId, string productName, string slug, decimal price, string imageUrl, int stockQuantity, int quantity = 1)
    {
        var cart = GetCartItems();
        var existingItem = cart.FirstOrDefault(x => x.ProductId == productId);

        if (existingItem != null)
        {
            var newQuantity = existingItem.Quantity + quantity;
            existingItem.Quantity = newQuantity > stockQuantity ? stockQuantity : newQuantity;
        }
        else
        {
            var newItem = new CartItem
            {
                ProductId = productId,
                ProductName = productName,
                Slug = slug,
                Price = price,
                Quantity = quantity > stockQuantity ? stockQuantity : quantity,
                ImageUrl = imageUrl,
                StockQuantity = stockQuantity
            };
            cart.Add(newItem);
        }

        SaveCart(cart);
    }

    // Cập nhật số lượng
    public void UpdateQuantity(int productId, int quantity)
    {
        var cart = GetCartItems();
        var item = cart.FirstOrDefault(x => x.ProductId == productId);

        if (item != null)
        {
            if (quantity <= 0)
            {
                cart.Remove(item);
            }
            else
            {
                item.Quantity = quantity > item.StockQuantity ? item.StockQuantity : quantity;
            }
            SaveCart(cart);
        }
    }

    // Xóa sản phẩm khỏi giỏ
    public void RemoveFromCart(int productId)
    {
        var cart = GetCartItems();
        var item = cart.FirstOrDefault(x => x.ProductId == productId);

        if (item != null)
        {
            cart.Remove(item);
            SaveCart(cart);
        }
    }

    // Xóa toàn bộ giỏ hàng
    public void ClearCart()
    {
        Session.Remove(CartSessionKey);
    }

    // Lấy tổng số lượng
    public int GetCartCount()
    {
        var cart = GetCartItems();
        return cart.Sum(x => x.Quantity);
    }

    // Lấy tổng tiền
    public decimal GetCartTotal()
    {
        var cart = GetCartItems();
        return cart.Sum(x => x.Subtotal);
    }
    public void SaveOrderNote(string note)
    {
        Session.SetString("OrderNote", note ?? "");
    }

    public string GetOrderNote()
    {
        return Session.GetString("OrderNote") ?? "";
    }

}