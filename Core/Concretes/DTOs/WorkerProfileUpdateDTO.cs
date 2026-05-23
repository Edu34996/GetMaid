using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using Core.Concretes.Enums;

namespace Core.Concretes.DTOs
{
    public class WorkerProfileUpdateDTO
    {
        [Display(Name = "Are you a smoker?")]
        public bool IsSmoker { get; set; }
       
        
        [Required(ErrorMessage = "Phone number is required.")]
        [Phone]
        [Display(Name = "Phone Number")]
        public string PhoneNumber { get; set; } = null!;

        [Required(ErrorMessage = "City is required.")]
        [MaxLength(100)]
        public string City { get; set; } = null!;
        
        [Required]
        public double Latitude { get; set; }
        
        [Required]
        public double Longitude { get; set; }

        [MaxLength(1000)]
        public string? Bio { get; set; }
        
        [Display(Name = "Minimum Hourly Rate")]
        [Range(0.01, 10000, ErrorMessage = "Rate must be greater than zero.")]
        public decimal? MinHourlyRate { get; set; }

        [Display(Name = "Maximum Hourly Rate")]
        [Range(0.01, 10000, ErrorMessage = "Rate must be greater than zero.")]
        public decimal? MaxHourlyRate { get; set; }

        [Required]
        [Range(0, 50)]
        [Display(Name = "Years of Experience")]
        public int ExperienceYears { get; set; }

        [Display(Name = "Services Offered")]
        public List<ServiceType> OfferedServices { get; set; } = new List<ServiceType>();
        
        [Display(Name = "Specific Skills")]
        public List<Skill> Skills { get; set; } = new List<Skill>();
        
        [Display(Name = "Experienced Age Groups")]
        public List<AgeGroup> ExperiencedAgeGroups { get; set; } = new List<AgeGroup>();
        
        [Display(Name = "Languages Spoken")]
        public List<string> LanguagesSpoken { get; set; } = new List<string>();

        [Required]
        [Display(Name = "Preferred Arrangement")]
        public WorkArrangement PreferredArrangement { get; set; }
        
        [Required]
        [Display(Name = "Commitment Preference")]
        public CommitmentPreference CommitmentPreference { get; set; }
        
        [Display(Name = "Max Days Per Week")]
        [Range(1, 7)]
        public int? MaxDaysPerWeek { get; set; }
        
        [Display(Name = "Max Hours Per Day")]
        [Range(1, 24)]
        public int? MaxHoursPerDay { get; set; }
        
        [Display(Name = "Preferred Work Days")]
        public List<DayOfWeek> PreferredWorkDays { get; set; } = new List<DayOfWeek>();
    }
}