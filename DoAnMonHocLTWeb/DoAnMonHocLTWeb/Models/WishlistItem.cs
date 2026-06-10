using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GearDTK.Models;

public class WishlistItem
{
    [Key]
    public int Id { get; set; }

    public string UserEmail { get; set; } = string.Empty;

    public int ProductId { get; set; }

    public DateTime AddedDate { get; set; } = DateTime.Now;

    [ForeignKey("ProductId")]
    public virtual Product? Product { get; set; }
}