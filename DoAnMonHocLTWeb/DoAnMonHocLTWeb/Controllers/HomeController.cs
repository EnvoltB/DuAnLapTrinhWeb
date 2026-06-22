using Microsoft.AspNetCore.Mvc;
using GearDTK.Models;
using GearDTK.Repositories;
using GearDTK.ViewModels;

namespace GearDTK.Controllers;

public class HomeController : Controller
{
    private readonly IProductRepository _productRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly IBannerRepository _BannerRepository;  

    public HomeController(
        IProductRepository productRepository,
        ICategoryRepository categoryRepository,
        IBannerRepository BannerRepository)  
    {
        _productRepository = productRepository;
        _categoryRepository = categoryRepository;
        _BannerRepository = BannerRepository;  
    }

    public async Task<IActionResult> Index()
    {
        var viewModel = new HomeViewModel
        {
            FeaturedProducts = (await _productRepository.GetFeaturedProductsAsync(8)).ToList(),
            NewProducts = (await _productRepository.GetNewProductsAsync(8)).ToList(),
            BestSellerProducts = (await _productRepository.GetBestSellerProductsAsync(4)).ToList(),
            Categories = (await _categoryRepository.GetHomepageCategoriesAsync()).ToList(),
            Banner = (await _BannerRepository.GetMainSliderBannerAsync()).ToList()  
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