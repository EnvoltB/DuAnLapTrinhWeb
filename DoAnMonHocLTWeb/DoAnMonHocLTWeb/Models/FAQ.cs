using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace GearDTK.Models
{
    public class FAQ 
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Category { get; set; }

        [Required]
        [StringLength(500)]
        public string Question { get; set; }

        [Required]
        public string Answer { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
