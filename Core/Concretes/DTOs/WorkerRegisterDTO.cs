using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using Core.Concretes.Enums;

namespace Core.Concretes.DTOs
{
    public class WorkerRegisterDTO
    {
        // ==========================================
        // STEP 1: Basic Account Information
        // ==========================================
        [Required(ErrorMessage = "First name is required.")]
        [MaxLength(50)]
        [Display(Name = "First Name")]
        public string FirstName { get; set; } = null!;

        [Required(ErrorMessage = "Last name is required.")]
        [MaxLength(50)]
        [Display(Name = "Last Name")]
        public string LastName { get; set; } = null!;

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email format.")]
        public string Email { get; set; } = null!;

        [Required(ErrorMessage = "Phone number is required.")]
        [Phone(ErrorMessage = "Invalid phone number format.")]
        [Display(Name = "Phone Number")]
        public string? PhoneNumber { get; set; }

        [Required(ErrorMessage = "Password is required.")]
        [DataType(DataType.Password)]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters long.")]
        public string Password { get; set; } = null!;

        [DataType(DataType.Password)]
        [Display(Name = "Confirm Password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; } = null!;
        
        [Display(Name = "Are you a smoker?")]
        public bool IsSmoker { get; set; }

        // ==========================================
        // STEP 2: Media and Identity
        // ==========================================
        [MaxLength(1000)]
        public string? Bio { get; set; }

        [Display(Name = "Introduction Video URL (YouTube/Vimeo)")]
        [Url(ErrorMessage = "Please enter a valid URL.")]
        [MaxLength(500)]
        public string? IntroductionVideoUrl { get; set; }

        [Display(Name = "Profile Picture")]
        [Url(ErrorMessage = "Please enter a valid URL.")]
        [MaxLength(500)]
        public string? ProfilePictureUrl { get; set; }

        // ==========================================
        // STEP 3: Logistics, Location, and Scheduling
        // ==========================================
        [Required(ErrorMessage = "City/Base Location is required.")]
        [MaxLength(100)]
        public string City { get; set; } = null!;
        
        [MaxLength(200)]
        public string? Address { get; set; }
        
        [Display(Name = "Minimum Hourly Rate (Optional)")]
        [Range(0.01, 10000, ErrorMessage = "Rate must be greater than zero.")]
        public decimal? MinHourlyRate { get; set; }

        [Display(Name = "Maximum Hourly Rate (Optional)")]
        [Range(0.01, 10000, ErrorMessage = "Rate must be greater than zero.")]
        public decimal? MaxHourlyRate { get; set; }

        [Required(ErrorMessage = "Please select your living arrangement preference.")]
        [Display(Name = "Preferred Arrangement")]
        public WorkArrangement PreferredArrangement { get; set; }

        [Display(Name = "Max Days Per Week")]
        [Range(1, 7, ErrorMessage = "Days per week must be between 1 and 7.")]
        public int? MaxDaysPerWeek { get; set; }

        [Display(Name = "Max Hours Per Day")]
        [Range(1, 24, ErrorMessage = "Hours per day must be between 1 and 24.")]
        public int? MaxHoursPerDay { get; set; }

        [Display(Name = "Preferred Work Days")]
        public List<DayOfWeek> PreferredWorkDays { get; set; } = new List<DayOfWeek>();

        // ==========================================
        // STEP 4: Skills, Services, and Experience
        // ==========================================
        [Required]
        [Display(Name = "Years of Experience")]
        [Range(0, 50, ErrorMessage = "Please enter a valid number of years.")]
        public int ExperienceYears { get; set; }

        [Display(Name = "Services Offered")]
        public List<ServiceType> OfferedServices { get; set; } = new List<ServiceType>();

        [Display(Name = "Specific Skills")]
        public List<Skill> Skills { get; set; } = new List<Skill>();

        [Display(Name = "Experienced Age Groups")]
        public List<AgeGroup> ExperiencedAgeGroups { get; set; } = new List<AgeGroup>();

        [Display(Name = "Languages Spoken")]
        public List<string> LanguagesSpoken { get; set; } = new List<string>();

        [Required(ErrorMessage = "Please select your commitment preference.")]
        [Display(Name = "Commitment Preference")]
        public CommitmentPreference CommitmentPreference { get; set; }
    }
}
