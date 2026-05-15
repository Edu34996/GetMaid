using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Core.Concretes.Entities
{
    public class Customer : ApplicationUser
    {
        [Required]
        [MaxLength(200)]
        public string Address { get; set; } = null!;

        [Required]
        [MaxLength(100)]
        public string City { get; set; } = null!;

        // Navigation property for associated children
        public virtual ICollection<Child> Children { get; set; } = new List<Child>();
    }
}