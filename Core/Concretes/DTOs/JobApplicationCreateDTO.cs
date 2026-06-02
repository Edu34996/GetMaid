using System;
using System.ComponentModel.DataAnnotations;

namespace Core.Concretes.DTOs
{
    public class JobApplicationCreateDTO
    {
        [MaxLength(500)]
        public string? MessageToCustomer { get; set; }

        [DataType(DataType.Date)]
        public DateTime? SoonestAvailableStartDate { get; set; }

        public bool IsCurrentlyWorking { get; set; }

        [MaxLength(1000)]
        public string? QuestionsAboutWork { get; set; }
    }
}