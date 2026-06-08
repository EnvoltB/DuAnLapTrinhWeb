using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GearDTK.Models;

public class Product
{
    [Key]
    public int Id { get; set; }

    [Required(ErrorMessage = "Tên sản phẩm không được để trống")]
    [StringLength(200)]
    [Display(Name = "Tên sản phẩm")]
    public string Name { get; set; } = string.Empty;

    [Required]
    [StringLength(200)]
    [Display(Name = "Slug (URL)")]
    public string Slug { get; set; } = string.Empty;

    [Display(Name = "Mô tả chi tiết")]
    [DataType(DataType.MultilineText)]
    public string Description { get; set; } = string.Empty;

    [Display(Name = "Mô tả ngắn")]
    [StringLength(500)]
    public string ShortDescription { get; set; } = string.Empty;

    [Required]
    [Range(0, 100000000)]
    [Display(Name = "Giá bán")]
    [DataType(DataType.Currency)]
    public decimal Price { get; set; }

    [Display(Name = "Giá cũ (so sánh)")]
    [Range(0, 100000000)]
    [DataType(DataType.Currency)]
    public decimal? ComparePrice { get; set; }

    [Display(Name = "Hình ảnh chính")]
    public string MainImageUrl { get; set; } = string.Empty;

    [Display(Name = "Thư viện ảnh")]
    public string GalleryImages { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Danh mục")]
    public int CategoryId { get; set; }

    [Required]
    [Range(0, 9999)]
    [Display(Name = "Số lượng tồn kho")]
    public int StockQuantity { get; set; } = 0;

    [Display(Name = "Sản phẩm nổi bật")]
    public bool IsFeatured { get; set; }

    [Display(Name = "Sản phẩm mới")]
    public bool IsNew { get; set; }

    [Display(Name = "Sản phẩm bán chạy")]
    public bool IsBestSeller { get; set; }

    [Display(Name = "Thông số kỹ thuật (JSON)")]
    public string Specifications { get; set; } = string.Empty;

    [Display(Name = "Trọng lượng (gram)")]
    public int? Weight { get; set; }

    [Display(Name = "Màu sắc")]
    public string Color { get; set; } = string.Empty;

    [Display(Name = "Thương hiệu")]
    public string Brand { get; set; } = "ATK GEAR";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // Navigation property
    [ForeignKey("CategoryId")]
    public virtual Category? Category { get; set; }

    // Computed properties
    [NotMapped]
    public decimal? DiscountPercent => ComparePrice.HasValue && ComparePrice > Price
        ? Math.Round((1 - (Price / ComparePrice.Value)) * 100, 0)
        : null;

    [NotMapped]
    public string[] GalleryImagesList => string.IsNullOrEmpty(GalleryImages)
        ? Array.Empty<string>()
        : GalleryImages.Split(',', StringSplitOptions.RemoveEmptyEntries);
}