using System;
using Core.Abstracts.Bases;
using Core.Concretes.Enums;

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

        public ApplicationStatus Status { get; set; } = ApplicationStatus.Pending;
    }
}