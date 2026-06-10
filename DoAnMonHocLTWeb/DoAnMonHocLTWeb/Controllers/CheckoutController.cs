using Microsoft.AspNetCore.Mvc;
using GearDTK.Repositories;

namespace GearDTK.Controllers;

public class CheckoutController : Controller
{
    private readonly ICartRepository _cartRepository;

    public CheckoutController(ICartRepository cartRepository)
    {
        _cartRepository = cartRepository;
    }

    // GET: Checkout/Index
    public IActionResult Index()
    {
        var cartItems = _cartRepository.GetCartItems();
        var total = _cartRepository.GetCartTotal();

        if (!cartItems.Any())
        {
            return RedirectToAction("Index", "Cart");
        }

        ViewBag.Total = total;
        ViewBag.CartItems = cartItems;

        return View();
    }

    // POST: Checkout/PlaceOrder
    [HttpPost]
    public IActionResult PlaceOrder()
    {
        // Xử lý đặt hàng ở đây
        _cartRepository.ClearCart();
        TempData["Success"] = "Đặt hàng thành công!";
        return RedirectToAction("Index", "Home");
    }
}