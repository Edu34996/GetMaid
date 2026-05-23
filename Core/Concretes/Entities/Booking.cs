using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Core.Abstracts.Bases;
using Core.Concretes.Enums;

namespace Core.Concretes.Entities
{
    public class Booking : BaseEntity
    {
        // ==========================================
        // SERVICE DETAILS (Matches JobPosting)
        // ==========================================
        [Required]
        [Display(Name = "Required Services")]
        public List<ServiceType> ServiceTypes { get; set; } = new List<ServiceType>();

        [Required]
        public WorkArrangement WorkArrangement { get; set; }

        [Required]
        public CommitmentPreference CommitmentPreference { get; set; }

        [Display(Name = "Required Skills")]
        public List<Skill> RequiredSkills { get; set; } = new List<Skill>();

        // ==========================================
        // BOOKING INFO (Matches JobPosting)
        // ==========================================
        [Required]
        [MaxLength(150)]
        public string Title { get; set; } = null!;

        [Required]
        [MaxLength(2000)]
        public string Description { get; set; } = null!;

        [MaxLength(1000)]
        public string? Requirements { get; set; }

        [Required]
        public string Location { get; set; } = null!;

        [Required]
        public DateTime StartDate { get; set; }
        
        [Required]
        public DateTime EndDate { get; set; }

        public int EstimatedHours { get; set; }

        [Column(TypeName = "decimal(18,2)")] 
        public decimal Budget { get; set; }

        [Required]
        public bool RequireNonSmoker { get; set; } = false;

        [Required]
        public ApplicationStatus Status { get; set; } = ApplicationStatus.Pending;

        // ==========================================
        // FOREIGN KEYS & RELATIONS
        // ==========================================
        [Required]
        public string CustomerId { get; set; } = null!;
        public virtual Customer Customer { get; set; } = null!; 
        
        public bool BookingInactive { get; set; } = false;

        // Unlike JobPosting, WorkerId is required here because a booking is 
        // explicitly proposed to a single, specific worker.
        [Required]
        public string WorkerId { get; set; } = null!;
        public virtual Worker Worker { get; set; } = null!; 
    }
}