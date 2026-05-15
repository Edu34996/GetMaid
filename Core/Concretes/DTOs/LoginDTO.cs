using System.ComponentModel.DataAnnotations;

namespace Core.Concretes.DTOs
{
    public class LoginDTO
    {
        [Required]
        [EmailAddress]
        [Display(Name = "Email Address", Prompt = "Email Address")]
        public string Email { get; set; } = null!;

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; } = null!;

        public bool RememberMe { get; set; }
    }
}