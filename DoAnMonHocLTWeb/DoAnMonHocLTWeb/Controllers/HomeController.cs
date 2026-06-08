using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GearDTK.Data;
using GearDTK.ViewModels;

namespace GearDTK.Controllers;

public class HomeController : Controller
{
    private readonly ApplicationDbContext _context;

    public HomeController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var Model = new HomeViewModel
        {
            FeaturedProducts = await _context.Products
                .Include(p => p.Category)
                .Where(p => p.IsFeatured)
                .Take(8)
                .ToListAsync(),

            NewProducts = await _context.Products
                .Include(p => p.Category)
                .Where(p => p.IsNew)
                .Take(8)
                .ToListAsync(),

            BestSellerProducts = await _context.Products
                .Include(p => p.Category)
                .Where(p => p.IsBestSeller)
                .Take(4)
                .ToListAsync(),

            Categories = await _context.Categories
                .Where(c => c.ShowOnHomepage)
                .OrderBy(c => c.DisplayOrder)
                .ToListAsync()
        };

        return View(Model);
    }

    public IActionResult About()
    {
        return View();
    }

    public IActionResult Contact()
    {
        return View();
    }
}