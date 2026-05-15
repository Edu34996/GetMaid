using System.ComponentModel.DataAnnotations;

namespace Core.Concretes.DTOs
{
    // Used exclusively to send customer data to the UI
    public class CustomerDashboardDTO
    {
        public string Id { get; set; } = null!;
        public string FirstName { get; set; } = null!;
        public string? Email { get; set; }
        public string Address { get; set; } = null!;
        public string? City { get; set; }
    }

    // Used exclusively to accept profile updates from the UI
    public class CustomerProfileUpdateDTO
    {
        [Required]
        [MaxLength(200)]
        public string Address { get; set; } = null!;

        [MaxLength(100)]
        public string? City { get; set; }
    }
}