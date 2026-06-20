using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GearDTK.Data;
using GearDTK.Models;

namespace GearDTK.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class OrderController : Controller
{
    private readonly ApplicationDbContext _context;

    public OrderController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: Admin/Order
    public async Task<IActionResult> Index(string searchString, string statusFilter)
    {
        ViewData["CurrentSearch"] = searchString;
        ViewData["CurrentStatus"] = statusFilter;

        var orders = _context.Orders
            .Include(o => o.OrderItems)
            .AsQueryable();

        // Lọc theo trạng thái
        if (!string.IsNullOrEmpty(statusFilter))
        {
            orders = orders.Where(o => o.OrderStatus == statusFilter);
        }

        // Tìm kiếm theo mã đơn hàng hoặc tên khách hàng
        if (!string.IsNullOrEmpty(searchString))
        {
            orders = orders.Where(o =>
                o.OrderCode.Contains(searchString) ||
                o.CustomerName.Contains(searchString) ||
                o.Email.Contains(searchString)
            );
        }

        // Sắp xếp theo ngày đặt mới nhất
        orders = orders.OrderByDescending(o => o.OrderDate);

        var orderList = await orders.ToListAsync();

        // Thống kê số lượng đơn hàng theo trạng thái
        ViewBag.PendingCount = await _context.Orders.CountAsync(o => o.OrderStatus == "Pending");
        ViewBag.ProcessingCount = await _context.Orders.CountAsync(o => o.OrderStatus == "Processing");
        ViewBag.ShippedCount = await _context.Orders.CountAsync(o => o.OrderStatus == "Shipped");
        ViewBag.DeliveredCount = await _context.Orders.CountAsync(o => o.OrderStatus == "Delivered");
        ViewBag.CancelledCount = await _context.Orders.CountAsync(o => o.OrderStatus == "Cancelled");
        ViewBag.TotalCount = await _context.Orders.CountAsync();

        return View(orderList);
    }

    // GET: Admin/Order/Details/{id}
    public async Task<IActionResult> Details(int id)
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

    // POST: Admin/Order/UpdateStatus
    [HttpPost]
    //[ValidateAntiForgeryToken] nếu không bật validateAntiforgeryToken sẽ bị tấn công mạng CSRF có thể thay đổi status của đơn hàng ( tắt chỉ để test nhanh (hoạt động tốt))
    public async Task<IActionResult> UpdateStatus(int id, string status)
    {
        var order = await _context.Orders.FindAsync(id);
        if (order == null)
        {
            return Json(new { success = false, message = "Không tìm thấy đơn hàng" });
        }

        // Kiểm tra trạng thái hợp lệ
        var validStatuses = new[] { "Pending", "Processing", "Shipped", "Delivered", "Cancelled" };
        if (!validStatuses.Contains(status))
        {
            return Json(new { success = false, message = "Trạng thái không hợp lệ" });
        }

        order.OrderStatus = status;

        // Nếu đơn hàng được giao thành công, cập nhật PaymentStatus
        if (status == "Delivered")
        {
            order.PaymentStatus = "Paid";
            order.PaymentDate = DateTime.Now;
        }

        await _context.SaveChangesAsync();

        return Json(new { success = true, message = "Cập nhật trạng thái thành công", newStatus = status });
    }

    // POST: Admin/Order/Delete
    [HttpPost]
    //[ValidateAntiForgeryToken]  nếu không bật validateAntiforgeryToken sẽ bị tấn công mạng CSRF có thể xóa status của đơn hàng ( tắt chỉ để test nhanh (hoạt động tốt))
    public async Task<IActionResult> Delete(int id)
    {
        var order = await _context.Orders
            .Include(o => o.OrderItems)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (order == null)
        {
            return Json(new { success = false, message = "Không tìm thấy đơn hàng" });
        }

        // Xóa các OrderItems trước
        if (order.OrderItems != null && order.OrderItems.Any())
        {
            _context.OrderItems.RemoveRange(order.OrderItems);
        }

        _context.Orders.Remove(order);
        await _context.SaveChangesAsync();

        return Json(new { success = true, message = "Xóa đơn hàng thành công" });
    }
    // GET: Admin/Order/GetNewOrdersCount
    [HttpGet]
    public async Task<IActionResult> GetNewOrdersCount()
    {
        // Đếm số đơn hàng mới (Pending hoặc Processing)
        var count = await _context.Orders
            .CountAsync(o => o.OrderStatus == "Pending" || o.OrderStatus == "Processing");

        return Json(new { count = count });
    }
}