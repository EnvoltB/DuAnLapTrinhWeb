using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GearDTK.Models;

public class Banners
{
    [Key]
    public int Id { get; set; }

    [Display(Name = "Tiêu đề")]
    public string? Title { get; set; }

    [Display(Name = "Mô tả")]
    public string? Description { get; set; }

    [Required(ErrorMessage = "Vui lòng chọn hình ảnh")]
    [Display(Name = "Hình ảnh")]
    public string? ImageUrl { get; set; }

    [Required(ErrorMessage = "Vui lòng nhập link")]
    [Display(Name = "Link URL")]
    public string? LinkUrl { get; set; }

    [Display(Name = "Thứ tự hiển thị")]
    public int DisplayOrder { get; set; } = 0;

    [Display(Name = "Hiển thị")]
    public bool IsActive { get; set; } = true;

    [Display(Name = "Slider chính")]
    public bool IsMainSlider { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime? UpdatedAt { get; set; }

    [NotMapped]
    public IFormFile? ImageFile { get; set; }
}