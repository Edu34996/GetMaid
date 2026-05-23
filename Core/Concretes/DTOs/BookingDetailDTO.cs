using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Core.Concretes.Enums;

namespace Core.Concretes.DTOs
{
    public class BookingDetailDTO
    {
        public string Id { get; set; } = null!;
        public string Title { get; set; } = null!;
        public string Description { get; set; } = null!;
        public string? Requirements { get; set; }
        
        // Customer Context
        public string CustomerId { get; set; } = null!; // Added
        public string CustomerName { get; set; } = null!;
        public string CustomerAddress { get; set; } = null!;
        public string? CustomerPhoneNumber { get; set; } // If available
        
        // Scheduling and Financials
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int EstimatedHours { get; set; }
        
        [Column(TypeName = "decimal(18,2)")]
        public decimal Budget { get; set; }
        
        // Requirements
        public bool RequireNonSmoker { get; set; }
        public List<ServiceType> ServiceTypes { get; set; } = new List<ServiceType>();
        public List<Skill> RequiredSkills { get; set; } = new List<Skill>();
        
        public ApplicationStatus Status { get; set; }
    }
}