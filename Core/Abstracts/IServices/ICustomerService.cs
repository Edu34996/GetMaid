using System.Collections.Generic;
using System.Threading.Tasks;
using Core.Concretes.DTOs;
using Utils.Responses;

namespace Core.Abstracts.IServices
{
    public interface ICustomerService
    {
        // Profile Management
        Task<IResult<CustomerDashboardDTO>> GetProfileAsync(string customerId);
        Task<IResult> UpdateProfileAsync(string customerId, CustomerProfileUpdateDTO model);

        // Job Posting/Management
        Task<IResult> CreateJobPostingAsync(JobPostingCreateDTO model, string customerId);
        Task<IResult<IEnumerable<JobPostingDTO>>> GetMyJobPostingsAsync(string customerId);
        
        // Child Management
        Task<IResult> AddChildAsync(ChildDTO model, string customerId);
        Task<IResult<List<ChildDTO>>> GetMyChildrenAsync(string customerId);
        Task<IResult> RemoveChildAsync(string childId, string customerId);
        
        //WorkerShop Search-filter
        Task<IResult<IEnumerable<WorkerDashboardDTO>>> BrowseWorkersAsync(WorkerSearchFilterDTO filter);
        
        // Booking Management
        Task<IResult> CreateBookingAsync(BookingCreateDTO model, string customerId);
        Task<IResult<IEnumerable<BookingDTO>>> GetMyBookingsAsync(string customerId);
        
        // Job Application Management
        Task<IResult<IEnumerable<JobApplicationDTO>>> GetJobApplicantsAsync(string jobPostingId, string customerId);
        Task<IResult> HireWorkerForJobAsync(string applicationId, string customerId);
        
        //Review Management
        Task<IResult> LeaveReviewForWorkerAsync(ReviewCreateDTO model, string customerId);
        Task<IResult<ReviewUpdateDTO>> GetMyReviewByBookingIdAsync(string bookingId, string userId);
        Task<IResult> UpdateReviewAsync(ReviewUpdateDTO model, string userId);
    }
}