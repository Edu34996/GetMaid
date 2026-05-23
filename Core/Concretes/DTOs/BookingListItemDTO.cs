using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Core.Concretes.DTOs
{
    public class BookingListItemDTO
    {
        public string Id { get; set; } = null!;
        public string CustomerId { get; set; } = null!; // Added
        public string CustomerName { get; set; } = null!; // Displayed for direct requests
        public string Title { get; set; } = null!;
        public string Location { get; set; } = null!;
        
        [Column(TypeName = "decimal(18,2)")]
        public decimal Budget { get; set; }
        
        public int EstimatedHours { get; set; }
        public DateTime StartDate { get; set; }
        
        // Status allows for color-coding the row (e.g., Pending, Confirmed)
        public string Status { get; set; } = null!; 
    }
}