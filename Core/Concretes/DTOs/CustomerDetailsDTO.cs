using System;
using Core.Concretes.Enums;

namespace Core.Concretes.DTOs
{
    /// <summary>
    /// Public-facing customer profile viewed by workers.
    /// Only shows non-sensitive information.
    /// </summary>
    public class CustomerDetailsDTO
    {
        public string Id { get; set; } = null!;
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;

        public string City { get; set; } = null!;
        public string? Bio { get; set; }

        // Family context (helpful for workers to know)
        public FamilyStatus FamilyStatus { get; set; }
        public bool HasPets { get; set; }
        public int? NumberOfPets { get; set; }

        // Verification badge (builds trust)
        public VerificationStatus IdentityVerificationStatus { get; set; }

        // Member since
        public DateTime MemberSince { get; set; }
    }
}