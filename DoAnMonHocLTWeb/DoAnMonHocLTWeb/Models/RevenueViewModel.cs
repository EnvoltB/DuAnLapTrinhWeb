using GearDTK.Models;

namespace GearDTK.ViewModels;

public class RevenueViewModel
{
    // Tổng quan
    public decimal TotalRevenue { get; set; }
    public int TotalOrders { get; set; }
    public int TotalProductsSold { get; set; }
    public decimal AverageOrderValue { get; set; }

    // Biểu đồ
    public List<RevenueChartData> DailyRevenue { get; set; } = new();
    public List<RevenueChartData> MonthlyRevenue { get; set; } = new();
    public List<RevenueChartData> YearlyRevenue { get; set; } = new();
    public List<TopProductData> TopProducts { get; set; } = new();

    // Bộ lọc
    public string? FilterType { get; set; } // day, month, year
    public DateTime? SelectedDate { get; set; }
    public int? SelectedMonth { get; set; }
    public int? SelectedYear { get; set; }
}

public class RevenueChartData
{
    public string Label { get; set; } = string.Empty;
    public decimal Revenue { get; set; }
    public int OrderCount { get; set; }
}

public class TopProductData
{
    public string ProductName { get; set; } = string.Empty;
    public int QuantitySold { get; set; }
    public decimal Revenue { get; set; }
    public string? ImageUrl { get; set; }
}