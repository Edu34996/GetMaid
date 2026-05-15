using System.ComponentModel.DataAnnotations;

namespace Core.Concretes.Entities
{
    public class Worker : ApplicationUser
    {
        [MaxLength(1000)]
        public string Bio { get; set; } = string.Empty;

        public decimal HourlyRate { get; set; }
        
        public int ExperienceYears { get; set; }

        public bool ProvidesMaidService { get; set; }
        
        public bool ProvidesChildcare { get; set; }
    }
}