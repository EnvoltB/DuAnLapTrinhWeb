using System.ComponentModel.DataAnnotations;

namespace GearDTK.Models
{
    public class ReviewVM
    {
        public int ProductId { get; set; }

        [Range(1, 5)]
        public int Rating { get; set; }

        [Required]
        public string Comment { get; set; }
    }
}
