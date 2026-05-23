using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Core.Concretes.Entities
{
    public class ApplicationUser : IdentityUser
    {
        [Required]
        [MaxLength(50)]
        public string FirstName { get; set; } = null!;

        [Required]
        [MaxLength(50)]
        public string LastName { get; set; } = null!;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? LastLoginDate { get; set; }
        public bool IsDeleted { get; set; } = false;
        
        [InverseProperty("Reviewee")]
        public virtual ICollection<Review> ReviewsReceived { get; set; } = new List<Review>();

        [InverseProperty("Reviewer")]
        public virtual ICollection<Review> ReviewsGiven { get; set; } = new List<Review>();
    }
}