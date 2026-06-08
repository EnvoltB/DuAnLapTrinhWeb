using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GearDTK.Models;

public class Category
{
    [Key]
    public int Id { get; set; }

    [Required(ErrorMessage = "Tên danh mục không được để trống")]
    [StringLength(100)]
    [Display(Name = "Tên danh mục")]
    public string Name { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    [Display(Name = "Slug (URL)")]
    public string Slug { get; set; } = string.Empty;

    [Display(Name = "Icon (FontAwesome class)")]
    public string IconClass { get; set; } = "fa-solid fa-box";

    [Display(Name = "Mô tả")]
    [DataType(DataType.MultilineText)]
    public string Description { get; set; } = string.Empty;

    [Display(Name = "Thứ tự hiển thị")]
    public int DisplayOrder { get; set; } = 0;

    [Display(Name = "Hiển thị trên trang chủ")]
    public bool ShowOnHomepage { get; set; } = true;

    [Display(Name = "Hình ảnh đại diện")]
    public string ImageUrl { get; set; } = string.Empty;

    // Navigation property
    public virtual ICollection<Product>? Products { get; set; }
}