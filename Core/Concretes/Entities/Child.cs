using System.ComponentModel.DataAnnotations;

namespace Core.Concretes.Entities
{
    public class Child
    {
        [Key]
        public string Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string FirstName { get; set; } = null!;

        public int Age { get; set; }

        [MaxLength(500)]
        public string? SpecialCareInstructions { get; set; }

        // Foreign Key linking back to the Customer
        // Note: IdentityUser primary keys are strings by default
        [Required]
        public string CustomerId { get; set; } = null!; 

        public virtual Customer Customer { get; set; } = null!;
    }
}