using Microsoft.AspNetCore.Mvc;
using GearDTK.Models;
using GearDTK.Repositories;

namespace GearDTK.Controllers;

public class CartController : Controller
{
    private readonly IProductRepository _productRepository;
    private readonly ICartRepository _cartRepository;

    public CartController(IProductRepository productRepository, ICartRepository cartRepository)
    {
        _productRepository = productRepository;
        _cartRepository = cartRepository;
    }

    // GET: Cart/Index
    public IActionResult Index()
    {
        var cartItems = _cartRepository.GetCartItems();
        var total = _cartRepository.GetCartTotal();

        ViewBag.Total = total;
        ViewBag.ItemCount = _cartRepository.GetCartCount();

        return View(cartItems);
    }

    // POST: Cart/Add
    [HttpPost]
    public async Task<IActionResult> Add(int productId, int quantity = 1)
    {
        var product = await _productRepository.GetByIdAsync(productId);

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

        return Json(new { success = true, cartCount = cartCount, message = "Đã thêm vào giỏ hàng" });
    }

    // POST: Cart/Update
    [HttpPost]
    public IActionResult Update(int productId, int quantity)
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

    // POST: Cart/Remove
    [HttpPost]
    public IActionResult Remove(int productId)
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

    // POST: Cart/Clear
    [HttpPost]
    public IActionResult Clear()
    {
        _cartRepository.ClearCart();

        return Json(new
        {
            success = true,
            message = "Đã xóa toàn bộ giỏ hàng"
        });
    }

    // GET: Cart/GetCount
    [HttpGet]
    public IActionResult GetCartCount()
    {
        var count = _cartRepository.GetCartCount();
        return Json(new { count = count });
    }
    // POST: Cart/SaveOrderNote
    [HttpPost]
    public IActionResult SaveOrderNote(string note)
    {
        HttpContext.Session.SetString("OrderNote", note ?? "");
        return Json(new { success = true, message = "Đã lưu ghi chú đơn hàng" });
    }
}