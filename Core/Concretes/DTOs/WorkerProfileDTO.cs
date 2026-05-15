namespace Core.Concretes.DTOs
{
    // Exclusively for displaying data to the UI
    public class WorkerDashboardDTO
    {
        public string Id { get; set; } = null!;
        public string FirstName { get; set; } = null!;
        public string? Email { get; set; }
        public string? Bio { get; set; }
        public string? HourlyRate { get; set; }
        public bool ProvidesMaidService { get; set; }
        public bool ProvidesChildcare { get; set; }
    }

    // Exclusively for receiving updates from the UI
    public class WorkerProfileUpdateDTO
    {
        public string? Bio { get; set; }
        public string? HourlyRate { get; set; }
        public bool ProvidesMaidService { get; set; }
        public bool ProvidesChildcare { get; set; }
    }
}