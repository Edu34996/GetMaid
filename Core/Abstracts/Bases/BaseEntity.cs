using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Abstracts.Bases
{
    public abstract class BaseEntity
    {
        // Unique identifier for every entity using a string-based GUID
        public string Id { get; set; } = Guid.NewGuid().ToString();

        // Timestamp for when the record was first created
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Nullable timestamp for when the record was last modified
        public DateTime? UpdatedAt { get; set; }

        // Boolean flag to implement soft-delete logic instead of physical deletion
        public bool IsDeleted { get; set; } = false;
    }
}