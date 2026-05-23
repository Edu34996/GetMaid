using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Core.Concretes.Enums;

namespace Core.Concretes.DTOs
{
    public class JobPostingDetailDTO
    {
        public string Id { get; set; } = null!;
        public string Title { get; set; } = null!;
        public string Description { get; set; } = null!;
        public string? Requirements { get; set; }
        public string Location { get; set; } = null!;
        
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int EstimatedHours { get; set; }
        
        [Column(TypeName = "decimal(18,2)")] 
        public decimal Budget { get; set; }
        
        public bool RequireNonSmoker { get; set; }
        public ApplicationStatus Status { get; set; }
        
        public List<ServiceType> ServiceTypes { get; set; } = new List<ServiceType>();
        public List<Skill> RequiredSkills { get; set; } = new List<Skill>();
        
        // Customer context is helpful on the details page 
        // to give the worker confidence before applying
        public string CustomerId { get; set; } = null!;
        public string CustomerName { get; set; } = null!; 
    }
}