using System.Collections.Generic;
using Core.Concretes.Enums;

namespace Core.Concretes.DTOs
{
    public class WorkerSearchFilterDTO
    {
        // --- Location ---
        public string? City { get; set; }

        // --- Rate ---
        public decimal? MinHourlyRate { get; set; }
        public decimal? MaxHourlyRate { get; set; }

        // --- Experience ---
        public int? MinExperienceYears { get; set; }

        // --- Services & Skills ---
        public List<ServiceType> RequiredServices { get; set; } = new List<ServiceType>();
        public List<Skill> RequiredSkills { get; set; } = new List<Skill>();
        public List<AgeGroup> RequiredAgeGroups { get; set; } = new List<AgeGroup>();

        // --- Languages ---
        public List<string> RequiredLanguages { get; set; } = new List<string>();

        // --- Preferences ---
        public WorkArrangement? PreferredArrangement { get; set; }
        public CommitmentPreference? CommitmentPreference { get; set; }

        // --- Personal Preference ---
        public bool? NonSmokerOnly { get; set; }

        // --- Trust & Verification ---
        public bool? VerifiedOnly { get; set; }

        // --- Reputation ---
        public double? MinAverageRating { get; set; }
    }
}