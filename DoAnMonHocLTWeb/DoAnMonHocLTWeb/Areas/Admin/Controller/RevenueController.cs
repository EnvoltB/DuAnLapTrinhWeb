using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GearDTK.Data;
using GearDTK.ViewModels;

namespace GearDTK.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class RevenueController : Controller
{
    private readonly ApplicationDbContext _context;

    public RevenueController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: Admin/Revenue
    public async Task<IActionResult> Index(
        string filterType = "day",
        DateTime? selectedDate = null,
        int? selectedMonth = null,
        int? selectedYear = null)
    {
        var viewModel = new RevenueViewModel
        {
            FilterType = filterType,
            SelectedDate = selectedDate ?? DateTime.Today,
            SelectedMonth = selectedMonth ?? DateTime.Today.Month,
            SelectedYear = selectedYear ?? DateTime.Today.Year
        };

        // Lấy tất cả đơn hàng đã giao thành công
        var deliveredOrders = _context.Orders
            .Include(o => o.OrderItems)
            .Where(o => o.OrderStatus == "Delivered" || o.OrderStatus == "Completed")
            .AsQueryable();

        // Áp dụng bộ lọc
        if (filterType == "day" && selectedDate.HasValue)
        {
            var date = selectedDate.Value.Date;
            deliveredOrders = deliveredOrders.Where(o => o.OrderDate.Date == date);
        }
        else if (filterType == "month" && selectedMonth.HasValue && selectedYear.HasValue)
        {
            deliveredOrders = deliveredOrders.Where(o =>
                o.OrderDate.Month == selectedMonth.Value &&
                o.OrderDate.Year == selectedYear.Value);
        }
        else if (filterType == "year" && selectedYear.HasValue)
        {
            deliveredOrders = deliveredOrders.Where(o => o.OrderDate.Year == selectedYear.Value);
        }

        // Tổng quan
        viewModel.TotalOrders = await deliveredOrders.CountAsync();
        viewModel.TotalRevenue = await deliveredOrders.SumAsync(o => o.Total);
        viewModel.TotalProductsSold = await deliveredOrders
            .SelectMany(o => o.OrderItems)
            .SumAsync(oi => oi.Quantity);
        viewModel.AverageOrderValue = viewModel.TotalOrders > 0
            ? viewModel.TotalRevenue / viewModel.TotalOrders
            : 0;

        // Biểu đồ theo ngày (7 ngày gần nhất hoặc theo tháng)
        if (filterType == "day")
        {
            var startDate = selectedDate?.Date ?? DateTime.Today;
            var endDate = startDate.AddDays(7);

            for (var date = startDate; date < endDate; date = date.AddDays(1))
            {
                var dayOrders = await _context.Orders
                    .Where(o => o.OrderDate.Date == date &&
                                (o.OrderStatus == "Delivered" || o.OrderStatus == "Completed"))
                    .ToListAsync();

                viewModel.DailyRevenue.Add(new RevenueChartData
                {
                    Label = date.ToString("dd/MM"),
                    Revenue = dayOrders.Sum(o => o.Total),
                    OrderCount = dayOrders.Count
                });
            }
        }
        else if (filterType == "month")
        {
            // Biểu đồ theo ngày trong tháng
            var year = selectedYear ?? DateTime.Today.Year;
            var month = selectedMonth ?? DateTime.Today.Month;
            var daysInMonth = DateTime.DaysInMonth(year, month);

            for (int day = 1; day <= daysInMonth; day++)
            {
                var date = new DateTime(year, month, day);
                var dayOrders = await _context.Orders
                    .Where(o => o.OrderDate.Date == date &&
                                (o.OrderStatus == "Delivered" || o.OrderStatus == "Completed"))
                    .ToListAsync();

                viewModel.DailyRevenue.Add(new RevenueChartData
                {
                    Label = day.ToString(),
                    Revenue = dayOrders.Sum(o => o.Total),
                    OrderCount = dayOrders.Count
                });
            }
        }
        else if (filterType == "year")
        {
            // Biểu đồ theo tháng
            var year = selectedYear ?? DateTime.Today.Year;

            for (int month = 1; month <= 12; month++)
            {
                var monthOrders = await _context.Orders
                    .Where(o => o.OrderDate.Year == year &&
                                o.OrderDate.Month == month &&
                                (o.OrderStatus == "Delivered" || o.OrderStatus == "Completed"))
                    .ToListAsync();

                viewModel.MonthlyRevenue.Add(new RevenueChartData
                {
                    Label = $"Tháng {month}",
                    Revenue = monthOrders.Sum(o => o.Total),
                    OrderCount = monthOrders.Count
                });
            }
        }

        // Biểu đồ theo tháng (12 tháng gần nhất)
        var monthlyData = new List<RevenueChartData>();
        for (int i = 11; i >= 0; i--)
        {
            var date = DateTime.Today.AddMonths(-i);
            var monthOrders = await _context.Orders
                .Where(o => o.OrderDate.Year == date.Year &&
                            o.OrderDate.Month == date.Month &&
                            (o.OrderStatus == "Delivered" || o.OrderStatus == "Completed"))
                .ToListAsync();

            monthlyData.Add(new RevenueChartData
            {
                Label = date.ToString("MM/yyyy"),
                Revenue = monthOrders.Sum(o => o.Total),
                OrderCount = monthOrders.Count
            });
        }
        viewModel.MonthlyRevenue = monthlyData;

        // Biểu đồ theo năm (5 năm gần nhất)
        var yearlyData = new List<RevenueChartData>();
        var currentYear = DateTime.Today.Year;
        for (int i = 4; i >= 0; i--)
        {
            var year = currentYear - i;
            var yearOrders = await _context.Orders
                .Where(o => o.OrderDate.Year == year &&
                            (o.OrderStatus == "Delivered" || o.OrderStatus == "Completed"))
                .ToListAsync();

            yearlyData.Add(new RevenueChartData
            {
                Label = year.ToString(),
                Revenue = yearOrders.Sum(o => o.Total),
                OrderCount = yearOrders.Count
            });
        }
        viewModel.YearlyRevenue = yearlyData;

        // Top sản phẩm bán chạy
        var topProducts = await _context.OrderItems
            .Include(oi => oi.Product)
            .Where(oi => oi.Order != null &&
                         (oi.Order.OrderStatus == "Delivered" || oi.Order.OrderStatus == "Completed"))
            .GroupBy(oi => new { oi.ProductId, oi.ProductName, oi.ProductImage })
            .Select(g => new TopProductData
            {
                ProductName = g.Key.ProductName,
                ImageUrl = g.Key.ProductImage,
                QuantitySold = g.Sum(x => x.Quantity),
                Revenue = g.Sum(x => x.Price * x.Quantity)
            })
            .OrderByDescending(p => p.Revenue)
            .Take(10)
            .ToListAsync();

        viewModel.TopProducts = topProducts;

        return View(viewModel);
    }

    // GET: Admin/Revenue/GetChartData
    [HttpGet]
    public async Task<IActionResult> GetChartData(string type = "day", int? month = null, int? year = null)
    {
        var deliveredOrders = _context.Orders
            .Include(o => o.OrderItems)
            .Where(o => o.OrderStatus == "Delivered" || o.OrderStatus == "Completed")
            .AsQueryable();

        var chartData = new List<object>();

        if (type == "day")
        {
            var startDate = DateTime.Today.AddDays(-6);
            for (var date = startDate; date <= DateTime.Today; date = date.AddDays(1))
            {
                var dayOrders = await deliveredOrders
                    .Where(o => o.OrderDate.Date == date.Date)
                    .ToListAsync();

                chartData.Add(new
                {
                    label = date.ToString("dd/MM"),
                    revenue = dayOrders.Sum(o => o.Total),
                    orders = dayOrders.Count
                });
            }
        }
        else if (type == "month")
        {
            var m = month ?? DateTime.Today.Month;
            var y = year ?? DateTime.Today.Year;
            var daysInMonth = DateTime.DaysInMonth(y, m);

            for (int day = 1; day <= daysInMonth; day++)
            {
                var date = new DateTime(y, m, day);
                var dayOrders = await deliveredOrders
                    .Where(o => o.OrderDate.Date == date)
                    .ToListAsync();

                chartData.Add(new
                {
                    label = day.ToString(),
                    revenue = dayOrders.Sum(o => o.Total),
                    orders = dayOrders.Count
                });
            }
        }
        else if (type == "year")
        {
            var y = year ?? DateTime.Today.Year;

            for (int m = 1; m <= 12; m++)
            {
                var monthOrders = await deliveredOrders
                    .Where(o => o.OrderDate.Month == m && o.OrderDate.Year == y)
                    .ToListAsync();

                chartData.Add(new
                {
                    label = $"Tháng {m}",
                    revenue = monthOrders.Sum(o => o.Total),
                    orders = monthOrders.Count
                });
            }
        }

        return Json(chartData);
    }
}