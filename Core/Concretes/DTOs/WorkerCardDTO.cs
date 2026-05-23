using System.Collections.Generic;
using Core.Concretes.Enums;

namespace Core.Concretes.DTOs
{
    public class WorkerCardDTO
    {
        public string Id { get; set; } = null!;
        public string FirstName { get; set; } = null!;
        public string ProfilePictureUrl { get; set; } = null!;
        public string City { get; set; } = null!;
        public bool IsSmoker { get; set; }
        
        // Economics
        public decimal? MinHourlyRate { get; set; }
        public decimal? MaxHourlyRate { get; set; }
        
        // NEW: Dynamically calculated for the card display
        public decimal? AverageHourlyRate { get; set; }

        // Reputation
        public double AverageRating { get; set; } 
        public int TotalReviews { get; set; }

        // NEW: Numeric counts instead of heavy lists for performance
        public int NumberOfSkills { get; set; }
        public int NumberOfLanguages { get; set; }

        // Kept as an enum list so the frontend can loop through and print matching icons
        public List<ServiceType> OfferedServices { get; set; } = new List<ServiceType>();
    }
}