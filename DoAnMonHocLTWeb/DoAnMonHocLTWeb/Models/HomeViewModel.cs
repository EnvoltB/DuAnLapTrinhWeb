using GearDTK.Models;

namespace GearDTK.ViewModels;

public class HomeViewModel
{
    public List<Product> FeaturedProducts { get; set; } = new();
    public List<Product> NewProducts { get; set; } = new();
    public List<Product> BestSellerProducts { get; set; } = new();
    public List<Category> Categories { get; set; } = new();
}