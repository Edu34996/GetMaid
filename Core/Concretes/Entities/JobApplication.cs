using System;
using Core.Abstracts.Bases;

namespace Core.Concretes.Entities
{
    public class JobApplication : BaseEntity
    {
        // Link to the Job
        public string JobPostingId { get; set; } = null!;
        public virtual JobPosting? JobPosting { get; set; }

        // Link to the Worker who applied
        public string WorkerId { get; set; } = null!;
        public virtual Worker? Worker { get; set; }

        // Status of this specific application: "Pending", "Accepted", or "Rejected"
        public string Status { get; set; } = "Pending"; 
    }
}