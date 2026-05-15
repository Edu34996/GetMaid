using System;

namespace Core.Concretes.DTOs
{
    public class JobApplicationDTO
    {
        public string ApplicationId { get; set; } = null!;
        public string JobPostingId { get; set; } = null!;
        
        // Worker Details to help the customer decide
        public string WorkerId { get; set; } = null!;
        public string WorkerName { get; set; } = null!;
        public string WorkerBio { get; set; } = null!;
        public decimal WorkerHourlyRate { get; set; }
        
        public string Status { get; set; } = null!;
    }
}