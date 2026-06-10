using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GearDTK.Models;

public class Order
{
    [Key]
    public int Id { get; set; }

    // Thông tin khách hàng
    public string? CustomerName { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? Country { get; set; }

    // Thông tin đơn hàng
    public string? OrderCode { get; set; }
    public decimal Subtotal { get; set; }
    public decimal ShippingFee { get; set; }
    public decimal Discount { get; set; }
    public decimal Total { get; set; }

    // Thông tin vận chuyển
    public string? ShippingMethod { get; set; }
    public string? OrderNote { get; set; }

    // Trạng thái
    public string? OrderStatus { get; set; }  // Pending, Processing, Shipped, Delivered, Cancelled
    public string? PaymentStatus { get; set; } // Pending, Paid, Failed

    // Thời gian
    public DateTime OrderDate { get; set; } = DateTime.UtcNow;
    public DateTime? PaymentDate { get; set; }

    // Navigation property
    public virtual ICollection<OrderItem>? OrderItems { get; set; }
}