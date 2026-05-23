using System.Collections.Generic;
using Core.Concretes.Enums;

namespace Core.Concretes.DTOs
{
    public class WorkerDetailsDTO
    {
        public string Id { get; set; } = null!;
        public string FirstName { get; set; } = null!;
        public string ProfilePictureUrl { get; set; } = null!;
        public string City { get; set; } = null!;
        public bool IsSmoker { get; set; }

        // NEW: Reputation
        public double AverageRating { get; set; }
        public int TotalReviews { get; set; }

        // The Heavy Details
        public string? IntroductionVideoUrl { get; set; }
        public string? Bio { get; set; }
        
        public decimal? MinHourlyRate { get; set; }
        public decimal? MaxHourlyRate { get; set; }
        public int ExperienceYears { get; set; }

        public List<ServiceType> OfferedServices { get; set; } = new List<ServiceType>();
        public List<Skill> Skills { get; set; } = new List<Skill>();
        public List<AgeGroup> ExperiencedAgeGroups { get; set; } = new List<AgeGroup>();
        public List<string> LanguagesSpoken { get; set; } = new List<string>();
        
        // We only show their preferences, NOT their private schedule (MaxDays, etc.)
        public WorkArrangement PreferredArrangement { get; set; }
        public CommitmentPreference CommitmentPreference { get; set; }
    }
}