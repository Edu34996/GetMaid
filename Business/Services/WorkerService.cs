using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Core.Abstracts;
using Core.Abstracts.IServices;
using Core.Concretes.Entities;
using Core.Concretes.DTOs;
using Core.Concretes.Enums;
using Utils.Helpers;
using Utils.Responses;

namespace Business.Services
{
    public class WorkerService : IWorkerService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IGeocodingService _geocoding;

        public WorkerService(IUnitOfWork unitOfWork, IMapper mapper, IGeocodingService geocoding)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _geocoding = geocoding;
        }

        public async Task<IResult<WorkerDashboardDTO>> GetDashboardProfileAsync(string workerId)
        {
            try
            {
                var workerResult = await _unitOfWork.Workers.FindFirstAsync(
                    w => w.Id == workerId && !w.IsDeleted
                );

                if (!workerResult.IsSuccess || workerResult.Data == null)
                    return Result<WorkerDashboardDTO>.Failure(new[] { "Worker profile not found." }, 404);

                var model = _mapper.Map<WorkerDashboardDTO>(workerResult.Data);
                return Result<WorkerDashboardDTO>.Success(model, 200);
            }
            catch (Exception ex)
            {
                return Result<WorkerDashboardDTO>.Failure(new[] { ex.Message }, 500, "Error loading dashboard");
            }
        }

        public async Task<IResult> UpdateProfileAsync(string workerId, WorkerProfileUpdateDTO model)
        {
            try
            {
                var workerResult = await _unitOfWork.Workers.FindFirstAsync(
                    w => w.Id == workerId && !w.IsDeleted
                );

                if (!workerResult.IsSuccess || workerResult.Data == null)
                    return Result.Failure("Worker profile not found.", 404);

                var worker = workerResult.Data;
                _mapper.Map(model, worker);

                var geoQuery = string.IsNullOrWhiteSpace(worker.Address)
                    ? worker.City
                    : $"{worker.Address}, {worker.City}";

                if (!string.IsNullOrWhiteSpace(geoQuery))
                {
                    var (lat, lon) = await _geocoding.GeocodeAsync(geoQuery);
                    if (lat.HasValue) worker.Latitude = lat.Value;
                    if (lon.HasValue) worker.Longitude = lon.Value;
                }

                await _unitOfWork.Workers.UpdateAsync(worker);
                var commitResult = await _unitOfWork.CommitAsync();

                return commitResult.IsSuccess
                    ? Result.Success(200, "Profile updated successfully.")
                    : Result.Failure(commitResult.Messages ?? new[] { "Failed to save profile." }, 500);
            }
            catch (Exception ex)
            {
                return Result.Failure(new[] { ex.Message }, 500);
            }
        }

        public async Task<IResult<IEnumerable<JobPostingCardDTO>>> GetOpenJobPostingsAsync(string workerId)
        {
            try
            {
                var postingsResult = await _unitOfWork.JobPostings.FindManyAsync(
                    j => !j.PostInactive && j.Status == ApplicationStatus.Pending
                );

                if (!postingsResult.IsSuccess || postingsResult.Data == null)
                    return Result<IEnumerable<JobPostingCardDTO>>.Failure(
                        postingsResult.Messages ?? new[] { "Failed to fetch job postings." }, 500);

                var dtos = _mapper.Map<IEnumerable<JobPostingCardDTO>>(postingsResult.Data);
                return Result<IEnumerable<JobPostingCardDTO>>.Success(dtos, 200);
            }
            catch (Exception ex)
            {
                return Result<IEnumerable<JobPostingCardDTO>>.Failure(new[] { ex.Message }, 500);
            }
        }

        // NEW: Completed jobs this worker has applied to and finished
        public async Task<IResult<IEnumerable<JobPostingCardDTO>>> GetMyCompletedJobsAsync(string workerId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(workerId))
                    return Result<IEnumerable<JobPostingCardDTO>>.Failure(new[] { "WorkerId is required." }, 400);

                // Pull applications for this worker with completed status
                var completedAppsResult = await _unitOfWork.JobApplications.FindManyAsync(
                    a => a.WorkerId == workerId && a.Status == ApplicationStatus.Completed,
                    "JobPosting"
                );

                if (!completedAppsResult.IsSuccess || completedAppsResult.Data == null)
                    return Result<IEnumerable<JobPostingCardDTO>>.Failure(
                        completedAppsResult.Messages ?? new[] { "Failed to fetch completed jobs." },
                        completedAppsResult.StatusCode);

                var completedJobCards = completedAppsResult.Data
                    .Where(a => a.JobPosting != null)
                    .Select(a => a.JobPosting!)
                    .GroupBy(j => j.Id)
                    .Select(g => g.First())
                    .Select(j => new JobPostingCardDTO
                    {
                        Id = j.Id,
                        Title = j.Title,
                        City = j.City,
                        Budget = j.Budget,
                        ServiceTypes = j.ServiceTypes?.ToList() ?? new List<ServiceType>(),
                        StartDate = j.StartDate
                    })
                    .OrderByDescending(j => j.StartDate)
                    .ToList();

                return Result<IEnumerable<JobPostingCardDTO>>.Success(completedJobCards, 200);
            }
            catch (Exception ex)
            {
                return Result<IEnumerable<JobPostingCardDTO>>.Failure(new[] { ex.Message }, 500);
            }
        }

        public async Task<IResult> ApplyForJobAsync(string jobPostingId, string workerId, JobApplicationCreateDTO? model = null)
        {
            try
            {
                var postingResult = await _unitOfWork.JobPostings.FindByIdAsync(jobPostingId);
                if (!postingResult.IsSuccess || postingResult.Data == null || postingResult.Data.PostInactive)
                    return Result.Failure("Job posting not found or no longer available.", 404);

                var existingAppResult = await _unitOfWork.JobApplications.FindFirstAsync(a =>
                    a.JobPostingId == jobPostingId && a.WorkerId == workerId && a.Status == ApplicationStatus.Pending
                );

                if (existingAppResult.IsSuccess && existingAppResult.Data != null)
                    return Result.Failure("You have already applied for this job.", 409);

                var application = new JobApplication
                {
                    JobPostingId = jobPostingId,
                    WorkerId = workerId,
                    Status = ApplicationStatus.Pending,
                    AppliedAt = DateTime.UtcNow,
                    MessageToCustomer = model?.MessageToCustomer,
                    SoonestAvailableStartDate = model?.SoonestAvailableStartDate,
                    IsCurrentlyWorking = model?.IsCurrentlyWorking ?? false,
                    QuestionsAboutWork = model?.QuestionsAboutWork
                };

                var createResult = await _unitOfWork.JobApplications.CreateAsync(application);
                if (!createResult.IsSuccess)
                    return Result.Failure(createResult.Messages ?? new[] { "Failed to submit application." }, 500);

                var commitResult = await _unitOfWork.CommitAsync();
                return commitResult.IsSuccess
                    ? Result.Success(201, "Application submitted successfully.")
                    : Result.Failure(commitResult.Messages ?? new[] { "Failed to save application." }, 500);
            }
            catch (Exception ex)
            {
                return Result.Failure(new[] { ex.Message }, 500);
            }
        }

        public async Task<IResult<IEnumerable<BookingListItemDTO>>> GetMyBookingsAsync(string workerId)
        {
            try
            {
                var workerResult = await _unitOfWork.Workers.FindFirstAsync(
                    w => w.Id == workerId && !w.IsDeleted
                );

                if (!workerResult.IsSuccess || workerResult.Data == null)
                    return Result<IEnumerable<BookingListItemDTO>>.Failure(new[] { "Worker not found." }, 404);

                var bookingsResult = await _unitOfWork.Bookings.FindManyAsync(
                    b => b.WorkerId == workerId,
                    "Customer"
                );

                if (!bookingsResult.IsSuccess || bookingsResult.Data == null)
                    return Result<IEnumerable<BookingListItemDTO>>.Failure(
                        bookingsResult.Messages ?? new[] { "Failed to fetch bookings." }, 500);

                var dtos = _mapper.Map<IEnumerable<BookingListItemDTO>>(
                    bookingsResult.Data.OrderByDescending(b => b.StartDate)
                );

                return Result<IEnumerable<BookingListItemDTO>>.Success(dtos, 200);
            }
            catch (Exception ex)
            {
                return Result<IEnumerable<BookingListItemDTO>>.Failure(new[] { ex.Message }, 500);
            }
        }

        
        public async Task<IResult<IEnumerable<JobPostingCardDTO>>> GetMyAppliedJobsAsync(string workerId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(workerId))
                    return Result<IEnumerable<JobPostingCardDTO>>.Failure(new[] { "WorkerId is required." }, 400);

                // Get all applications for this worker except completed ones
                var applicationsResult = await _unitOfWork.JobApplications.FindManyAsync(
                    a => a.WorkerId == workerId && a.Status != ApplicationStatus.Completed,
                    "JobPosting"
                );

                if (!applicationsResult.IsSuccess || applicationsResult.Data == null)
                    return Result<IEnumerable<JobPostingCardDTO>>.Failure(
                        applicationsResult.Messages ?? new[] { "Failed to fetch applied jobs." },
                        applicationsResult.StatusCode);

                var appliedJobs = applicationsResult.Data
                    .Where(a => a.JobPosting != null)
                    .Select(a => a.JobPosting!)
                    .GroupBy(j => j.Id)
                    .Select(g => g.First())
                    .Select(j => new JobPostingCardDTO
                    {
                        Id = j.Id,
                        Title = j.Title,
                        City = j.City,
                        Budget = j.Budget,
                        ServiceTypes = j.ServiceTypes?.ToList() ?? new List<ServiceType>(),
                        StartDate = j.StartDate
                    })
                    .OrderByDescending(j => j.StartDate)
                    .ToList();

                return Result<IEnumerable<JobPostingCardDTO>>.Success(appliedJobs, 200);
            }
            catch (Exception ex)
            {
                return Result<IEnumerable<JobPostingCardDTO>>.Failure(new[] { ex.Message }, 500);
            }
        }
        
        public async Task<IResult<BookingDetailDTO>> GetBookingDetailsAsync(string bookingId, string workerId)
        {
            try
            {
                var workerExistsResult = await _unitOfWork.Workers.FindFirstAsync(
                    w => w.Id == workerId && !w.IsDeleted
                );

                if (!workerExistsResult.IsSuccess || workerExistsResult.Data == null)
                    return Result<BookingDetailDTO>.Failure(new[] { "Worker not found." }, 404);

                var bookingResult = await _unitOfWork.Bookings.FindFirstAsync(
                    b => b.Id == bookingId && b.WorkerId == workerId
                );

                if (!bookingResult.IsSuccess || bookingResult.Data == null)
                    return Result<BookingDetailDTO>.Failure(
                        new[] { "Booking not found or does not belong to this worker." }, 404);

                var dto = _mapper.Map<BookingDetailDTO>(bookingResult.Data);
                return Result<BookingDetailDTO>.Success(dto, 200);
            }
            catch (Exception ex)
            {
                return Result<BookingDetailDTO>.Failure(new[] { ex.Message }, 500, "Error loading booking details");
            }
        }

        public async Task<IResult> RespondToBookingAsync(string bookingId, string workerId, bool isConfirmed)
        {
            try
            {
                var workerExistsResult = await _unitOfWork.Workers.FindFirstAsync(
                    w => w.Id == workerId && !w.IsDeleted
                );

                if (!workerExistsResult.IsSuccess || workerExistsResult.Data == null)
                    return Result.Failure("Worker not found.", 404);

                var bookingResult = await _unitOfWork.Bookings.FindFirstAsync(
                    b => b.Id == bookingId && b.WorkerId == workerId
                );

                if (!bookingResult.IsSuccess || bookingResult.Data == null)
                    return Result.Failure("Booking not found or does not belong to this worker.", 404);

                var booking = bookingResult.Data;

                if (booking.Status != ApplicationStatus.Pending)
                    return Result.Failure("This booking has already been responded to.", 409);

                booking.Status = isConfirmed ? ApplicationStatus.Accepted : ApplicationStatus.Rejected;
                booking.UpdatedAt = DateTime.UtcNow;

                await _unitOfWork.Bookings.UpdateAsync(booking);
                var commitResult = await _unitOfWork.CommitAsync();

                return commitResult.IsSuccess
                    ? Result.Success(200, isConfirmed ? "Booking confirmed successfully." : "Booking rejected successfully.")
                    : Result.Failure(commitResult.Messages ?? new[] { "Failed to update booking." }, 500);
            }
            catch (Exception ex)
            {
                return Result.Failure(new[] { ex.Message }, 500);
            }
        }

        public async Task<IResult> LeaveReviewForCustomerAsync(ReviewCreateDTO model, string workerId)
        {
            try
            {
                if (model == null)
                    return Result.Failure("Review data is required.", 400);

                var workerExistsResult = await _unitOfWork.Workers.FindFirstAsync(
                    w => w.Id == workerId && !w.IsDeleted
                );

                if (!workerExistsResult.IsSuccess || workerExistsResult.Data == null)
                    return Result.Failure("Worker not found.", 404);

                if (string.IsNullOrWhiteSpace(model.BookingId))
                    return Result.Failure("BookingId is required.", 400);

                var bookingResult = await _unitOfWork.Bookings.FindFirstAsync(
                    b => b.Id == model.BookingId && b.WorkerId == workerId
                );

                if (!bookingResult.IsSuccess || bookingResult.Data == null)
                    return Result.Failure("Booking not found or does not belong to this worker.", 404);

                var booking = bookingResult.Data;

                if (booking.Status != ApplicationStatus.Accepted && booking.Status != ApplicationStatus.Completed)
                    return Result.Failure("You can only review a customer after the booking is accepted/completed.", 409);

                var alreadyReviewedResult = await _unitOfWork.Reviews.FindFirstAsync(
                    r => r.BookingId == model.BookingId && r.ReviewerId == workerId
                );

                if (alreadyReviewedResult.IsSuccess && alreadyReviewedResult.Data != null)
                    return Result.Failure("You have already reviewed this customer for this booking.", 409);

                var review = _mapper.Map<Review>(model);
                review.ReviewerId = workerId;
                review.RevieweeId = booking.CustomerId;
                review.BookingId = booking.Id;
                review.CreatedAt = DateTime.UtcNow;

                var createResult = await _unitOfWork.Reviews.CreateAsync(review);
                if (!createResult.IsSuccess)
                    return Result.Failure(createResult.Messages ?? new[] { "Failed to create review." }, 500);

                var commitResult = await _unitOfWork.CommitAsync();
                return commitResult.IsSuccess
                    ? Result.Success(201, "Review submitted successfully.")
                    : Result.Failure(commitResult.Messages ?? new[] { "Failed to save review." }, 500);
            }
            catch (Exception ex)
            {
                return Result.Failure(new[] { ex.Message }, 500);
            }
        }

        public async Task<IResult<ReviewUpdateDTO>> GetMyReviewByBookingIdAsync(string bookingId, string userId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(bookingId))
                    return Result<ReviewUpdateDTO>.Failure(new[] { "BookingId is required." }, 400);

                var workerExistsResult = await _unitOfWork.Workers.FindFirstAsync(
                    w => w.Id == userId && !w.IsDeleted
                );

                if (!workerExistsResult.IsSuccess || workerExistsResult.Data == null)
                    return Result<ReviewUpdateDTO>.Failure(new[] { "Worker not found." }, 404);

                var bookingExistsResult = await _unitOfWork.Bookings.FindFirstAsync(
                    b => b.Id == bookingId && b.WorkerId == userId
                );

                if (!bookingExistsResult.IsSuccess || bookingExistsResult.Data == null)
                    return Result<ReviewUpdateDTO>.Failure(
                        new[] { "Booking not found or does not belong to this worker." }, 404);

                var reviewResult = await _unitOfWork.Reviews.FindFirstAsync(
                    r => r.BookingId == bookingId && r.ReviewerId == userId
                );

                if (!reviewResult.IsSuccess || reviewResult.Data == null)
                    return Result<ReviewUpdateDTO>.Failure(
                        new[] { "Review not found for this booking." }, 404);

                var dto = _mapper.Map<ReviewUpdateDTO>(reviewResult.Data);
                return Result<ReviewUpdateDTO>.Success(dto, 200);
            }
            catch (Exception ex)
            {
                return Result<ReviewUpdateDTO>.Failure(new[] { ex.Message }, 500, "Error loading review");
            }
        }

        public async Task<IResult> UpdateReviewAsync(ReviewUpdateDTO model, string userId)
        {
            try
            {
                if (model == null)
                    return Result.Failure("Review update data is required.", 400);

                if (string.IsNullOrWhiteSpace(model.Id))
                    return Result.Failure("Review ID is required.", 400);

                var workerExistsResult = await _unitOfWork.Workers.FindFirstAsync(
                    w => w.Id == userId && !w.IsDeleted
                );

                if (!workerExistsResult.IsSuccess || workerExistsResult.Data == null)
                    return Result.Failure("Worker not found.", 404);

                var bookingExistsResult = await _unitOfWork.Bookings.FindFirstAsync(
                    b => b.Id == model.Id && b.WorkerId == userId
                );

                if (!bookingExistsResult.IsSuccess || bookingExistsResult.Data == null)
                    return Result.Failure("Booking not found or does not belong to this worker.", 404);

                var reviewResult = await _unitOfWork.Reviews.FindFirstAsync(
                    r => r.BookingId == model.Id && r.ReviewerId == userId
                );

                if (!reviewResult.IsSuccess || reviewResult.Data == null)
                    return Result.Failure("Review not found for this booking.", 404);

                var review = reviewResult.Data;

                _mapper.Map(model, review);

                review.ReviewerId = userId;
                review.BookingId = model.Id;
                review.UpdatedAt = DateTime.UtcNow;

                await _unitOfWork.Reviews.UpdateAsync(review);
                var commitResult = await _unitOfWork.CommitAsync();

                return commitResult.IsSuccess
                    ? Result.Success(200, "Review updated successfully.")
                    : Result.Failure(commitResult.Messages ?? new[] { "Failed to update review." }, 500);
            }
            catch (Exception ex)
            {
                return Result.Failure(new[] { ex.Message }, 500);
            }
        }
    }
}