using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Core.Concretes.Enums;

namespace Core.Concretes.DTOs
{
    public class ServiceRequestCreateDTO
    {
        [Required(ErrorMessage = "A title is required.")]
        [MaxLength(150, ErrorMessage = "Title cannot exceed 150 characters.")]
        public string Title { get; set; } = null!;

        [Required(ErrorMessage = "A description is required.")]
        [MaxLength(2000, ErrorMessage = "Description cannot exceed 2000 characters.")]
        public string Description { get; set; } = null!;

        [MaxLength(1000)]
        public string? Requirements { get; set; }

        [Required(ErrorMessage = "City is required.")]
        [MaxLength(120, ErrorMessage = "City cannot exceed 120 characters.")]
        public string City { get; set; } = null!;

        [MaxLength(250, ErrorMessage = "Address cannot exceed 250 characters.")]
        public string Address { get; set; } = null!;

        [Required(ErrorMessage = "Start Date is required.")]
        public DateTime StartDate { get; set; }
        
        [Required(ErrorMessage = "End Date is required.")]
        public DateTime EndDate { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Estimated hours must be at least 1.")]
        public int EstimatedHours { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Budget must be greater than zero.")]
        public decimal Budget { get; set; }

        public bool RequireNonSmoker { get; set; }

        [Required]
        // Ensures the list isn't empty when submitted
        [MinLength(1, ErrorMessage = "At least one service type is required.")] 
        public List<ServiceType> ServiceTypes { get; set; } = new List<ServiceType>();

        [Required]
        public WorkArrangement WorkArrangement { get; set; }

        [Required]
        public CommitmentPreference CommitmentPreference { get; set; }

        public List<Skill> RequiredSkills { get; set; } = new List<Skill>();

        // ==========================================
        // THE ROUTING SWITCH
        // ==========================================
        // If this is null, the form was submitted from the "Create Job" page.
        // If this has a value, the form was submitted from a Worker's profile "Book" button.
        public string? TargetWorkerId { get; set; } 
    }
}