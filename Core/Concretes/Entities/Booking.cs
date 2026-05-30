using System.ComponentModel.DataAnnotations;

namespace Core.Concretes.Entities
{
    public class Booking : ServiceRequest
    {
        // Booking-specific lifecycle flag
        public bool BookingInactive { get; set; } = false;

        // Unlike JobPosting, WorkerId is required here because a booking is
        // explicitly proposed to a single, specific worker.
        [Required]
        public string WorkerId { get; set; } = null!;

        public virtual Worker Worker { get; set; } = null!;
    }
}