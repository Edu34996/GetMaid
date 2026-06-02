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

        // Worker's application details
        [MaxLength(500)]
        public string? MessageToCustomer { get; set; }

        public DateTime? SoonestAvailableStartDate { get; set; }

        public bool IsCurrentlyWorking { get; set; }

        [MaxLength(1000)]
        public string? QuestionsAboutWork { get; set; }

        // Tracking dates
        public DateTime AppliedAt { get; set; } = DateTime.UtcNow;
        public DateTime? RejectedAt { get; set; }
        public DateTime? AcceptedAt { get; set; }

        // Optional: if application converted to booking
        public string? BookingId { get; set; }
        public virtual Booking? Booking { get; set; }
    }
}