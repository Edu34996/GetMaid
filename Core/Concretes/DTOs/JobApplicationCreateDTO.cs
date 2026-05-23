using System.ComponentModel.DataAnnotations;

public class JobApplicationCreateDTO
{
    [Required]
    public string JobPostingId { get; set; } = null!;

    [MaxLength(500)]
    public string? MessageToCustomer { get; set; }

    public DateTime? SoonestAvailableStartDate { get; set; }

    public bool IsCurrentlyWorking { get; set; }

    [MaxLength(1000)]
    public string? QuestionsAboutWork { get; set; }
}