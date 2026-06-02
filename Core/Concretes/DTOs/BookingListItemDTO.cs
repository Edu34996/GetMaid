using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Core.Concretes.DTOs
{
    public class BookingListItemDTO
    {
        public string Id { get; set; } = null!;
        
        // Worker tracking info (for customer-side booking list)
        public string? WorkerId { get; set; }
        public string WorkerName { get; set; } = "Unknown";
        public string? WorkerProfilePictureUrl { get; set; }
        
        // Customer tracking info (for customer-side booking list)
        public string? CustomerId { get; set; }
        public string CustomerName { get; set; } = "Unknown";
        public string? CustomerProfilePictureUrl { get; set; }
        
        public string Title { get; set; } = null!;

        public string City { get; set; } = null!;
        
        [Column(TypeName = "decimal(18,2)")]
        public decimal Budget { get; set; }
        
        public int EstimatedHours { get; set; }
        public DateTime StartDate { get; set; }
        
        // Status allows for color-coding the row (e.g., Pending, Confirmed)
        public string Status { get; set; } = null!; 
    }
}