using Core.Concretes.DTOs;
using Utils.Responses;

namespace Core.Abstracts.IServices
{
    public interface IWorkerService
    {
        Task<IResult<WorkerDashboardDTO>> GetDashboardProfileAsync(string workerId);
        
        Task<IResult> UpdateProfileAsync(string workerId, WorkerProfileUpdateDTO model);
        
        Task<IResult<IEnumerable<JobPostingCardDTO>>> GetOpenJobPostingsAsync(string workerId);
        
        Task<IResult> ApplyForJobAsync(string jobPostingId, string workerId, JobApplicationCreateDTO? model = null);
        
        Task<IResult<IEnumerable<BookingListItemDTO>>> GetMyBookingsAsync(string workerId);
        
        Task<IResult<BookingDetailDTO>> GetBookingDetailsAsync(string bookingId, string workerId);
        
        Task<IResult> RespondToBookingAsync(string bookingId, string workerId, bool isConfirmed);
        
        Task<IResult> LeaveReviewForCustomerAsync(ReviewCreateDTO model, string workerId);
        
        Task<IResult<ReviewUpdateDTO>> GetMyReviewByBookingIdAsync(string bookingId, string userId);
        
        Task<IResult> UpdateReviewAsync(ReviewUpdateDTO model, string userId);
        
        Task<IResult<IEnumerable<JobPostingCardDTO>>> GetMyCompletedJobsAsync(string workerId);
        
        Task<IResult<IEnumerable<JobPostingCardDTO>>> GetMyAppliedJobsAsync(string workerId);
        
    }
}