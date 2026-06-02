using System.ComponentModel.DataAnnotations;

namespace Core.Concretes.DTOs
{
    public class ForgotPasswordDTO
    {
        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
        [MaxLength(256)]
        public string Email { get; set; } = null!;
    }
}