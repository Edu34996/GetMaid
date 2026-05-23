using System.ComponentModel.DataAnnotations;
using Core.Abstracts.Bases;

namespace Core.Concretes.Entities
{
    public class Child : BaseEntity
    {
        public int? Age { get; set; }

        [MaxLength(1000)]
        public string? Bio { get; set; }

        [MaxLength(500)]
        public string? SpecialCareInstructions { get; set; }

        [Required]
        public string CustomerId { get; set; } = null!; 

        public virtual Customer Customer { get; set; } = null!;
    }
}