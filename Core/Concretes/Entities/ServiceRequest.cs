using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Core.Abstracts.Bases;
using Core.Concretes.Enums;

namespace Core.Concretes.Entities
{
    /// <summary>
    /// Abstract base for all service-related requests.
    /// Both JobPosting and Booking share this structure.
    /// </summary>
    public abstract class ServiceRequest : BaseEntity
    {
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

        [Required]
        [Display(Name = "Required Services")]
        public List<ServiceType> ServiceTypes { get; set; } = new List<ServiceType>();

        [Required]
        public WorkArrangement WorkArrangement { get; set; }

        [Required]
        public CommitmentPreference CommitmentPreference { get; set; }

        [Display(Name = "Required Skills")]
        public List<Skill> RequiredSkills { get; set; } = new List<Skill>();

        // Every service request is owned by a customer
        [Required]
        public string CustomerId { get; set; } = null!;
        public virtual Customer Customer { get; set; } = null!;
    }
}