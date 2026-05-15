using System;
using System.ComponentModel.DataAnnotations;

namespace Core.Concretes.DTOs
{
    public class JobPostingCreateDTO
    {
        [Required(ErrorMessage = "A job title is required.")]
        [StringLength(150, MinimumLength = 5)]
        public string Title { get; set; } = null!;

        [Required(ErrorMessage = "Please describe the job details.")]
        [StringLength(2000)]
        public string Description { get; set; } = null!;

        [Required]
        public string Location { get; set; } = null!;

        [Required]
        public DateTime DateNeeded { get; set; }

        [Range(1, 100, ErrorMessage = "Estimated hours must be between 1 and 100.")]
        public int EstimatedHours { get; set; }

        [Range(1.0, 10000.0, ErrorMessage = "Budget must be a valid amount.")]
        public decimal Budget { get; set; }
        
        // Notice: We do NOT include 'Status', 'CustomerId', or 'AssignedWorkerId' here.
        // Those fields will be set securely by the Business Service, preventing malicious users 
        // from manipulating them via the HTML form.
    }
}