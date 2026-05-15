using System;

namespace Core.Concretes.DTOs
{
    public class JobPostingDTO
    {
        public string Id { get; set; }
        public string Title { get; set; } = null!;
        public string Description { get; set; } = null!;
        public string Location { get; set; } = null!;
        public DateTime DateNeeded { get; set; }
        public int EstimatedHours { get; set; }
        public decimal Budget { get; set; }
        public string Status { get; set; } = null!;
        
        // We will pass the Customer's Name so the Worker can see who posted it
        public string CustomerId { get; set; } = null!;
        public string CustomerName { get; set; } = null!;
        
        public bool HasApplied { get; set; } = false;
    }
}