using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Core.Concretes.Enums;

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
        
        [MaxLength(1000)]
        public string Bio { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(100)]
        public string City { get; set; } = null!; // ADDED for local matching
        
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        
        [MaxLength(200)]
        public string? Address { get; set; }
        
        [MaxLength(500)]
        public string? IdentityDocumentPath { get; set; }
        
        public VerificationStatus IdentityVerificationStatus { get; set; } = VerificationStatus.Unverified;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? LastLoginDate { get; set; }
        public bool IsDeleted { get; set; } = false;
        
        [InverseProperty("Reviewee")]
        public virtual ICollection<Review> ReviewsReceived { get; set; } = new List<Review>();

        [InverseProperty("Reviewer")]
        public virtual ICollection<Review> ReviewsGiven { get; set; } = new List<Review>();
    }
}