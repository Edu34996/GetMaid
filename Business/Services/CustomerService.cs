using AutoMapper;
using Core.Abstracts;
using Core.Abstracts.IServices;
using Core.Concretes.DTOs;
using Core.Concretes.Entities;
using Utils.Responses;

namespace Business.Services
{
    public class CustomerService : ICustomerService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public CustomerService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<IResult<CustomerDashboardDTO>> GetProfileAsync(string customerId)
        {
            var customerResult = await _unitOfWork.Customers.FindByIdAsync(customerId);
            
            if (!customerResult.IsSuccess || customerResult.Data == null)
            {
                return Result<CustomerDashboardDTO>.Failure(["Customer profile not found."], 404);
            }

            var dashboardDto = _mapper.Map<CustomerDashboardDTO>(customerResult.Data);

            return Result<CustomerDashboardDTO>.Success(dashboardDto);
        }

        public async Task<IResult> UpdateProfileAsync(string customerId, CustomerProfileUpdateDTO model)
        {
            var customerResult = await _unitOfWork.Customers.FindByIdAsync(customerId);
            
            if (!customerResult.IsSuccess || customerResult.Data == null)
            {
                return Result.Failure(["Customer not found to update."], 404);
            }

            var customer = customerResult.Data;

            // AutoMapper securely copies the address and city fields to the tracked entity
            _mapper.Map(model, customer);

            await _unitOfWork.Customers.UpdateAsync(customer);
            return await _unitOfWork.CommitAsync();
        }

        public async Task<IResult> AddChildAsync(ChildDTO model, string customerId)
        {
            var child = _mapper.Map<Child>(model);
            child.CustomerId = customerId; // Ensure the child is linked to the active customer

            await _unitOfWork.Children.CreateAsync(child);
            return await _unitOfWork.CommitAsync();
        }

        public async Task<IResult<List<ChildDTO>>> GetMyChildrenAsync(string customerId)
        {
            var childrenResult = await _unitOfWork.Children.FindManyAsync(c => c.CustomerId == customerId);

            if (!childrenResult.IsSuccess || childrenResult.Data == null)
            {
                return Result<List<ChildDTO>>.Failure(["No children found."], 404);
            }

            var childDtos = _mapper.Map<List<ChildDTO>>(childrenResult.Data);

            return Result<List<ChildDTO>>.Success(childDtos);
        }

        public async Task<IResult> RemoveChildAsync(string childId, string customerId)
        {
            var childResult = await _unitOfWork.Children.FindByIdAsync(childId);

            if (!childResult.IsSuccess || childResult.Data == null)
            {
                return Result.Failure(["Child not found."], 404);
            }

            // Security Check: Ensure the user attempting the deletion owns the child record
            if (childResult.Data.CustomerId != customerId)
            {
                return Result.Failure(["Unauthorized action."], 401);
            }

            await _unitOfWork.Children.DeleteAsync(childResult.Data);
            return await _unitOfWork.CommitAsync();
        }
        // ... (Keep your existing GetProfileAsync, UpdateProfileAsync, AddChildAsync, etc.) ...
        
        public async Task<IResult> CreateJobPostingAsync(JobPostingCreateDTO model, string customerId)
        {
            try
            {
                // Manual mapping from the safe DTO to the Database Entity
                var jobPosting = new JobPosting
                {
                    Title = model.Title,
                    Description = model.Description,
                    Location = model.Location,
                    DateNeeded = model.DateNeeded,
                    EstimatedHours = model.EstimatedHours,
                    Budget = model.Budget,
                    
                    // Secure assignments: These ensure a user cannot manipulate their identity or the workflow
                    CustomerId = customerId,
                    Status = "Open"
                };

                // 1. Attempt to create the entity in the repository
                var createResult = await _unitOfWork.JobPostings.CreateAsync(jobPosting);
                
                // 2. Evaluate the repository result
                if (!createResult.IsSuccess)
                {
                    // Pass the repository-level failure (and its messages) up to the UI
                    return createResult; 
                }
                
                // 3. Commit the transaction to the database
                var commitResult = await _unitOfWork.CommitAsync();
                
                if (!commitResult.IsSuccess)
                {
                    return commitResult;
                }

                return Result.Success();
            }
            catch (Exception ex)
            {
                // Catch any unexpected business layer exceptions
                return Result.Failure(new[] { ex.Message }); 
            }
        }
        public async Task<IResult<IEnumerable<JobPostingDTO>>> GetMyJobPostingsAsync(string customerId)
        {
            try
            {
                // 1. Fetch the jobs tied to the authenticated customer using the new repository method.
                // We pass "Customer" into the includes array to ensure Eager Loading works correctly.
                var repositoryResult = await _unitOfWork.JobPostings.FindManyAsync(
                    j => j.CustomerId == customerId, 
                    "Customer"
                );

                // 2. Evaluate the repository result
                if (!repositoryResult.IsSuccess || repositoryResult.Data == null)
                {
                    return Result<IEnumerable<JobPostingDTO>>.Failure(
                        repositoryResult.Messages ?? new[] { "Failed to retrieve job postings." }
                    );
                }

                // 3. Manually map the entity collection back to DTOs for the UI
                var postingDtos = repositoryResult.Data.Select(j => new JobPostingDTO
                {
                    Id = j.Id,
                    Title = j.Title,
                    Description = j.Description,
                    Location = j.Location,
                    DateNeeded = j.DateNeeded,
                    EstimatedHours = j.EstimatedHours,
                    Budget = j.Budget,
                    Status = j.Status,
                    
                    // The null-coalescing operator provides a safe fallback
                    CustomerName = j.Customer?.FirstName ?? "Unknown" 
                }).ToList();

                return Result<IEnumerable<JobPostingDTO>>.Success(postingDtos);
            }
            catch (Exception ex)
            {
                // Catch any unexpected business layer exceptions
                return Result<IEnumerable<JobPostingDTO>>.Failure(new[] { ex.Message });
            }
        }
        public async Task<IResult<IEnumerable<WorkerDashboardDTO>>> BrowseWorkersAsync(WorkerSearchFilterDTO filter)
        {
            try
            {
                // 1. Fetch workers using dynamic filtering
                // If a filter is provided, we build an expression to check the parameters.
                // If the filter parameter is false/null, we ignore that specific condition.
                var workersResult = await _unitOfWork.Workers.FindManyAsync(w => 
                    (!filter.NeedsMaidService || w.ProvidesMaidService) &&
                    (!filter.NeedsChildcare || w.ProvidesChildcare) &&
                    (!filter.MaxHourlyRate.HasValue || w.HourlyRate <= filter.MaxHourlyRate.Value)
                );

                if (!workersResult.IsSuccess || workersResult.Data == null)
                {
                    return Result<IEnumerable<WorkerDashboardDTO>>.Failure(
                        workersResult.Messages ?? new[] { "Failed to retrieve workers." }
                    );
                }

                // 2. Map the results to the display DTO
                var workerDtos = workersResult.Data.Select(w => new WorkerDashboardDTO
                {
                    Id = w.Id,
                    FirstName = w.FirstName,
                    Email = w.Email,
                    Bio = w.Bio,
                    HourlyRate = w.HourlyRate.ToString("0.00"), // Formats to standard currency display
                    ProvidesMaidService = w.ProvidesMaidService,
                    ProvidesChildcare = w.ProvidesChildcare
                }).ToList();

                return Result<IEnumerable<WorkerDashboardDTO>>.Success(workerDtos);
            }
            catch (Exception ex)
            {
                return Result<IEnumerable<WorkerDashboardDTO>>.Failure(new[] { ex.Message });
            }
        }
        public async Task<IResult> CreateBookingAsync(BookingCreateDTO model, string customerId)
        {
            try
            {
                // Ensure the worker actually exists
                var workerCheck = await _unitOfWork.Workers.FindByIdAsync(model.WorkerId);
                if (!workerCheck.IsSuccess || workerCheck.Data == null)
                {
                    return Result.Failure(new[] { "The selected worker could not be found." });
                }

                var booking = new Booking
                {
                    WorkerId = model.WorkerId,
                    CustomerId = customerId,
                    ScheduledDate = model.ScheduledDate, 
                    DurationHours = model.DurationHours, 
                    Status = Core.Concretes.Enums.ApplicationStatus.Pending // REPLACED
                };

                var createResult = await _unitOfWork.Bookings.CreateAsync(booking);
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

        public async Task<IResult<IEnumerable<BookingDTO>>> GetMyBookingsAsync(string customerId)
        {
            try
            {
                // Fetch bookings and eagerly load BOTH Customer and Worker data
                var bookingsResult = await _unitOfWork.Bookings.FindManyAsync(
                    b => b.CustomerId == customerId,
                    "Customer", "Worker"
                );

                if (!bookingsResult.IsSuccess || bookingsResult.Data == null)
                {
                    return Result<IEnumerable<BookingDTO>>.Failure(
                        bookingsResult.Messages ?? new[] { "Failed to retrieve bookings." }
                    );
                }

                // Map to DTOs
                // Notice how we can mix manual mapping and AutoMapper depending on your preference.
                // Since we created the BookingProfiles, let's use AutoMapper here for clean code!
                var bookingDtos = _mapper.Map<IEnumerable<BookingDTO>>(bookingsResult.Data);

                return Result<IEnumerable<BookingDTO>>.Success(bookingDtos);
            }
            catch (Exception ex)
            {
                return Result<IEnumerable<BookingDTO>>.Failure(new[] { ex.Message });
            }
        }
        public async Task<IResult<IEnumerable<JobApplicationDTO>>> GetJobApplicantsAsync(string jobPostingId, string customerId)
        {
            try
            {
                // First, verify the job belongs to this customer
                var jobResult = await _unitOfWork.JobPostings.FindByIdAsync(jobPostingId);
                if (!jobResult.IsSuccess || jobResult.Data == null || jobResult.Data.CustomerId != customerId)
                {
                    return Result<IEnumerable<JobApplicationDTO>>.Failure(new[] { "Unauthorized or job not found." }, 401);
                }

                // Fetch applications and eagerly load the Worker data
                var applicationsResult = await _unitOfWork.JobApplications.FindManyAsync(
                    a => a.JobPostingId == jobPostingId,
                    "Worker"
                );

                if (!applicationsResult.IsSuccess || applicationsResult.Data == null)
                {
                    return Result<IEnumerable<JobApplicationDTO>>.Failure(new[] { "Failed to retrieve applicants." });
                }

                // Map to the DTO manually so we extract the worker's info perfectly
                var applicantDtos = applicationsResult.Data.Select(a => new JobApplicationDTO
                {
                    ApplicationId = a.Id,
                    JobPostingId = a.JobPostingId,
                    WorkerId = a.WorkerId,
                    WorkerName = a.Worker?.FirstName ?? "Unknown",
                    WorkerBio = a.Worker?.Bio ?? "No bio provided.",
                    WorkerHourlyRate = a.Worker?.HourlyRate ?? 0,
                    Status = a.Status
                }).ToList();

                return Result<IEnumerable<JobApplicationDTO>>.Success(applicantDtos);
            }
            catch (Exception ex)
            {
                return Result<IEnumerable<JobApplicationDTO>>.Failure(new[] { ex.Message });
            }
        }

        public async Task<IResult> HireWorkerForJobAsync(string applicationId, string customerId)
        {
            try
            {
                // 1. Fetch the application and strictly include the related JobPosting
                var appResult = await _unitOfWork.JobApplications.FindManyAsync(
                    a => a.Id == applicationId, 
                    "JobPosting"
                );
                
                var targetApplication = appResult.Data?.FirstOrDefault();

                if (targetApplication == null || targetApplication.JobPosting == null)
                {
                    return Result.Failure(new[] { "Application or Job not found." }, 404);
                }

                // 2. Security: Ensure the Customer actually owns this job!
                if (targetApplication.JobPosting.CustomerId != customerId)
                {
                    return Result.Failure(new[] { "Unauthorized action." }, 401);
                }

                // 3. Update the winning application
                targetApplication.Status = Core.Concretes.Enums.ApplicationStatus.Accepted; // REPLACED
                await _unitOfWork.JobApplications.UpdateAsync(targetApplication);


                // 4. Reject all other applications for this specific job
                var otherAppsResult = await _unitOfWork.JobApplications.FindManyAsync(
                    a => a.JobPostingId == targetApplication.JobPostingId && a.Id != applicationId
                );
                if (otherAppsResult.IsSuccess && otherAppsResult.Data != null)
                {
                    foreach (var otherApp in otherAppsResult.Data)
                    {
                        otherApp.Status = Core.Concretes.Enums.ApplicationStatus.Rejected; // REPLACED
                        await _unitOfWork.JobApplications.UpdateAsync(otherApp);
                    }
                }

                // 5. Officially assign the worker to the job and close it
                targetApplication.JobPosting.AssignedWorkerId = targetApplication.WorkerId;
                targetApplication.JobPosting.Status = "Assigned";
                await _unitOfWork.JobPostings.UpdateAsync(targetApplication.JobPosting);

                // 6. Commit the entire transaction
                var commitResult = await _unitOfWork.CommitAsync();
                if (!commitResult.IsSuccess) return commitResult;

                return Result.Success();
            }
            catch (Exception ex)
            {
                return Result.Failure(new[] { ex.Message });
            }
        }
        public async Task<IResult> LeaveReviewForWorkerAsync(ReviewCreateDTO model, string customerId)
        {
            try
            {
                // 1. Verify the booking exists and belongs to this customer
                var bookingResult = await _unitOfWork.Bookings.FindByIdAsync(model.BookingId);
                if (!bookingResult.IsSuccess || bookingResult.Data == null) return Result.Failure(["Booking not found."], 404);
                
                var booking = bookingResult.Data;
                if (booking.CustomerId != customerId || booking.WorkerId != model.RevieweeId)
                {
                    return Result.Failure(["Unauthorized to review this booking."], 401);
                }

                // 2. Prevent Duplicate Reviews
                var existingReview = await _unitOfWork.Reviews.FindFirstAsync(
                    r => r.BookingId == model.BookingId && r.ReviewerId == customerId
                );
                
                if (existingReview.IsSuccess && existingReview.Data != null)
                {
                    return Result.Failure(["You have already reviewed this booking."], 400);
                }

                // 3. Create the Review
                var review = new Review
                {
                    BookingId = model.BookingId,
                    ReviewerId = customerId,
                    RevieweeId = model.RevieweeId,
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
        public async Task<IResult<ReviewUpdateDTO>> GetMyReviewByBookingIdAsync(string bookingId, string userId)
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