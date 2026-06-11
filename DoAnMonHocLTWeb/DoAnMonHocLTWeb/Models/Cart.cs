using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GearDTK.Models;

public class Cart
{
    [Key]
    public int Id { get; set; }

    public string UserId { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // Navigation property
    public virtual ICollection<CartItem>? CartItems { get; set; }
}

public class CartItem
{
    [Key]
    public int Id { get; set; }

    public int CartId { get; set; }
    public int ProductId { get; set; }

    public string ProductName { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Quantity { get; set; }
    public int StockQuantity { get; set; }

    public decimal Subtotal => Price * Quantity;

    [ForeignKey("CartId")]
    public virtual Cart? Cart { get; set; }

    [ForeignKey("ProductId")]
    public virtual Product? Product { get; set; }
}