using System.ComponentModel.DataAnnotations;

namespace Core.Concretes.DTOs
{
    public class WorkerRegisterDTO
    {
        [Required]
        [MaxLength(50)]
        public string FirstName { get; set; } = null!;

        [Required]
        [MaxLength(50)]
        public string LastName { get; set; } = null!;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = null!;

        [Required]
        [MinLength(6)]
        [DataType(DataType.Password)]
        public string Password { get; set; } = null!;

        [MaxLength(1000)]
        public string Bio { get; set; } = string.Empty;

        [Required]
        public decimal HourlyRate { get; set; }

        public int ExperienceYears { get; set; }

        public bool ProvidesMaidService { get; set; }

        public bool ProvidesChildcare { get; set; }
    }
}