using System.ComponentModel.DataAnnotations;
using Core.Abstracts.Bases;

namespace Core.Concretes.Entities
{
    public class WorkerReference : BaseEntity
    {
        [Required]
        public string WorkerId { get; set; } = null!;
        public virtual Worker Worker { get; set; } = null!;

        [Required]
        public string CustomerId { get; set; } = null!;
        public virtual Customer Customer { get; set; } = null!;

    }
}