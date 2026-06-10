using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GearDTK.Data;
using GearDTK.Models;
using GearDTK.Repositories;
using GearDTK.ViewModels;

namespace GearDTK.Controllers;

public class OrderController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly ICartRepository _cartRepository;

    public OrderController(ApplicationDbContext context, ICartRepository cartRepository)
    {
        _context = context;
        _cartRepository = cartRepository;
    }

    // GET: Order/Complete
    public IActionResult Complete()
    {
        var cartItems = _cartRepository.GetCartItems();

        if (!cartItems.Any())
        {
            return RedirectToAction("Index", "Cart");
        }

        var orderNote = HttpContext.Session.GetString("OrderNote") ?? "";
        var total = _cartRepository.GetCartTotal();

        var viewModel = new OrderViewModel
        {
            CartItems = cartItems,
            OrderNote = orderNote,
            Order = new Order
            {
                Subtotal = total,
                ShippingFee = 0,
                Total = total,
                OrderDate = DateTime.Now,
                OrderCode = GenerateOrderCode()
            }
        };

        return View(viewModel);
    }

    // POST: Order/PlaceOrder
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> PlaceOrder(OrderViewModel model)
    {
        // Tạm thời bỏ qua validation
        ModelState.Clear();

        var cartItems = _cartRepository.GetCartItems();

        if (!cartItems.Any())
        {
            TempData["Error"] = "Giỏ hàng trống!";
            return RedirectToAction("Index", "Cart");
        }

        // Tạo đơn hàng mới (cho phép null)
        var order = new Order
        {
            OrderCode = GenerateOrderCode(),
            CustomerName = model.CustomerName ?? "Khách hàng",
            Email = model.CustomerEmail ?? "unknown@email.com",
            Phone = model.CustomerPhone ?? "N/A",
            Address = model.CustomerAddress ?? "N/A",
            City = model.City ?? "N/A",
            Country = model.Country ?? "Vietnam",
            Subtotal = _cartRepository.GetCartTotal(),
            ShippingFee = 0,
            Discount = 0,
            Total = _cartRepository.GetCartTotal(),
            OrderNote = model.OrderNote ?? "",
            OrderStatus = "Pending",
            PaymentStatus = "Pending",
            OrderDate = DateTime.Now
        };

        _context.Orders.Add(order);
        await _context.SaveChangesAsync();

        // Tạo OrderItems
        foreach (var item in cartItems)
        {
            var orderItem = new OrderItem
            {
                OrderId = order.Id,
                ProductId = item.ProductId,
                ProductName = item.ProductName,
                ProductImage = item.ImageUrl,
                Price = item.Price,
                Quantity = item.Quantity
            };
            _context.OrderItems.Add(orderItem);
        }

        await _context.SaveChangesAsync();

        // Xóa giỏ hàng
        _cartRepository.ClearCart();
        HttpContext.Session.Remove("OrderNote");

        TempData["Success"] = "Đặt hàng thành công!";
        return RedirectToAction("ThankYou", new { id = order.Id });
    }

    // GET: Order/ThankYou/{id}
    public async Task<IActionResult> ThankYou(int id)
    {
        var order = await _context.Orders
            .Include(o => o.OrderItems)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (order == null)
        {
            return NotFound();
        }

        return View(order);
    }

    private string GenerateOrderCode()
    {
        return "DTK" + DateTime.Now.ToString("yyyyMMddHHmmss") + new Random().Next(1000, 9999);
    }
}