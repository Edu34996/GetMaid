using System;
using System.ComponentModel.DataAnnotations;

namespace Core.Concretes.DTOs
{
    // Used when submitting a form
    public class ReviewCreateDTO
    {
        [Required]
        public string BookingId { get; set; }

        [Required]
        public string RevieweeId { get; set; } = null!; // Hidden input: Who are we reviewing?

        [Required(ErrorMessage = "Please select a rating.")]
        [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5 stars.")]
        public int Rating { get; set; }

        [MaxLength(1000)]
        public string? Comment { get; set; }
    }

    // Used when displaying reviews on a profile
    public class ReviewDTO
    {
        public string Id { get; set; }
        public string BookingId { get; set; }
        public string ReviewerName { get; set; } = null!;
        public int Rating { get; set; }
        public string? Comment { get; set; }
        public DateTime CreatedAt { get; set; }
    }
    // Used when updating an existing review
    public class ReviewUpdateDTO
    {
        public string Id { get; set; } // The actual Review ID in the database

        [Required(ErrorMessage = "Please select a rating.")]
        [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5 stars.")]
        public int Rating { get; set; }

        [MaxLength(1000)]
        public string? Comment { get; set; }
    }
}