using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Core.Concretes.Enums;

namespace Core.Concretes.Entities
{
    public class JobPosting : ServiceRequest
    {
        // Open posting — worker is assigned after hiring
        public bool PostInactive { get; set; } = false;

        public string? AssignedWorkerId { get; set; }
        public virtual Worker? AssignedWorker { get; set; }

        public virtual ICollection<JobApplication> Applications { get; set; } = new List<JobApplication>();
    }
}