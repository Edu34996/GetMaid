namespace Core.Concretes.DTOs
{
    public class WorkerSearchFilterDTO
    {
        public decimal? MaxHourlyRate { get; set; }
        public bool NeedsMaidService { get; set; }
        public bool NeedsChildcare { get; set; }
    }
}