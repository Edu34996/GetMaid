using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Core.Abstracts.IServices;
using Core.Concretes.Entities;
using Core.Concretes.DTOs;
using Core.Concretes.Enums;
using Utils.Responses;
using Data.Contexts;
using Utils.Helpers;

namespace Business.Services
{
    public class WorkerService : IWorkerService
    {
        private readonly GetMaidContext _context; 
        private readonly IMapper _mapper;
        private readonly IGeocodingService _geocoding;

        public WorkerService(GetMaidContext context, IMapper mapper, IGeocodingService geocoding)
        {
            _context = context;
            _mapper = mapper;
            _geocoding = geocoding;
        }
        
        public async Task<IResult<WorkerDashboardDTO>> GetDashboardProfileAsync(string workerId)
        {
            try
            {
                var worker = await _context.Workers
                    .FirstOrDefaultAsync(w => w.Id == workerId && !w.IsDeleted);
                if (worker == null)
                {
                    return Result<WorkerDashboardDTO>.Failure(new[] { "Worker profile not found." }, 404);
                }
                var model = _mapper.Map<WorkerDashboardDTO>(worker);
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
                var worker = await _context.Workers
                    .FirstOrDefaultAsync(w => w.Id == workerId && !w.IsDeleted);
                if (worker == null)
                {
                    return Result.Failure("Worker profile not found.", 404);
                }
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
                
                _context.Workers.Update(worker);
                await _context.SaveChangesAsync();
                return Result.Success(200, "Profile updated successfully.");
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
                var openPostings = await _context.JobPostings
                    .Where(j => !j.PostInactive && j.Status == ApplicationStatus.Pending)
                    .ToListAsync();

                var dtos = _mapper.Map<IEnumerable<JobPostingCardDTO>>(openPostings);
        
                return Result<IEnumerable<JobPostingCardDTO>>.Success(dtos, 200);
            }
            catch (Exception ex)
            {
                return Result<IEnumerable<JobPostingCardDTO>>.Failure(new[] { ex.Message }, 500);
            }
        }

        public async Task<IResult> ApplyForJobAsync(string jobPostingId, string workerId)
        {
            try
            {
                var posting = await _context.JobPostings
                    .FirstOrDefaultAsync(j => j.Id == jobPostingId && !j.PostInactive);
                if (posting == null)
                    return Result.Failure("Job posting not found or no longer available.", 404);

                var alreadyApplied = await _context.JobApplications
                    .AnyAsync(a => a.JobPostingId == jobPostingId && a.WorkerId == workerId);
                if (alreadyApplied)
                    return Result.Failure("You have already applied for this job.", 409);

                var application = new JobApplication
                {
                    JobPostingId = jobPostingId,
                    WorkerId = workerId,
                    CreatedAt = DateTime.UtcNow,
                    Status = ApplicationStatus.Pending
                };

                await _context.JobApplications.AddAsync(application);
                await _context.SaveChangesAsync();

                return Result.Success(201, "Application submitted successfully.");
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
                var worker = await _context.Workers
                    .FirstOrDefaultAsync(w => w.Id == workerId && !w.IsDeleted);
                if (worker == null)
                    return Result<IEnumerable<BookingListItemDTO>>.Failure(new[] { "Worker not found." }, 404);

                var bookings = await _context.Bookings
                    .Where(b => b.WorkerId == workerId)
                    .OrderByDescending(b => b.StartDate)
                    .ToListAsync();

                var dtos = _mapper.Map<IEnumerable<BookingListItemDTO>>(bookings);
                return Result<IEnumerable<BookingListItemDTO>>.Success(dtos, 200);
            }
            catch (Exception ex)
            {
                return Result<IEnumerable<BookingListItemDTO>>.Failure(new[] { ex.Message }, 500);
            }
        }

        public async Task<IResult<BookingDetailDTO>> GetBookingDetailsAsync(string bookingId, string workerId)
        {
            try
            {
                var workerExists = await _context.Workers
                    .AnyAsync(w => w.Id == workerId && !w.IsDeleted);

                if (!workerExists)
                {
                    return Result<BookingDetailDTO>.Failure(new[] { "Worker not found." }, 404);
                }

                var booking = await _context.Bookings
                    .Include(b => b.Customer)
                    .Include(b => b.Worker)
                    .FirstOrDefaultAsync(b => b.Id == bookingId && b.WorkerId == workerId);

                if (booking == null)
                {
                    return Result<BookingDetailDTO>.Failure(
                        new[] { "Booking not found or does not belong to this worker." }, 404);
                }

                var dto = _mapper.Map<BookingDetailDTO>(booking);
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
                var workerExists = await _context.Workers
                    .AnyAsync(w => w.Id == workerId && !w.IsDeleted);

                if (!workerExists)
                    return Result.Failure("Worker not found.", 404);

                var booking = await _context.Bookings
                    .FirstOrDefaultAsync(b => b.Id == bookingId && b.WorkerId == workerId);

                if (booking == null)
                    return Result.Failure("Booking not found or does not belong to this worker.", 404);

                // Only allow response when booking is still pending.
                if (booking.Status != ApplicationStatus.Pending)
                    return Result.Failure("This booking has already been responded to.", 409);

                booking.Status = isConfirmed ? ApplicationStatus.Accepted : ApplicationStatus.Rejected;
                booking.UpdatedAt = DateTime.UtcNow; // remove if your entity doesn't have UpdatedAt

                _context.Bookings.Update(booking);
                await _context.SaveChangesAsync();

                return Result.Success(
                    200,
                    isConfirmed ? "Booking confirmed successfully." : "Booking rejected successfully.");
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

                var workerExists = await _context.Workers
                    .AnyAsync(w => w.Id == workerId && !w.IsDeleted);

                if (!workerExists) 
                    return Result.Failure("Worker not found.", 404);

                // Assumes ReviewCreateDTO has BookingId.
                if (string.IsNullOrWhiteSpace(model.BookingId))
                    return Result.Failure("BookingId is required.", 400);

                var booking = await _context.Bookings
                    .FirstOrDefaultAsync(b => b.Id == model.BookingId && b.WorkerId == workerId);

                if (booking == null)
                    return Result.Failure("Booking not found or does not belong to this worker.", 404);

                // Optional business rule: allow reviews only after accepted/completed bookings.
                // Keep/adjust statuses to your real enum values.
                if (booking.Status != ApplicationStatus.Accepted && booking.Status != ApplicationStatus.Completed)
                    return Result.Failure("You can only review a customer after the booking is accepted/completed.", 409);

                var alreadyReviewed = await _context.Reviews
                    .AnyAsync(r => r.BookingId == model.BookingId && r.ReviewerId == workerId);

                if (alreadyReviewed)
                    return Result.Failure("You have already reviewed this customer for this booking.", 409);

                // Assumes your AutoMapper maps ReviewCreateDTO -> Review.
                var review = _mapper.Map<Review>(model);
                review.ReviewerId = workerId;
                review.RevieweeId = booking.CustomerId; // review target customer from booking
                review.BookingId = booking.Id;
                review.CreatedAt = DateTime.UtcNow;

                await _context.Reviews.AddAsync(review);
                await _context.SaveChangesAsync();

                return Result.Success(201, "Review submitted successfully.");
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

                var workerExists = await _context.Workers
                    .AnyAsync(w => w.Id == userId && !w.IsDeleted);

                if (!workerExists)
                    return Result<ReviewUpdateDTO>.Failure(new[] { "Worker not found." }, 404);

                var bookingExistsForWorker = await _context.Bookings
                    .AnyAsync(b => b.Id == bookingId && b.WorkerId == userId);

                if (!bookingExistsForWorker)
                    return Result<ReviewUpdateDTO>.Failure(
                        new[] { "Booking not found or does not belong to this worker." }, 404);

                var review = await _context.Reviews
                    .FirstOrDefaultAsync(r => r.BookingId == bookingId && r.ReviewerId == userId);

                if (review == null)
                    return Result<ReviewUpdateDTO>.Failure(
                        new[] { "Review not found for this booking." }, 404);

                var dto = _mapper.Map<ReviewUpdateDTO>(review);
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
                    return Result.Failure("BookingId is required.", 400);

                var workerExists = await _context.Workers
                    .AnyAsync(w => w.Id == userId && !w.IsDeleted);

                if (!workerExists)
                    return Result.Failure("Worker not found.", 404);

                var bookingExistsForWorker = await _context.Bookings
                    .AnyAsync(b => b.Id == model.Id && b.WorkerId == userId);

                if (!bookingExistsForWorker)
                    return Result.Failure("Booking not found or does not belong to this worker.", 404);

                var review = await _context.Reviews
                    .FirstOrDefaultAsync(r => r.BookingId == model.Id && r.ReviewerId == userId);

                if (review == null)
                    return Result.Failure("Review not found for this booking.", 404);

                // Update only editable fields.
                _mapper.Map(model, review);

                // Keep ownership and linkage immutable.
                review.ReviewerId = userId;
                review.BookingId = model.Id;
                review.UpdatedAt = DateTime.UtcNow; // remove if your entity doesn't have UpdatedAt

                _context.Reviews.Update(review);
                await _context.SaveChangesAsync();

                return Result.Success(200, "Review updated successfully.");
            }
            catch (Exception ex)
            {
                return Result.Failure(new[] { ex.Message }, 500);
            }
        }
    }
}