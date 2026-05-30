using System.ComponentModel.DataAnnotations;

namespace Core.Concretes.Entities
{
    public class WorkerReference
    {
        [Required]
        public string WorkerId { get; set; } = null!;
        public virtual Worker Worker { get; set; } = null!;

        [Required]
        public string CustomerId { get; set; } = null!;
        public virtual Customer Customer { get; set; } = null!;

        // Optional: when the customer agreed to be a reference
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}