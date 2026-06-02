using System.Collections.Generic;
using System.Threading.Tasks;
using Core.Concretes.DTOs;
using Utils.Responses;

namespace Core.Abstracts.IServices
{
    public interface ICustomerService
    {
       
        Task<IResult<CustomerDashboardDTO>> GetProfileAsync(string customerId);
       
        Task<IResult> UpdateProfileAsync(string customerId, CustomerProfileUpdateDTO model);

        Task<IResult<string>> CreateServiceRequestAsync(ServiceRequestCreateDTO model, string customerId);

        Task<IResult<IEnumerable<JobPostingCardDTO>>> GetMyJobPostingsAsync(string customerId);
        
        Task<IResult<JobPostingDetailDTO>> GetJobPostingDetailsAsync(string jobId, string customerId);
        
        Task<IResult<IEnumerable<BookingListItemDTO>>> GetMyBookingsAsync(string customerId);
        
        Task<IResult<BookingDetailDTO>> GetBookingDetailsAsync(string bookingId, string customerId);
        
        Task<IResult> AddChildAsync(ChildDTO model, string customerId);
       
        Task<IResult<List<ChildDTO>>> GetMyChildrenAsync(string customerId);
        
        Task<IResult<ChildDTO>> GetChildByIdAsync(string childId, string customerId);
        
        Task<IResult> UpdateChildAsync(ChildDTO model, string customerId);
        
        Task<IResult> RemoveChildAsync(string childId, string customerId);
 
        Task<IResult<IEnumerable<WorkerCardDTO>>> BrowseWorkersAsync(WorkerSearchFilterDTO filter, string customerId);
        
        Task<IResult<WorkerDetailsDTO>> GetWorkerDetailsAsync(string workerId);
        
        Task<IResult<IEnumerable<JobApplicationDTO>>> GetJobApplicantsAsync(string jobPostingId, string customerId);
        
        Task<IResult> HireWorkerForJobAsync(string applicationId, string customerId);
        
        Task<IResult> LeaveReviewForWorkerAsync(ReviewCreateDTO model, string customerId);
        
        Task<IResult<ReviewUpdateDTO>> GetMyReviewByBookingIdAsync(string bookingId, string customerId);
        
        Task<IResult> UpdateReviewAsync(ReviewUpdateDTO model, string customerId);
        
        Task<IResult> UpdateBookingAsync(BookingDetailDTO model, string customerId);
        
        Task<IResult> CancelBookingAsync(string bookingId, string customerId);
        
        Task<IResult> UpdateJobPostingAsync(JobPostingDetailDTO model, string customerId);
        
        Task<IResult> CancelJobPostingAsync(string jobId, string customerId);

        Task<IResult<JobApplicationDTO>> GetJobApplicationDetailsAsync(string applicationId, string customerId);
    }
}