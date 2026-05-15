using System.Threading.Tasks;
using Core.Concretes.DTOs;
using Utils.Responses;

namespace Core.Abstracts.IServices
{
    public interface IWorkerService
    {
        Task<IResult<WorkerDashboardDTO>> GetProfileAsync(string workerId);
        Task<IResult> UpdateProfileAsync(string workerId, WorkerProfileUpdateDTO model);

        Task<IResult<IEnumerable<JobPostingDTO>>> GetOpenJobPostingsAsync(string workerId);
        
        Task<IResult> ApplyForJobAsync(string jobPostingId, string workerId);
        // ... (Keep existing profile and job board methods) ...

        // Booking Management
        Task<IResult<IEnumerable<BookingDTO>>> GetMyBookingsAsync(string workerId);
        
        // We use a boolean 'isConfirmed' to easily toggle between Confirmed and Rejected
        Task<IResult> RespondToBookingAsync(int bookingId, string workerId, bool isConfirmed);
        
        //Review Management
        Task<IResult> LeaveReviewForCustomerAsync(ReviewCreateDTO model, string workerId);
        Task<IResult<ReviewUpdateDTO>> GetMyReviewByBookingIdAsync(int bookingId, string userId);
        Task<IResult> UpdateReviewAsync(ReviewUpdateDTO model, string userId);
    }
}