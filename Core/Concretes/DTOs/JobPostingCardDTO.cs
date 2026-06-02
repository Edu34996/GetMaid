using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Core.Concretes.Enums;

namespace Core.Concretes.DTOs
{
    public class JobPostingCardDTO
    {
        public string Id { get; set; } = null!;
        public string Title { get; set; } = null!;
        
        public string City { get; set; } = null!;        
        
        [Column(TypeName = "decimal(18,2)")] 
        public decimal Budget { get; set; }
        
        // These will be used by the front-end to render icons
        public List<ServiceType> ServiceTypes { get; set; } = new List<ServiceType>();
        
        public DateTime StartDate { get; set; }
    }
}