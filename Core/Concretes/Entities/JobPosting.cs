using System;
using System.ComponentModel.DataAnnotations;
using Core.Abstracts.Bases;

namespace Core.Concretes.Entities
{
    public class JobPosting : BaseEntity
    {
        [Required]
        [MaxLength(150)]
        public string Title { get; set; } = null!;

        [Required]
        [MaxLength(2000)]
        public string Description { get; set; } = null!;

        [Required]
        public string Location { get; set; } = null!;

        public DateTime DateNeeded { get; set; }

        public int EstimatedHours { get; set; }

        public decimal Budget { get; set; }

        [Required]
        public string Status { get; set; } = "Open"; // e.g., Open, Assigned, Completed, Cancelled

        // Foreign Keys
        [Required]
        public string CustomerId { get; set; } = null!;
        public virtual Customer? Customer { get; set; }

        public string? AssignedWorkerId { get; set; }
        public virtual Worker? AssignedWorker { get; set; }
        // ... (Keep existing properties) ...

        // Navigation property for the new 1-to-Many relationship
        public virtual ICollection<JobApplication> Applications { get; set; } = new List<JobApplication>();
    }
}