using System;
using System.Collections.Generic;
using Core.Concretes.Entities;
using Core.Concretes.Enums;

namespace Core.Concretes.DTOs
{
    public class WorkerDashboardDTO
    {
        // --- Basic User Info (from ApplicationUser) ---
        public string Id { get; set; } = null!;
        public string FirstName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string PhoneNumber { get; set; } = null!;

        // --- Core Profile Details ---
        public string Bio { get; set; } = string.Empty;
        public bool IsSmoker { get; set; }
        public string City { get; set; } = null!;
        
        // --- Economics ---
        public decimal? MinHourlyRate { get; set; }
        public decimal? MaxHourlyRate { get; set; }
        public int ExperienceYears { get; set; }

        // --- Media & Status ---
        public string ProfilePictureUrl { get; set; } = null!;
        public string? IntroductionVideoUrl { get; set; }
        public string IdentityVerificationStatus { get; set; } = "Unverified";

        // --- Qualifications & Preferences ---
        public List<ServiceType> OfferedServices { get; set; } = new List<ServiceType>();
        public List<Skill> Skills { get; set; } = new List<Skill>();
        public List<AgeGroup> ExperiencedAgeGroups { get; set; } = new List<AgeGroup>();
        public List<string> LanguagesSpoken { get; set; } = new List<string>();
        
        public WorkArrangement PreferredArrangement { get; set; }
        public CommitmentPreference CommitmentPreference { get; set; }

        // --- Availability ---
        public int? MaxDaysPerWeek { get; set; }
        public int? MaxHoursPerDay { get; set; }
        public List<DayOfWeek> PreferredWorkDays { get; set; } = new List<DayOfWeek>();

        // --- Reputation Summary (Calculated in Service) ---
        public double AverageRating { get; set; }
        public int TotalReviews { get; set; }
    }
}