using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Core.Concretes.Enums;

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

        // Geospatial coordinates populated by frontend geocoding
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }

        [MaxLength(1000)]
        public string? Bio { get; set; }

        [Required]
        public FamilyStatus FamilyStatus { get; set; }

        // Pet ownership metrics
        [Required]
        public bool HasPets { get; set; }

        public int? NumberOfPets { get; set; }

        [Required]
        public VerificationStatus IdentityVerificationStatus { get; set; } = VerificationStatus.Unverified;

        [MaxLength(500)]
        public string? IdentityDocumentPath { get; set; }
        
        public virtual ICollection<Child> Children { get; set; } = new List<Child>();
        
        
    }
}