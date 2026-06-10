using Microsoft.AspNetCore.Mvc;
using GearDTK.Repositories;
using GearDTK.ViewModels;

namespace GearDTK.Controllers;

public class HomeController : Controller
{
    private readonly IProductRepository _productRepository;
    private readonly ICategoryRepository _categoryRepository;

    public HomeController(IProductRepository productRepository, ICategoryRepository categoryRepository)
    {
        _productRepository = productRepository;
        _categoryRepository = categoryRepository;
    }

    public async Task<IActionResult> Index()
    {
        var viewModel = new HomeViewModel
        {
            FeaturedProducts = (await _productRepository.GetFeaturedProductsAsync(8)).ToList(),
            NewProducts = (await _productRepository.GetNewProductsAsync(8)).ToList(),
            BestSellerProducts = (await _productRepository.GetBestSellerProductsAsync(4)).ToList(),
            Categories = (await _categoryRepository.GetHomepageCategoriesAsync()).ToList()
        };

        return View(viewModel);
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