using Microsoft.EntityFrameworkCore;
using GearDTK.Data;
using GearDTK.Models;


namespace GearDTK.Repositories;

public class CartRepository : ICartRepository
{
    private readonly ApplicationDbContext _context;

    public CartRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    private async Task<Cart> GetOrCreateCartAsync(string userId)
    {
        var cart = await _context.Carts
            .Include(c => c.CartItems)
            .FirstOrDefaultAsync(c => c.UserId == userId);

        if (cart == null)
        {
            cart = new Cart
            {
                UserId = userId,
                CreatedAt = DateTime.UtcNow
            };
            _context.Carts.Add(cart);
            await _context.SaveChangesAsync();
        }

        return cart;
    }

    public async Task<Cart?> GetCartByUserIdAsync(string userId)
    {
        return await _context.Carts
            .Include(c => c.CartItems)
            .FirstOrDefaultAsync(c => c.UserId == userId);
    }

    public async Task<List<CartItem>> GetCartItemsAsync(string userId)
    {
        var cart = await GetOrCreateCartAsync(userId);
        return cart.CartItems?.ToList() ?? new List<CartItem>();
    }

    public async Task AddToCartAsync(string userId, int productId, int quantity = 1)
    {
        var product = await _context.Products.FindAsync(productId);
        if (product == null) return;

        var cart = await GetOrCreateCartAsync(userId);

        var existingItem = cart.CartItems?.FirstOrDefault(x => x.ProductId == productId);

        if (existingItem != null)
        {
            var newQuantity = existingItem.Quantity + quantity;
            existingItem.Quantity = newQuantity > product.StockQuantity ? product.StockQuantity : newQuantity;
        }
        else
        {
            cart.CartItems ??= new List<CartItem>();
            cart.CartItems.Add(new CartItem
            {
                ProductId = productId,
                ProductName = product.Name,
                Slug = product.Slug,
                Price = product.Price,
                Quantity = quantity > product.StockQuantity ? product.StockQuantity : quantity,
                ImageUrl = product.MainImageUrl,
                StockQuantity = product.StockQuantity
            });
        }

        cart.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
    }

    public async Task UpdateQuantityAsync(string userId, int productId, int quantity)
    {
        var cart = await GetCartByUserIdAsync(userId);
        if (cart == null || cart.CartItems == null) return;

        var item = cart.CartItems.FirstOrDefault(x => x.ProductId == productId);
        if (item == null) return;

        if (quantity <= 0)
        {
            cart.CartItems.Remove(item);
        }
        else
        {
            item.Quantity = quantity > item.StockQuantity ? item.StockQuantity : quantity;
        }

        await _context.SaveChangesAsync();
    }

    public async Task RemoveFromCartAsync(string userId, int productId)
    {
        var cart = await GetCartByUserIdAsync(userId);
        if (cart == null || cart.CartItems == null) return;

        var item = cart.CartItems.FirstOrDefault(x => x.ProductId == productId);
        if (item != null)
        {
            cart.CartItems.Remove(item);
            await _context.SaveChangesAsync();
        }
    }

    public async Task ClearCartAsync(string userId)
    {
        var cart = await GetCartByUserIdAsync(userId);
        if (cart == null || cart.CartItems == null) return;

        cart.CartItems.Clear();
        await _context.SaveChangesAsync();
    }

    public async Task<int> GetCartCountAsync(string userId)
    {
        var cart = await GetCartByUserIdAsync(userId);
        return cart?.CartItems?.Sum(x => x.Quantity) ?? 0;
    }

    public async Task<decimal> GetCartTotalAsync(string userId)
    {
        var cart = await GetCartByUserIdAsync(userId);
        return cart?.CartItems?.Sum(x => x.Subtotal) ?? 0;
    }
}