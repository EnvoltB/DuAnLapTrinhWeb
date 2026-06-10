using GearDTK.Models;

namespace GearDTK.ViewModels;

public class OrderViewModel
{
    public Order? Order { get; set; }
    public List<CartItem>? CartItems { get; set; }
    public string? OrderNote { get; set; }
    public string? CustomerEmail { get; set; }
    public string? CustomerName { get; set; }
    public string? CustomerPhone { get; set; }
    public string? CustomerAddress { get; set; }
    public string? City { get; set; }
    public string? Country { get; set; }
    public string? ShippingMethod { get; set; }
}