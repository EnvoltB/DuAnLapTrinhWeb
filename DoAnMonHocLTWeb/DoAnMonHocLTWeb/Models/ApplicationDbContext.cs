
using GearDTK.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace GearDTK.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Product> Products { get; set; }
    public DbSet<Category> Categories { get; set; }

    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderItem> OrderItems { get; set; }
    public DbSet<WishlistItem> WishlistItems { get; set; }
    public DbSet<Cart> Carts { get; set; }
    public DbSet<CartItem> CartItems { get; set; }
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Cấu hình chỉ mục cho Slug
        builder.Entity<Product>()
            .HasIndex(p => p.Slug)
            .IsUnique();

        builder.Entity<Category>()
            .HasIndex(c => c.Slug)
            .IsUnique();

        // Cấu hình quan hệ
        builder.Entity<Product>()
            .HasOne(p => p.Category)
            .WithMany(c => c.Products)
            .HasForeignKey(p => p.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        // ========== SEED CATEGORIES ==========
        builder.Entity<Category>().HasData(
            new Category
            {
                Id = 1,
                Name = "Gaming Mice",
                Slug = "gaming-mice",
                IconClass = "fa-solid fa-computer-mouse",
                Description = "Ultra-light gaming mice with high-precision sensors for competitive gaming",
                DisplayOrder = 1,
                ShowOnHomepage = true,
                ImageUrl = "/images/categories/mice-category.jpg"
            },
            new Category
            {
                Id = 2,
                Name = "Gaming Keyboards",
                Slug = "gaming-keyboards",
                IconClass = "fa-solid fa-keyboard",
                Description = "Hall effect and mechanical keyboards for pro-level performance",
                DisplayOrder = 2,
                ShowOnHomepage = true,
                ImageUrl = "/images/categories/keyboard-category.jpg"
            }
        );

        // ========== SEED PRODUCTS ==========
        // Product 1: ATK Blazing Sky ZERO (Gaming Mice)
        builder.Entity<Product>().HasData(
            new Product
            {
                Id = 1,
                Name = "ATK Blazing Sky ZERO",
                Slug = "atk-blazing-sky-zero",
                ShortDescription = "Ultra-light 39g wireless gaming mouse with PAW3950 sensor",
                Description = @"The ATK Blazing Sky ZERO redefines lightweight gaming performance:
                <ul>
                    <li>Ultra-light at just 39g with solid holeless frosted translucent shell</li>
                    <li>Dual 8K wireless connectivity</li>
                    <li>Flagship PAW3950 Ultra sensor for high-precision tracking</li>
                    <li>Nordic 54 series chip for ultra-low latency</li>
                    <li>Nano coating for stable, comfortable grip</li>
                    <li>Built for competitive gaming</li>
                </ul>",
                Price = 2990000,
                ComparePrice = 3990000,
                MainImageUrl = "/images/products/atk-blazing-sky-zero.png",
                GalleryImages = "/images/products/atk-zero-1.png,/images/products/atk-zero-2.png",
                CategoryId = 1,
                StockQuantity = 50,
                IsFeatured = true,
                IsNew = true,
                IsBestSeller = true,
                Weight = 39,
                Color = "Frosted White",
                Brand = "ATK GEAR",
                Specifications = @"{""Sensor"":""PAW3950 Ultra"",""Connectivity"":""Dual 8K Wireless"",""Battery Life"":""80 hours"",""Switch"":""Optical"",""DPI"":""26000""}"
            }
        );

        // Product 2: ATK RS6 Hall Effect Keyboard (Gaming Keyboards)
        builder.Entity<Product>().HasData(
            new Product
            {
                Id = 2,
                Name = "ATK RS6 Hall Effect Keyboard",
                Slug = "atk-rs6-hall-effect",
                ShortDescription = "Co-Developed with Pro Player Aspas - Extreme Performance Gaming Keyboard",
                Description = @"The ATK RS6 Hall Effect Keyboard delivers unparalleled gaming performance:
                <ul>
                    <li>Hall Effect magnetic switches for ultra-fast response</li>
                    <li>Co-developed with VALORANT pro player Aspas</li>
                    <li>Adjustable actuation points (0.1mm - 4.0mm)</li>
                    <li>Rapid Trigger technology</li>
                    <li>RGB backlighting with customizable effects</li>
                    <li>PBT double-shot keycaps</li>
                    <li>Hot-swappable PCB</li>
                </ul>",
                Price = 4590000,
                ComparePrice = 5890000,
                MainImageUrl = "/images/products/atk-rs6-keyboard.png",
                GalleryImages = "/images/products/atk-rs6-1.png,/images/products/atk-rs6-2.png",
                CategoryId = 2,
                StockQuantity = 35,
                IsFeatured = true,
                IsNew = true,
                IsBestSeller = true,
                Weight = 850,
                Color = "Black",
                Brand = "ATK GEAR",
                Specifications = @"{""Switch Type"":""Hall Effect Magnetic"",""Keycaps"":""PBT Double-shot"",""Connectivity"":""Wired USB-C"",""Polling Rate"":""8000Hz"",""Actuation"":""Adjustable 0.1-4.0mm"",""Form Factor"":""75%""}"
            }
        );
    }
}