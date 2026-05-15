using System;
using System.ComponentModel.DataAnnotations;
using Core.Concretes.Enums;

namespace Core.Concretes.DTOs
{
    // Used when a customer clicks "Book" on a worker's profile
    public class BookingCreateDTO
    {
        [Required]
        public string WorkerId { get; set; } = null!; // Hidden input from the Shop UI

        [Required]
        public DateTime ScheduledDate { get; set; }

        [Range(1, 12, ErrorMessage = "Duration must be between 1 and 12 hours.")]
        public int DurationHours { get; set; }
    }

    // Used to display booking history to both Customers and Workers
    public class BookingDTO
    {
        public string Id { get; set; }
        public string CustomerId { get; set; } = null!;
        public string WorkerId { get; set; } = null!;
        
        public string CustomerName { get; set; } = null!;
        public string WorkerName { get; set; } = null!;

        public DateTime ScheduledDate { get; set; }
        public int DurationHours { get; set; }
        public ApplicationStatus Status { get; set; }    }
}