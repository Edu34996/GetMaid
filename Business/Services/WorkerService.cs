using System;
using System.Threading.Tasks;
using AutoMapper;
using Core.Abstracts;
using Core.Abstracts.IServices;
using Core.Concretes.DTOs;
using Core.Concretes.Entities;
using Utils.Responses;

namespace Business.Services
{
    public class WorkerService : IWorkerService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        // Classic C# constructor injecting both UnitOfWork and AutoMapper
        public WorkerService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<IResult<WorkerDashboardDTO>> GetProfileAsync(string workerId)
        {
            var workerResult = await _unitOfWork.Workers.FindByIdAsync(workerId);
            
            if (!workerResult.IsSuccess || workerResult.Data == null)
            {
                return Result<WorkerDashboardDTO>.Failure(["Worker profile not found."], 404);
            }

            // AutoMapper creates a new DTO populated with the entity's data
            var dashboardDto = _mapper.Map<WorkerDashboardDTO>(workerResult.Data);

            return Result<WorkerDashboardDTO>.Success(dashboardDto);
        }

        public async Task<IResult> UpdateProfileAsync(string workerId, WorkerProfileUpdateDTO model)
        {
            var workerResult = await _unitOfWork.Workers.FindByIdAsync(workerId);
            
            if (!workerResult.IsSuccess || workerResult.Data == null)
            {
                return Result.Failure(["Worker not found to update."], 404);
            }

            var worker = workerResult.Data;

            // AutoMapper securely copies the specific fields from the DTO onto the tracked entity
            _mapper.Map(model, worker);

            await _unitOfWork.Workers.UpdateAsync(worker);

            return await _unitOfWork.CommitAsync();
        }
        
        // Update the method signature
        public async Task<IResult<IEnumerable<JobPostingDTO>>> GetOpenJobPostingsAsync(string workerId)
        {
            try
            {
                // 1. Fetch open jobs
                var repositoryResult = await _unitOfWork.JobPostings.FindManyAsync(
                    j => j.Status == "Open", 
                    "Customer"
                );

                if (!repositoryResult.IsSuccess || repositoryResult.Data == null)
                {
                    return Result<IEnumerable<JobPostingDTO>>.Failure(
                        repositoryResult.Messages ?? new[] { "Failed to retrieve open job postings." }
                    );
                }

                // 2. Fetch the applications made by this specific worker
                var myApplicationsResult = await _unitOfWork.JobApplications.FindManyAsync(
                    a => a.WorkerId == workerId
                );
                
                // Extract just the Job IDs into a fast lookup list
                var myAppliedJobIds = myApplicationsResult.IsSuccess && myApplicationsResult.Data != null 
                    ? myApplicationsResult.Data.Select(a => a.JobPostingId).ToList() 
                    : new List<string>();

                // 3. Map to DTOs using AutoMapper
                var postingDtos = _mapper.Map<IEnumerable<JobPostingDTO>>(repositoryResult.Data).ToList();
                
                // 4. Loop through and flip the flag if they already applied
                foreach (var dto in postingDtos)
                {
                    dto.HasApplied = myAppliedJobIds.Contains(dto.Id);
                }

                return Result<IEnumerable<JobPostingDTO>>.Success(postingDtos);
            }
            catch (Exception ex)
            {
                return Result<IEnumerable<JobPostingDTO>>.Failure(new[] { ex.Message });
            }
        }

        public async Task<IResult> ApplyForJobAsync(string jobPostingId, string workerId)
        {
            try
            {
                // 1. Fetch the job posting
                var jobResult = await _unitOfWork.JobPostings.FindByIdAsync(jobPostingId);

                if (!jobResult.IsSuccess || jobResult.Data == null)
                {
                    return Result.Failure(new[] { "Job posting not found." }, 404);
                }

                var job = jobResult.Data;

                // 2. Ensure the job is still accepting applications
                if (job.Status != "Open")
                {
                    return Result.Failure(new[] { "This job is no longer accepting applications." }, 400);
                }

                // 3. Prevent duplicate applications
                var existingApplication = await _unitOfWork.JobApplications.FindFirstAsync(
                    a => a.JobPostingId == jobPostingId && a.WorkerId == workerId
                );
                
                if (existingApplication.IsSuccess && existingApplication.Data != null)
                {
                    return Result.Failure(new[] { "You have already applied for this job." }, 400);
                }

                // 4. Create the new Job Application
                var application = new JobApplication
                {
                    JobPostingId = jobPostingId,
                    WorkerId = workerId,
                    Status = "Pending"
                };

                var createResult = await _unitOfWork.JobApplications.CreateAsync(application);
                if (!createResult.IsSuccess) return createResult;

                var commitResult = await _unitOfWork.CommitAsync();
                if (!commitResult.IsSuccess) return commitResult;

                return Result.Success();
            }
            catch (Exception ex)
            {
                return Result.Failure(new[] { ex.Message });
            }
        }
        
        public async Task<IResult<IEnumerable<BookingDTO>>> GetMyBookingsAsync(string workerId)
        {
            try
            {
                // Fetch bookings tied to this worker and eagerly load the Customer data
                var bookingsResult = await _unitOfWork.Bookings.FindManyAsync(
                    b => b.WorkerId == workerId,
                    "Customer", "Worker"
                );

                if (!bookingsResult.IsSuccess || bookingsResult.Data == null)
                {
                    return Result<IEnumerable<BookingDTO>>.Failure(
                        bookingsResult.Messages ?? new[] { "Failed to retrieve bookings." }
                    );
                }

                // AutoMapper seamlessly translates the entities (including the eagerly loaded names)
                var bookingDtos = _mapper.Map<IEnumerable<BookingDTO>>(bookingsResult.Data);

                // Sort so "Pending" requests show up at the top of the list!
                return Result<IEnumerable<BookingDTO>>.Success(
                    bookingDtos.OrderByDescending(b => b.Status == "Pending").ToList()
                );
            }
            catch (Exception ex)
            {
                return Result<IEnumerable<BookingDTO>>.Failure(new[] { ex.Message });
            }
        }

        public async Task<IResult> RespondToBookingAsync(string bookingId, string workerId, bool isConfirmed)
        {
            try
            {
                var bookingResult = await _unitOfWork.Bookings.FindByIdAsync(bookingId);

                if (!bookingResult.IsSuccess || bookingResult.Data == null)
                {
                    return Result.Failure(new[] { "Booking not found." }, 404);
                }

                var booking = bookingResult.Data;

                // Security Check: Does this booking actually belong to the logged-in worker?
                if (booking.WorkerId != workerId)
                {
                    return Result.Failure(new[] { "Unauthorized action." }, 401);
                }

                // Workflow Check: Only "Pending" bookings can be responded to
                if (booking.Status != "Pending")
                {
                    return Result.Failure(new[] { $"This booking is already {booking.Status}." }, 400);
                }

                // Update the status based on the boolean
                booking.Status = isConfirmed ? "Confirmed" : "Rejected";

                var updateResult = await _unitOfWork.Bookings.UpdateAsync(booking);
                if (!updateResult.IsSuccess) return updateResult;

                var commitResult = await _unitOfWork.CommitAsync();
                if (!commitResult.IsSuccess) return commitResult;

                return Result.Success();
            }
            catch (Exception ex)
            {
                return Result.Failure(new[] { ex.Message });
            }
        }
        public async Task<IResult> LeaveReviewForCustomerAsync(ReviewCreateDTO model, string workerId)
        {
            try
            {
                // 1. Verify the booking exists and belongs to this worker
                var bookingResult = await _unitOfWork.Bookings.FindByIdAsync(model.BookingId);
                if (!bookingResult.IsSuccess || bookingResult.Data == null) return Result.Failure(["Booking not found."], 404);
                
                var booking = bookingResult.Data;
                if (booking.WorkerId != workerId || booking.CustomerId != model.RevieweeId)
                {
                    return Result.Failure(["Unauthorized to review this booking."], 401);
                }

                // 2. Prevent Duplicate Reviews
                var existingReview = await _unitOfWork.Reviews.FindFirstAsync(
                    r => r.BookingId == model.BookingId && r.ReviewerId == workerId
                );
                
                if (existingReview.IsSuccess && existingReview.Data != null)
                {
                    return Result.Failure(["You have already reviewed this booking."], 400);
                }

                // 3. Create the Review
                var review = new Review
                {
                    BookingId = model.BookingId,
                    ReviewerId = workerId,
                    RevieweeId = model.RevieweeId, // This is the CustomerId
                    Rating = model.Rating,
                    Comment = model.Comment,
                    CreatedAt = DateTime.UtcNow
                };

                await _unitOfWork.Reviews.CreateAsync(review);
                return await _unitOfWork.CommitAsync();
            }
            catch (Exception ex)
            {
                return Result.Failure([ex.Message]);
            }
        }
        public async Task<IResult<ReviewUpdateDTO>> GetMyReviewByBookingIdAsync(int bookingId, string userId)
        {
            try
            {
                // Find the review where this user was the reviewer for this specific booking
                var reviewResult = await _unitOfWork.Reviews.FindFirstAsync(
                    r => r.BookingId == bookingId && r.ReviewerId == userId
                );

                if (!reviewResult.IsSuccess || reviewResult.Data == null)
                {
                    return Result<ReviewUpdateDTO>.Failure(["Review not found."]);
                }

                // Map to the Update DTO
                var dto = new ReviewUpdateDTO
                {
                    Id = reviewResult.Data.Id,
                    Rating = reviewResult.Data.Rating,
                    Comment = reviewResult.Data.Comment
                };

                return Result<ReviewUpdateDTO>.Success(dto);
            }
            catch (Exception ex)
            {
                return Result<ReviewUpdateDTO>.Failure([ex.Message]);
            }
        }

        public async Task<IResult> UpdateReviewAsync(ReviewUpdateDTO model, string userId)
        {
            try
            {
                var reviewResult = await _unitOfWork.Reviews.FindByIdAsync(model.Id);
                
                if (!reviewResult.IsSuccess || reviewResult.Data == null) return Result.Failure(["Review not found."], 404);

                var review = reviewResult.Data;

                // Security Check: Only the original author can edit this review
                if (review.ReviewerId != userId) return Result.Failure(["Unauthorized to edit this review."], 401);

                // Update the mutable fields
                review.Rating = model.Rating;
                review.Comment = model.Comment;

                var updateResult = await _unitOfWork.Reviews.UpdateAsync(review);
                if (!updateResult.IsSuccess) return updateResult;

                return await _unitOfWork.CommitAsync();
            }
            catch (Exception ex)
            {
                return Result.Failure([ex.Message]);
            }
        }
    }
}