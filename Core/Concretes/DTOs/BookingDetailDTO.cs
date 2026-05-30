using System;
using System.Collections.Generic;
using Core.Concretes.Enums;

namespace Core.Concretes.DTOs
{
    public class BookingDetailDTO
    {
        public string Id { get; set; } = null!;
        public string Title { get; set; } = null!;
        public string Description { get; set; } = null!;
        public string? Requirements { get; set; }
        public string Location { get; set; } = null!;

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int EstimatedHours { get; set; }
        public decimal Budget { get; set; }

        public bool RequireNonSmoker { get; set; }
        public ApplicationStatus Status { get; set; }

        public WorkArrangement WorkArrangement { get; set; }
        public CommitmentPreference CommitmentPreference { get; set; }
        public List<ServiceType> ServiceTypes { get; set; } = new List<ServiceType>();
        public List<Skill> RequiredSkills { get; set; } = new List<Skill>();

        // Customer context
        public string CustomerId { get; set; } = null!;
        public string CustomerName { get; set; } = null!;
        public string? CustomerAddress { get; set; }
        public string? CustomerPhoneNumber { get; set; }

        // Worker context
        public string WorkerId { get; set; } = null!;
        public string? WorkerName { get; set; }
        public string? WorkerPhoneNumber { get; set; }

        // Booking state
        public bool BookingInactive { get; set; }
    }
}