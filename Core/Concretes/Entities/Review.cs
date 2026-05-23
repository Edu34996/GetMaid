using System.ComponentModel.DataAnnotations.Schema;
using Core.Abstracts.Bases;

namespace Core.Concretes.Entities
{
    public class Review : BaseEntity
    {
        // Tying the review to a specific job proves they worked together!
        public string BookingId { get; set; } 
        
        public string ReviewerId { get; set; } = null!; // Person writing it
        public string RevieweeId { get; set; } = null!; // Person receiving it
        
        public int Rating { get; set; } // 1 to 5 stars
        public string? Comment { get; set; }
        
        [ForeignKey("ReviewerId")]
        public virtual ApplicationUser Reviewer { get; set; } = null!;

        [ForeignKey("RevieweeId")]
        public virtual ApplicationUser Reviewee { get; set; } = null!;
    }
}