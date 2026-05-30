using System;
using System.Collections.Generic;
using Core.Concretes.Enums;

namespace Core.Concretes.DTOs
{
    /// <summary>
    /// Full customer profile for their own dashboard.
    /// Shows all editable + read-only fields.
    /// </summary>
    public class CustomerDashboardDTO
    {
        public string Id { get; set; } = null!;
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }

        // Profile
        public string Address { get; set; } = null!;
        public string City { get; set; } = null!;
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public string? Bio { get; set; }

        // Family Info
        public FamilyStatus FamilyStatus { get; set; }
        public bool HasPets { get; set; }
        public int? NumberOfPets { get; set; }

        // Verification
        public VerificationStatus IdentityVerificationStatus { get; set; }

        // Metadata
        public DateTime CreatedAt { get; set; }
        public DateTime? LastLoginDate { get; set; }
    }
}