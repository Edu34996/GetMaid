using System;

namespace Core.Concretes.Entities
{
    public class Booking
    {
        public int Id { get; set; }
        
        public string CustomerId { get; set; } = null!;
        public virtual Customer? Customer { get; set; } // Added navigation property

        public string WorkerId { get; set; } = null!;
        public virtual Worker? Worker { get; set; }     // Added navigation property

        public DateTime ScheduledDate { get; set; }
        public int DurationHours { get; set; }

        // e.g., "Pending", "Confirmed", "Completed", "Cancelled"
        public string Status { get; set; } = "Pending"; 
    }
}