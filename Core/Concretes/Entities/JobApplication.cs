using System;
using System.ComponentModel.DataAnnotations;
using Core.Abstracts.Bases;
using Core.Concretes.Enums;

namespace Core.Concretes.Entities
{
    public class JobApplication : BaseEntity
    {
        // Link to the Job
        [Required]
        public string JobPostingId { get; set; } = null!;
        public virtual JobPosting? JobPosting { get; set; }

        // Link to the Worker who applied
        [Required]
        public string WorkerId { get; set; } = null!;
        public virtual Worker? Worker { get; set; }

        [Required]
        public ApplicationStatus Status { get; set; } = ApplicationStatus.Pending;

        // NEW: worker's note to the customer
        [MaxLength(500)]
        public string? MessageToCustomer { get; set; }

        // NEW: earliest date worker can start
        public DateTime? SoonestAvailableStartDate { get; set; }

        // NEW: whether the worker is currently employed / engaged elsewhere
        public bool IsCurrentlyWorking { get; set; }

        // NEW: any clarification questions for the customer
        [MaxLength(1000)]
        public string? QuestionsAboutWork { get; set; }
    }
}