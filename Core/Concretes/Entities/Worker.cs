using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Core.Concretes.Enums;

namespace Core.Concretes.Entities
{
    public class Worker : ApplicationUser
    {
        // --- Core Profile Details ---
        public bool IsSmoker { get; set; }

        // Optional hourly rate range
        public decimal? MinHourlyRate { get; set; }
        public decimal? MaxHourlyRate { get; set; }     
        
        public bool IsExperienced { get; set; }
        
        public int? ExperienceYears { get; set; }

        // --- Media & Trust Properties ---
        //[Required]
        [MaxLength(500)]
        public string? ProfilePictureUrl { get; set; }

        [MaxLength(500)]
        public string? IntroductionVideoUrl { get; set; }
        
        // --- Qualifications, Services & Arrangements ---
        public List<ServiceType> OfferedServices { get; set; } = new List<ServiceType>();
        
        public List<Skill> Skills { get; set; } = new List<Skill>();

        public List<AgeGroup> ExperiencedAgeGroups { get; set; } = new List<AgeGroup>();

        public List<string> LanguagesSpoken { get; set; } = new List<string>();
        
        public WorkArrangement PreferredArrangement { get; set; }
        
        public CommitmentPreference CommitmentPreference { get; set; }

        // --- Availability & Scheduling ---
        public int? MaxDaysPerWeek { get; set; }
        public int? MaxHoursPerDay { get; set; }
        public List<DayOfWeek> PreferredWorkDays { get; set; } = new List<DayOfWeek>();

        // --- References & Reputation ---
        public virtual ICollection<WorkerReference> References { get; set; } = new List<WorkerReference>();
        
    }
}