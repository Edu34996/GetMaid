using System;
using Core.Concretes.Enums;

namespace Core.Concretes.DTOs
{
    public class JobApplicationDTO
    {
        public string Id { get; set; } = null!;
        public string JobPostingId { get; set; } = null!;
        public string WorkerId { get; set; } = null!;

        // Helpful display fields
        public string WorkerName { get; set; } = null!;
        public string WorkerBio { get; set; } = null!;
        public decimal? WorkerMinHourlyRate { get; set; }
        public decimal? WorkerMaxHourlyRate { get; set; }

        // Application content
        public string? MessageToCustomer { get; set; }
        public DateTime? SoonestAvailableStartDate { get; set; }
        public bool IsCurrentlyWorking { get; set; }
        public string? QuestionsAboutWork { get; set; }

        public ApplicationStatus Status { get; set; }
    }
}
