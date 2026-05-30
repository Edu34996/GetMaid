using System;
using System.Collections.Generic;
using Core.Concretes.Enums;

namespace Core.Concretes.DTOs
{
    public class JobPostingDetailDTO
    {
        public string Id { get; set; } = null!;
        public string Title { get; set; } = null!;
        public string Description { get; set; } = null!;
        public string? Requirements { get; set; }
        public string Location { get; set; } = null!;

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int EstimatedHours { get; set; }
        public decimal Budget { get; set; }

        public bool RequireNonSmoker { get; set; }
        public ApplicationStatus Status { get; set; }

        public WorkArrangement WorkArrangement { get; set; }
        public CommitmentPreference CommitmentPreference { get; set; }
        public List<ServiceType> ServiceTypes { get; set; } = new List<ServiceType>();
        public List<Skill> RequiredSkills { get; set; } = new List<Skill>();

        // Customer context — gives workers confidence before applying
        public string CustomerId { get; set; } = null!;
        public string CustomerName { get; set; } = null!;

        // Posting state
        public bool PostInactive { get; set; }
        public string? AssignedWorkerId { get; set; }
    }
}