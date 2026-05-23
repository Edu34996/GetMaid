using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Core.Abstracts.Bases;
using Core.Concretes.Enums;

namespace Core.Concretes.Entities
{
    public class JobPosting : BaseEntity
    {
        // ==========================================
        // CHANGED: Upgraded to a List to support multi-service jobs
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
        
        public bool PostInactive { get; set; } = false;

        public string? AssignedWorkerId { get; set; }
        public virtual Worker? AssignedWorker { get; set; }

        public virtual ICollection<JobApplication> Applications { get; set; } = new List<JobApplication>();
    }
}