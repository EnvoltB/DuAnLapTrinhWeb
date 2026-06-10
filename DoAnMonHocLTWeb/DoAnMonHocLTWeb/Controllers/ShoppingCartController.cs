using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GearDTK.Data;
using GearDTK.Models;
using GearDTK.Repositories;
using GearDTK.ViewModels;

namespace GearDTK.Controllers;

public class ShoppingCartController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly ICartRepository _cartRepository;

    public ShoppingCartController(ApplicationDbContext context, ICartRepository cartRepository)
    {
        _context = context;
        _cartRepository = cartRepository;
    }

    // ========== GIỎ HÀNG ==========

    // GET: ShoppingCart/Index
    public IActionResult Index()
    {
        var cartItems = _cartRepository.GetCartItems();
        var total = _cartRepository.GetCartTotal();
        var orderNote = HttpContext.Session.GetString("OrderNote") ?? "";

        ViewBag.Total = total;
        ViewBag.OrderNote = orderNote;
        ViewBag.ItemCount = _cartRepository.GetCartCount();

        return View(cartItems);
    }

    // POST: ShoppingCart/AddToCart
    [HttpPost]
    public async Task<IActionResult> AddToCart(int productId, int quantity = 1)
    {
        var product = await _context.Products.FindAsync(productId);

        if (product == null)
        {
            return Json(new { success = false, message = "Sản phẩm không tồn tại" });
        }

        if (product.StockQuantity < quantity)
        {
            return Json(new { success = false, message = "Số lượng sản phẩm không đủ" });
        }

        _cartRepository.AddToCart(
            product.Id,
            product.Name,
            product.Slug,
            product.Price,
            product.MainImageUrl,
            product.StockQuantity,
            quantity
        );

        var cartCount = _cartRepository.GetCartCount();

        return Json(new { success = true, cartCount = cartCount });
    }

    // POST: ShoppingCart/UpdateQuantity
    [HttpPost]
    public IActionResult UpdateQuantity(int productId, int quantity)
    {
        _cartRepository.UpdateQuantity(productId, quantity);

        var cartItems = _cartRepository.GetCartItems();
        var total = _cartRepository.GetCartTotal();
        var cartCount = _cartRepository.GetCartCount();

        var item = cartItems.FirstOrDefault(x => x.ProductId == productId);

        return Json(new
        {
            success = true,
            subtotal = item?.Subtotal ?? 0,
            total = total,
            cartCount = cartCount
        });
    }

    // POST: ShoppingCart/RemoveFromCart
    [HttpPost]
    public IActionResult RemoveFromCart(int productId)
    {
        _cartRepository.RemoveFromCart(productId);

        var total = _cartRepository.GetCartTotal();
        var cartCount = _cartRepository.GetCartCount();

        return Json(new
        {
            success = true,
            total = total,
            cartCount = cartCount
        });
    }

    // POST: ShoppingCart/SaveOrderNote
    [HttpPost]
    public IActionResult SaveOrderNote(string note)
    {
        HttpContext.Session.SetString("OrderNote", note ?? "");
        return Json(new { success = true, message = "Đã lưu ghi chú" });
    }

    // GET: ShoppingCart/GetCartCount
    [HttpGet]
    public IActionResult GetCartCount()
    {
        var count = _cartRepository.GetCartCount();
        return Json(new { count = count });
    }

    // ========== CHECKOUT ==========

    // GET: ShoppingCart/Checkout
    public IActionResult Checkout()
    {
        var cartItems = _cartRepository.GetCartItems();

        if (!cartItems.Any())
        {
            return RedirectToAction("Index");
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

    // POST: ShoppingCart/PlaceOrder
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> PlaceOrder(OrderViewModel model)
    {
        try
        {
            var cartItems = _cartRepository.GetCartItems();

            if (!cartItems.Any())
            {
                TempData["Error"] = "Giỏ hàng trống!";
                return RedirectToAction("Index");
            }

            // Lấy email từ user đã đăng nhập hoặc từ form
            var userEmail = User.Identity?.Name ?? model.CustomerEmail;
            var userName = User.Identity?.Name ?? model.CustomerName;

            // Tạo đơn hàng mới
            var order = new Order
            {
                OrderCode = GenerateOrderCode(),
                CustomerName = model.CustomerName ?? "Khách hàng",
                Email = userEmail ?? model.CustomerEmail ?? "",
                Phone = model.CustomerPhone ?? "",
                Address = model.CustomerAddress ?? "",
                City = model.City ?? "",
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

            // Tạo OrderItems từ giỏ hàng
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

            // Xóa giỏ hàng sau khi đặt hàng thành công
            _cartRepository.ClearCart();
            HttpContext.Session.Remove("OrderNote");

            // Chuyển hướng đến trang ThankYou và lưu orderId vào session
            HttpContext.Session.SetInt32("LastOrderId", order.Id);
            TempData["Success"] = "Đặt hàng thành công! Mã đơn hàng: " + order.OrderCode;

            return RedirectToAction("ThankYou", new { id = order.Id });
        }
        catch (Exception ex)
        {
            TempData["Error"] = "Có lỗi xảy ra: " + ex.Message;
            return RedirectToAction("Checkout");
        }
    }

    // ========== THANK YOU ==========

    // GET: ShoppingCart/ThankYou/{id}
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

    // Chỉ GIỮ LẠI MỘT phương thức GenerateOrderCode (xóa cái còn lại)
    private string GenerateOrderCode()
    {
        return "DTK" + DateTime.Now.ToString("yyyyMMddHHmmss") + new Random().Next(1000, 9999);
    }
}