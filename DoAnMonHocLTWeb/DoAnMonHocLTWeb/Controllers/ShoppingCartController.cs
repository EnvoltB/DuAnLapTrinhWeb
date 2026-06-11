using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GearDTK.Data;
using GearDTK.Models;
using GearDTK.Repositories;
using GearDTK.ViewModels;
using Microsoft.AspNetCore.Authorization;

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
    public async Task<IActionResult> Index()
    {
        var userId = User.Identity?.Name;

        if (string.IsNullOrEmpty(userId))
        {
            return View(new List<CartItem>());
        }

        var cartItems = await _cartRepository.GetCartItemsAsync(userId);
        var total = await _cartRepository.GetCartTotalAsync(userId);
        var orderNote = HttpContext.Session.GetString("OrderNote") ?? "";

        ViewBag.Total = total;
        ViewBag.OrderNote = orderNote;
        ViewBag.ItemCount = await _cartRepository.GetCartCountAsync(userId);

        return View(cartItems);
    }

    // POST: ShoppingCart/AddToCart
    [HttpPost]
    public async Task<IActionResult> AddToCart(int productId, int quantity = 1)
    {
        var userId = User.Identity?.Name;

        if (string.IsNullOrEmpty(userId))
        {
            return Json(new { success = false, message = "Vui lòng đăng nhập để thêm vào giỏ hàng" });
        }

        var product = await _context.Products.FindAsync(productId);

        if (product == null)
        {
            return Json(new { success = false, message = "Sản phẩm không tồn tại" });
        }

        if (product.StockQuantity < quantity)
        {
            return Json(new { success = false, message = "Số lượng sản phẩm không đủ" });
        }

        await _cartRepository.AddToCartAsync(userId, productId, quantity);

        var cartCount = await _cartRepository.GetCartCountAsync(userId);

        return Json(new { success = true, cartCount = cartCount });
    }

    // POST: ShoppingCart/UpdateQuantity
    [HttpPost]
    public async Task<IActionResult> UpdateQuantity(int productId, int quantity)
    {
        var userId = User.Identity?.Name;

        if (string.IsNullOrEmpty(userId))
        {
            return Json(new { success = false, message = "Vui lòng đăng nhập" });
        }

        await _cartRepository.UpdateQuantityAsync(userId, productId, quantity);

        var cartItems = await _cartRepository.GetCartItemsAsync(userId);
        var total = await _cartRepository.GetCartTotalAsync(userId);
        var cartCount = await _cartRepository.GetCartCountAsync(userId);

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
    public async Task<IActionResult> RemoveFromCart(int productId)
    {
        var userId = User.Identity?.Name;

        if (string.IsNullOrEmpty(userId))
        {
            return Json(new { success = false, message = "Vui lòng đăng nhập" });
        }

        await _cartRepository.RemoveFromCartAsync(userId, productId);

        var total = await _cartRepository.GetCartTotalAsync(userId);
        var cartCount = await _cartRepository.GetCartCountAsync(userId);

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
    public async Task<IActionResult> GetCartCount()
    {
        var userId = User.Identity?.Name;

        if (string.IsNullOrEmpty(userId))
        {
            return Json(new { count = 0 });
        }

        var count = await _cartRepository.GetCartCountAsync(userId);
        return Json(new { count = count });
    }

    // ========== CHECKOUT ==========

    // GET: ShoppingCart/Checkout
    public async Task<IActionResult> Checkout()
    {
        var userId = User.Identity?.Name;

        if (string.IsNullOrEmpty(userId))
        {
            return RedirectToAction("Login", "Account");
        }

        var cartItems = await _cartRepository.GetCartItemsAsync(userId);

        if (!cartItems.Any())
        {
            return RedirectToAction("Index");
        }

        var orderNote = HttpContext.Session.GetString("OrderNote") ?? "";
        var total = await _cartRepository.GetCartTotalAsync(userId);

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
            var userId = User.Identity?.Name;

            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }

            var cartItems = await _cartRepository.GetCartItemsAsync(userId);

            if (!cartItems.Any())
            {
                TempData["Error"] = "Giỏ hàng trống!";
                return RedirectToAction("Index");
            }

            // ========== KIỂM TRA TỒN KHO TRƯỚC KHI ĐẶT HÀNG ==========
            foreach (var item in cartItems)
            {
                var product = await _context.Products.FindAsync(item.ProductId);
                if (product == null || product.StockQuantity < item.Quantity)
                {
                    TempData["Error"] = $"Sản phẩm {item.ProductName} không đủ số lượng tồn kho!";
                    return RedirectToAction("Checkout");
                }
            }

            // Tạo đơn hàng
            var order = new Order
            {
                OrderCode = GenerateOrderCode(),
                CustomerName = model.CustomerName ?? "Khách hàng",
                Email = userId ?? model.CustomerEmail ?? "",
                Phone = model.CustomerPhone ?? "",
                Address = model.CustomerAddress ?? "",
                City = model.City ?? "",
                Country = model.Country ?? "Vietnam",
                Subtotal = await _cartRepository.GetCartTotalAsync(userId),
                ShippingFee = 0,
                Discount = 0,
                Total = await _cartRepository.GetCartTotalAsync(userId),
                OrderNote = model.OrderNote ?? "",
                OrderStatus = "Pending",
                PaymentStatus = "Pending",
                OrderDate = DateTime.Now
            };

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            // Tạo OrderItems và TRỪ TỒN KHO
            foreach (var item in cartItems)
            {
                // Thêm OrderItem
                _context.OrderItems.Add(new OrderItem
                {
                    OrderId = order.Id,
                    ProductId = item.ProductId,
                    ProductName = item.ProductName,
                    ProductImage = item.ImageUrl,
                    Price = item.Price,
                    Quantity = item.Quantity
                });

                // ========== TRỪ SỐ LƯỢNG TỒN KHO ==========
                var product = await _context.Products.FindAsync(item.ProductId);
                if (product != null)
                {
                    product.StockQuantity -= item.Quantity;
                    _context.Products.Update(product);
                }
            }

            await _context.SaveChangesAsync();

            // Xóa giỏ hàng
            await _cartRepository.ClearCartAsync(userId);
            HttpContext.Session.Remove("OrderNote");

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

    private string GenerateOrderCode()
    {
        return "DTK" + DateTime.Now.ToString("yyyyMMddHHmmss") + new Random().Next(1000, 9999);
    }
}