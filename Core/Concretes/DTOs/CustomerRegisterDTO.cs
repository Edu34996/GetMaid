using System.ComponentModel.DataAnnotations;
using Core.Concretes.Enums;

namespace Core.Concretes.DTOs
{
    /// <summary>
    /// Customer registration form input.
    /// </summary>
    public class CustomerRegisterDTO
    {
        [Required(ErrorMessage = "First name is required.")]
        [MaxLength(50)]
        public string FirstName { get; set; } = null!;

        [Required(ErrorMessage = "Last name is required.")]
        [MaxLength(50)]
        public string LastName { get; set; } = null!;

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress]
        public string Email { get; set; } = null!;

        [Required(ErrorMessage = "Password is required.")]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters.")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = null!;

        [Required(ErrorMessage = "Phone number is required.")]
        [Phone]
        public string PhoneNumber { get; set; } = null!;

        [Required(ErrorMessage = "Address is required.")]
        [MaxLength(200)]
        public string Address { get; set; } = null!;

        [Required(ErrorMessage = "City is required.")]
        [MaxLength(100)]
        public string City { get; set; } = null!;

        [Required(ErrorMessage = "Please select a family status.")]
        public FamilyStatus FamilyStatus { get; set; }

        [Required]
        public bool HasPets { get; set; }

        public int? NumberOfPets { get; set; }
    }
}