using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Core.Concretes.Enums;

namespace Core.Concretes.Entities
{
    public class Customer : ApplicationUser
    {
        [Required]
        public FamilyStatus FamilyStatus { get; set; }

        // Pet ownership metrics
        [Required]
        public bool HasPets { get; set; }

        public int? NumberOfPets { get; set; }
        
        public virtual ICollection<Child> Children { get; set; } = new List<Child>();
        
        public virtual ICollection<WorkerReference> WorkerReferences { get; set; } = new List<WorkerReference>();
    }
}