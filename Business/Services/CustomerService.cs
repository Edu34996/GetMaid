using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Core.Abstracts;
using Core.Abstracts.IServices;
using Core.Concretes.DTOs;
using Core.Concretes.Entities;
using Core.Concretes.Enums;
using Utils.Helpers;
using Utils.Responses;

namespace Business.Services
{
    public class CustomerService : ICustomerService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IGeocodingService _geocoding;

        public CustomerService(IUnitOfWork unitOfWork, IMapper mapper, IGeocodingService geocoding)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _geocoding = geocoding;
        }

        public async Task<IResult<CustomerDashboardDTO>> GetProfileAsync(string customerId)
        {
            var customerResult = await _unitOfWork.Customers.FindByIdAsync(customerId);

            if (!customerResult.IsSuccess || customerResult.Data == null)
                return Result<CustomerDashboardDTO>.Failure(new[] { "Customer profile not found." }, 404);

            var dashboardDto = _mapper.Map<CustomerDashboardDTO>(customerResult.Data);
            return Result<CustomerDashboardDTO>.Success(dashboardDto, 200);
        }

        public async Task<IResult> UpdateProfileAsync(string customerId, CustomerProfileUpdateDTO model)
        {
            var customerResult = await _unitOfWork.Customers.FindByIdAsync(customerId);

            if (!customerResult.IsSuccess || customerResult.Data == null)
                return Result.Failure(new[] { "Customer not found to update." }, 404);

            var customer = customerResult.Data;
            _mapper.Map(model, customer);

            var geoQuery = string.IsNullOrWhiteSpace(customer.Address)
                ? customer.City
                : $"{customer.Address}, {customer.City}";

            if (!string.IsNullOrWhiteSpace(geoQuery))
            {
                var (lat, lon) = await _geocoding.GeocodeAsync(geoQuery);
                if (lat.HasValue) customer.Latitude = lat.Value;
                if (lon.HasValue) customer.Longitude = lon.Value;
            }

            await _unitOfWork.Customers.UpdateAsync(customer);
            return await _unitOfWork.CommitAsync();
        }

        public async Task<IResult<string>> CreateServiceRequestAsync(ServiceRequestCreateDTO model, string customerId)
        {
            try
            {
                if (model == null)
                    return Result<string>.Failure(new[] { "Request payload is required." }, 400);

                if (string.IsNullOrWhiteSpace(customerId))
                    return Result<string>.Failure(new[] { "Customer ID is required." }, 400);

                if (model.EndDate < model.StartDate)
                    return Result<string>.Failure(new[] { "End date must be on or after start date." }, 400);

                if (model.ServiceTypes == null || !model.ServiceTypes.Any())
                    return Result<string>.Failure(new[] { "At least one service type is required." }, 400);

                var geoQuery = string.IsNullOrWhiteSpace(model.Address)
                    ? model.City
                    : $"{model.Address}, {model.City}";

                double? reqLat = null;
                double? reqLon = null;

                if (!string.IsNullOrWhiteSpace(geoQuery))
                {
                    var (lat, lon) = await _geocoding.GeocodeAsync(geoQuery);
                    reqLat = lat;
                    reqLon = lon;
                }

                if (!string.IsNullOrWhiteSpace(model.TargetWorkerId))
                {
                    var workerCheck = await _unitOfWork.Workers.FindByIdAsync(model.TargetWorkerId);
                    if (!workerCheck.IsSuccess || workerCheck.Data == null)
                        return Result<string>.Failure(new[] { "The selected worker could not be found." }, 404);

                    var booking = new Booking
                    {
                        CustomerId = customerId,
                        WorkerId = model.TargetWorkerId,
                        Title = model.Title,
                        Description = model.Description,
                        Requirements = model.Requirements,
                        City = model.City,
                        Address = model.Address,
                        Latitude = reqLat,
                        Longitude = reqLon,
                        StartDate = model.StartDate,
                        EndDate = model.EndDate,
                        EstimatedHours = model.EstimatedHours,
                        Budget = model.Budget,
                        RequireNonSmoker = model.RequireNonSmoker,
                        ServiceTypes = model.ServiceTypes.ToList(),
                        WorkArrangement = model.WorkArrangement,
                        CommitmentPreference = model.CommitmentPreference,
                        RequiredSkills = model.RequiredSkills?.ToList() ?? new List<Skill>(),
                        Status = ApplicationStatus.Pending,
                        BookingInactive = false
                    };

                    var createBookingResult = await _unitOfWork.Bookings.CreateAsync(booking);
                    if (!createBookingResult.IsSuccess)
                        return Result<string>.Failure(
                            createBookingResult.Messages ?? new[] { "Failed to create booking." },
                            createBookingResult.StatusCode);

                    var commitBooking = await _unitOfWork.CommitAsync();
                    if (!commitBooking.IsSuccess)
                        return Result<string>.Failure(
                            commitBooking.Messages ?? new[] { "Failed to save booking." },
                            commitBooking.StatusCode);

                    return Result<string>.Success(booking.Id, 201, "Booking created successfully.");
                }

                var jobPosting = new JobPosting
                {
                    CustomerId = customerId,
                    Title = model.Title,
                    Description = model.Description,
                    Requirements = model.Requirements,
                    City = model.City,
                    Address = model.Address,
                    Latitude = reqLat,
                    Longitude = reqLon,
                    StartDate = model.StartDate,
                    EndDate = model.EndDate,
                    EstimatedHours = model.EstimatedHours,
                    Budget = model.Budget,
                    RequireNonSmoker = model.RequireNonSmoker,
                    ServiceTypes = model.ServiceTypes.ToList(),
                    WorkArrangement = model.WorkArrangement,
                    CommitmentPreference = model.CommitmentPreference,
                    RequiredSkills = model.RequiredSkills?.ToList() ?? new List<Skill>(),
                    Status = ApplicationStatus.Pending,
                    PostInactive = false
                };

                var createJobResult = await _unitOfWork.JobPostings.CreateAsync(jobPosting);
                if (!createJobResult.IsSuccess)
                    return Result<string>.Failure(
                        createJobResult.Messages ?? new[] { "Failed to create job posting." },
                        createJobResult.StatusCode);

                var commitJob = await _unitOfWork.CommitAsync();
                if (!commitJob.IsSuccess)
                    return Result<string>.Failure(
                        commitJob.Messages ?? new[] { "Failed to save job posting." },
                        commitJob.StatusCode);

                return Result<string>.Success(jobPosting.Id, 201, "Job posting created successfully.");
            }
            catch (Exception ex)
            {
                return Result<string>.Failure(new[] { ex.Message }, 500);
            }
        }

        public async Task<IResult<IEnumerable<JobPostingCardDTO>>> GetMyJobPostingsAsync(string customerId)
        {
            try
            {
                var repositoryResult = await _unitOfWork.JobPostings.FindManyAsync(
                    j => j.CustomerId == customerId,
                    "Customer"
                );

                if (!repositoryResult.IsSuccess || repositoryResult.Data == null)
                    return Result<IEnumerable<JobPostingCardDTO>>.Failure(
                        repositoryResult.Messages ?? new[] { "Failed to retrieve job postings." },
                        repositoryResult.StatusCode);

                var postingDtos = repositoryResult.Data.Select(j => new JobPostingCardDTO
                {
                    Id = j.Id,
                    Title = j.Title,
                    City = j.City,
                    Budget = j.Budget,
                    ServiceTypes = j.ServiceTypes?.ToList() ?? new List<ServiceType>(),
                    StartDate = j.StartDate
                }).ToList();

                return Result<IEnumerable<JobPostingCardDTO>>.Success(postingDtos, 200);
            }
            catch (Exception ex)
            {
                return Result<IEnumerable<JobPostingCardDTO>>.Failure(new[] { ex.Message }, 500);
            }
        }

        public async Task<IResult<JobPostingDetailDTO>> GetJobPostingDetailsAsync(string jobId, string customerId)
        {
            try
            {
                var jobResult = await _unitOfWork.JobPostings.FindManyAsync(
                    j => j.Id == jobId && !j.PostInactive,
                    "Customer"
                );

                var job = jobResult.Data?.FirstOrDefault();

                if (!jobResult.IsSuccess || job == null)
                    return Result<JobPostingDetailDTO>.Failure(new[] { "Job posting not found." }, 404);

                if (job.CustomerId != customerId)
                    return Result<JobPostingDetailDTO>.Failure(new[] { "Unauthorized access." }, 401);

                var dto = new JobPostingDetailDTO
                {
                    Id = job.Id,
                    Title = job.Title,
                    Description = job.Description,
                    Requirements = job.Requirements,
                    City = job.City,
                    Address = job.Address,
                    Latitude = job.Latitude,
                    Longitude = job.Longitude,
                    StartDate = job.StartDate,
                    EndDate = job.EndDate,
                    EstimatedHours = job.EstimatedHours,
                    Budget = job.Budget,
                    RequireNonSmoker = job.RequireNonSmoker,
                    Status = job.Status,
                    WorkArrangement = job.WorkArrangement,
                    CommitmentPreference = job.CommitmentPreference,
                    ServiceTypes = job.ServiceTypes?.ToList() ?? new List<ServiceType>(),
                    RequiredSkills = job.RequiredSkills?.ToList() ?? new List<Skill>(),
                    CustomerId = job.CustomerId,
                    CustomerName = job.Customer != null
                        ? $"{job.Customer.FirstName} {job.Customer.LastName}"
                        : "Unknown"
                };

                return Result<JobPostingDetailDTO>.Success(dto, 200);
            }
            catch (Exception ex)
            {
                return Result<JobPostingDetailDTO>.Failure(new[] { ex.Message }, 500);
            }
        }

        public async Task<IResult<IEnumerable<BookingListItemDTO>>> GetMyBookingsAsync(string customerId)
        {
            try
            {
                var bookingsResult = await _unitOfWork.Bookings.FindManyAsync(
                    b => b.CustomerId == customerId && !b.BookingInactive,
                    "Worker"
                );

                if (!bookingsResult.IsSuccess || bookingsResult.Data == null)
                    return Result<IEnumerable<BookingListItemDTO>>.Failure(
                        bookingsResult.Messages ?? new[] { "Failed to retrieve bookings." },
                        bookingsResult.StatusCode);

                var bookingDtos = bookingsResult.Data
                    .OrderByDescending(b => b.StartDate)
                    .Select(b => new BookingListItemDTO
                    {
                        Id = b.Id,
                        WorkerId = b.WorkerId ?? string.Empty,
                        WorkerName = b.Worker != null
                            ? $"{b.Worker.FirstName} {b.Worker.LastName}".Trim()
                            : "Unassigned",
                        WorkerProfilePictureUrl = b.Worker?.ProfilePictureUrl,
                        Title = b.Title,
                        City = b.City,
                        Budget = b.Budget,
                        EstimatedHours = b.EstimatedHours,
                        StartDate = b.StartDate,
                        Status = b.Status.ToString()
                    })
                    .ToList();

                return Result<IEnumerable<BookingListItemDTO>>.Success(bookingDtos, 200);
            }
            catch (Exception ex)
            {
                return Result<IEnumerable<BookingListItemDTO>>.Failure(new[] { ex.Message }, 500);
            }
        }

        public async Task<IResult<BookingDetailDTO>> GetBookingDetailsAsync(string bookingId, string customerId)
        {
            try
            {
                var bookingResult = await _unitOfWork.Bookings.FindManyAsync(
                    b => b.Id == bookingId && !b.BookingInactive,
                    "Customer", "Worker"
                );

                var booking = bookingResult.Data?.FirstOrDefault();

                if (!bookingResult.IsSuccess || booking == null)
                    return Result<BookingDetailDTO>.Failure(new[] { "Booking not found." }, 404);

                if (booking.CustomerId != customerId)
                    return Result<BookingDetailDTO>.Failure(new[] { "Unauthorized access." }, 401);

                var dto = new BookingDetailDTO
                {
                    Id = booking.Id,
                    Title = booking.Title,
                    Description = booking.Description,
                    Requirements = booking.Requirements,
                    City = booking.City,
                    Address = booking.Address,
                    Latitude = booking.Latitude,
                    Longitude = booking.Longitude,
                    CustomerId = booking.CustomerId,
                    CustomerName = booking.Customer != null
                        ? $"{booking.Customer.FirstName} {booking.Customer.LastName}"
                        : "Unknown",
                    CustomerAddress = booking.Customer?.Address ?? string.Empty,
                    CustomerPhoneNumber = booking.Customer?.PhoneNumber,
                    CustomerProfilePictureUrl = booking.Customer?.ProfilePictureUrl,
                    WorkerId = booking.WorkerId ?? string.Empty,
                    WorkerName = booking.Worker != null
                        ? $"{booking.Worker.FirstName} {booking.Worker.LastName}".Trim()
                        : null,
                    WorkerPhoneNumber = booking.Worker?.PhoneNumber,
                    WorkerProfilePictureUrl = booking.Worker?.ProfilePictureUrl,
                    StartDate = booking.StartDate,
                    EndDate = booking.EndDate,
                    EstimatedHours = booking.EstimatedHours,
                    Budget = booking.Budget,
                    RequireNonSmoker = booking.RequireNonSmoker,
                    WorkArrangement = booking.WorkArrangement,
                    CommitmentPreference = booking.CommitmentPreference,
                    ServiceTypes = booking.ServiceTypes?.ToList() ?? new List<ServiceType>(),
                    RequiredSkills = booking.RequiredSkills?.ToList() ?? new List<Skill>(),
                    Status = booking.Status,
                    BookingInactive = booking.BookingInactive
                };

                return Result<BookingDetailDTO>.Success(dto, 200);
            }
            catch (Exception ex)
            {
                return Result<BookingDetailDTO>.Failure(new[] { ex.Message }, 500);
            }
        }

        public async Task<IResult> AddChildAsync(ChildDTO model, string customerId)
        {
            var child = _mapper.Map<Child>(model);
            child.CustomerId = customerId;

            await _unitOfWork.Children.CreateAsync(child);
            return await _unitOfWork.CommitAsync();
        }

        public async Task<IResult<List<ChildDTO>>> GetMyChildrenAsync(string customerId)
        {
            var childrenResult = await _unitOfWork.Children.FindManyAsync(c => c.CustomerId == customerId);

            if (!childrenResult.IsSuccess || childrenResult.Data == null)
                return Result<List<ChildDTO>>.Failure(new[] { "No children found." }, 404);

            var childDtos = _mapper.Map<List<ChildDTO>>(childrenResult.Data);
            return Result<List<ChildDTO>>.Success(childDtos, 200);
        }

        public async Task<IResult<ChildDTO>> GetChildByIdAsync(string childId, string customerId)
        {
            var childResult = await _unitOfWork.Children.FindByIdAsync(childId);

            if (!childResult.IsSuccess || childResult.Data == null)
                return Result<ChildDTO>.Failure(new[] { "Child not found." }, 404);

            if (childResult.Data.CustomerId != customerId)
                return Result<ChildDTO>.Failure(new[] { "Unauthorized action." }, 401);

            var dto = _mapper.Map<ChildDTO>(childResult.Data);
            return Result<ChildDTO>.Success(dto, 200);
        }

        public async Task<IResult> UpdateChildAsync(ChildDTO model, string customerId)
        {
            var childResult = await _unitOfWork.Children.FindByIdAsync(model.Id);

            if (!childResult.IsSuccess || childResult.Data == null)
                return Result.Failure(new[] { "Child not found." }, 404);

            if (childResult.Data.CustomerId != customerId)
                return Result.Failure(new[] { "Unauthorized action." }, 401);

            var child = childResult.Data;
            _mapper.Map(model, child);

            await _unitOfWork.Children.UpdateAsync(child);
            return await _unitOfWork.CommitAsync();
        }

        public async Task<IResult> RemoveChildAsync(string childId, string customerId)
        {
            var childResult = await _unitOfWork.Children.FindByIdAsync(childId);

            if (!childResult.IsSuccess || childResult.Data == null)
                return Result.Failure(new[] { "Child not found." }, 404);

            if (childResult.Data.CustomerId != customerId)
                return Result.Failure(new[] { "Unauthorized action." }, 401);

            await _unitOfWork.Children.DeleteAsync(childResult.Data);
            return await _unitOfWork.CommitAsync();
        }

        public async Task<IResult<IEnumerable<WorkerCardDTO>>> BrowseWorkersAsync(WorkerSearchFilterDTO filter, string customerId)
        {
            try
            {
                filter ??= new WorkerSearchFilterDTO();

                // Pull workers using existing filters first
                var workersResult = await _unitOfWork.Workers.FindManyAsync(
                    w => !w.IsDeleted &&
                         (string.IsNullOrWhiteSpace(filter.City) ||
                          w.City.ToLower().Contains(filter.City.ToLower())) &&
                         (!filter.MinHourlyRate.HasValue ||
                          (w.MinHourlyRate.HasValue && w.MinHourlyRate.Value >= filter.MinHourlyRate.Value)) &&
                         (!filter.MaxHourlyRate.HasValue ||
                          (w.MaxHourlyRate.HasValue && w.MaxHourlyRate.Value <= filter.MaxHourlyRate.Value)) &&
                         (!filter.MinExperienceYears.HasValue ||
                          w.ExperienceYears >= filter.MinExperienceYears.Value) &&
                         (!filter.PreferredArrangement.HasValue ||
                          w.PreferredArrangement == filter.PreferredArrangement.Value) &&
                         (!filter.CommitmentPreference.HasValue ||
                          w.CommitmentPreference == filter.CommitmentPreference.Value) &&
                         (!filter.NonSmokerOnly.HasValue || !filter.NonSmokerOnly.Value || !w.IsSmoker) &&
                         (!filter.VerifiedOnly.HasValue || !filter.VerifiedOnly.Value ||
                          w.IdentityVerificationStatus == VerificationStatus.Verified),
                    "ReviewsReceived"
                );

                if (!workersResult.IsSuccess || workersResult.Data == null)
                    return Result<IEnumerable<WorkerCardDTO>>.Failure(
                        workersResult.Messages ?? new[] { "Failed to retrieve workers." },
                        workersResult.StatusCode);

                // Get existing non-completed bookings for this customer
                var activeBookingsResult = await _unitOfWork.Bookings.FindManyAsync(
                    b => b.CustomerId == customerId
                         && !b.BookingInactive
                         && b.WorkerId != null
                         && b.Status != ApplicationStatus.Completed
                );

                var blockedWorkerIds = new HashSet<string>(
                    activeBookingsResult.IsSuccess && activeBookingsResult.Data != null
                        ? activeBookingsResult.Data
                            .Where(b => !string.IsNullOrWhiteSpace(b.WorkerId))
                            .Select(b => b.WorkerId!)
                        : Enumerable.Empty<string>()
                );

                // Apply list-based filters + exclude blocked workers
                var filteredWorkers = workersResult.Data
                    .Where(w => !blockedWorkerIds.Contains(w.Id))
                    .Where(w => !filter.RequiredServices.Any() ||
                                filter.RequiredServices.All(s => w.OfferedServices.Contains(s)))
                    .Where(w => !filter.RequiredSkills.Any() ||
                                filter.RequiredSkills.All(s => w.Skills.Contains(s)))
                    .Where(w => !filter.RequiredAgeGroups.Any() ||
                                filter.RequiredAgeGroups.All(a => w.ExperiencedAgeGroups.Contains(a)))
                    .Where(w => !filter.RequiredLanguages.Any() ||
                                filter.RequiredLanguages.All(req =>
                                    w.LanguagesSpoken.Any(lang =>
                                        string.Equals(lang, req, StringComparison.OrdinalIgnoreCase))))
                    .ToList();

                var workerCards = _mapper.Map<List<WorkerCardDTO>>(filteredWorkers);

                if (filter.MinAverageRating.HasValue)
                    workerCards = workerCards
                        .Where(c => c.AverageRating >= filter.MinAverageRating.Value)
                        .ToList();

                return Result<IEnumerable<WorkerCardDTO>>.Success(workerCards, 200);
            }
            catch (Exception ex)
            {
                return Result<IEnumerable<WorkerCardDTO>>.Failure(new[] { ex.Message }, 500);
            }
        }

        public async Task<IResult<WorkerDetailsDTO>> GetWorkerDetailsAsync(string workerId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(workerId))
                    return Result<WorkerDetailsDTO>.Failure(new[] { "WorkerId is required." }, 400);

                var workersResult = await _unitOfWork.Workers.FindManyAsync(
                    w => w.Id == workerId && !w.IsDeleted,
                    "ReviewsReceived"
                );

                var worker = workersResult.Data?.FirstOrDefault();

                if (!workersResult.IsSuccess || worker == null)
                    return Result<WorkerDetailsDTO>.Failure(new[] { "Worker not found." }, 404);

                var dto = _mapper.Map<WorkerDetailsDTO>(worker);
                return Result<WorkerDetailsDTO>.Success(dto, 200);
            }
            catch (Exception ex)
            {
                return Result<WorkerDetailsDTO>.Failure(new[] { ex.Message }, 500);
            }
        }

        public async Task<IResult<IEnumerable<JobApplicationDTO>>> GetJobApplicantsAsync(string jobPostingId, string customerId)
        {
            try
            {
                var jobResult = await _unitOfWork.JobPostings.FindByIdAsync(jobPostingId);
                if (!jobResult.IsSuccess || jobResult.Data == null || jobResult.Data.CustomerId != customerId)
                    return Result<IEnumerable<JobApplicationDTO>>.Failure(new[] { "Unauthorized or job not found." }, 401);

                var applicationsResult = await _unitOfWork.JobApplications.FindManyAsync(
                    a => a.JobPostingId == jobPostingId,
                    "Worker"
                );

                if (!applicationsResult.IsSuccess || applicationsResult.Data == null)
                    return Result<IEnumerable<JobApplicationDTO>>.Failure(
                        applicationsResult.Messages ?? new[] { "Failed to retrieve applicants." },
                        applicationsResult.StatusCode);

                var applicantDtos = applicationsResult.Data
                    .Select(a => new JobApplicationDTO
                    {
                        Id = a.Id,
                        JobPostingId = a.JobPostingId,
                        WorkerId = a.WorkerId,
                        WorkerName = a.Worker != null
                            ? $"{a.Worker.FirstName} {a.Worker.LastName}"
                            : "Unknown",
                        WorkerBio = a.Worker?.Bio ?? "No bio provided.",
                        WorkerMinHourlyRate = a.Worker?.MinHourlyRate,
                        WorkerMaxHourlyRate = a.Worker?.MaxHourlyRate,
                        MessageToCustomer = a.MessageToCustomer,
                        SoonestAvailableStartDate = a.SoonestAvailableStartDate,
                        IsCurrentlyWorking = a.IsCurrentlyWorking,
                        QuestionsAboutWork = a.QuestionsAboutWork,
                        Status = a.Status
                    })
                    .ToList();

                return Result<IEnumerable<JobApplicationDTO>>.Success(applicantDtos, 200);
            }
            catch (Exception ex)
            {
                return Result<IEnumerable<JobApplicationDTO>>.Failure(new[] { ex.Message }, 500);
            }
        }

        public async Task<IResult> HireWorkerForJobAsync(string applicationId, string customerId)
        {
            try
            {
                var appResult = await _unitOfWork.JobApplications.FindManyAsync(
                    a => a.Id == applicationId,
                    "JobPosting", "Worker"
                );

                var application = appResult.Data?.FirstOrDefault();

                if (!appResult.IsSuccess || application == null)
                    return Result.Failure(new[] { "Application not found." }, 404);

                var jobPosting = application.JobPosting;

                if (jobPosting == null || jobPosting.CustomerId != customerId)
                    return Result.Failure(new[] { "Unauthorized action." }, 401);

                if (jobPosting.PostInactive)
                    return Result.Failure(new[] { "This job posting is no longer active." }, 409);

                if (!string.IsNullOrWhiteSpace(jobPosting.AssignedWorkerId))
                    return Result.Failure(new[] { "A worker has already been assigned to this job." }, 409);

                if (application.Status != ApplicationStatus.Pending)
                    return Result.Failure(new[] { "This application can no longer be selected." }, 409);

                application.Status = ApplicationStatus.Accepted;
                application.AcceptedAt = DateTime.UtcNow;
                await _unitOfWork.JobApplications.UpdateAsync(application);

                var otherAppsResult = await _unitOfWork.JobApplications.FindManyAsync(a =>
                    a.JobPostingId == jobPosting.Id && a.Id != applicationId && a.Status == ApplicationStatus.Pending
                );

                if (otherAppsResult.IsSuccess && otherAppsResult.Data != null)
                {
                    foreach (var otherApp in otherAppsResult.Data)
                    {
                        otherApp.Status = ApplicationStatus.Rejected;
                        otherApp.RejectedAt = DateTime.UtcNow;
                        await _unitOfWork.JobApplications.UpdateAsync(otherApp);
                    }
                }

                jobPosting.AssignedWorkerId = application.WorkerId;
                jobPosting.Status = ApplicationStatus.Accepted;
                jobPosting.PostInactive = true;
                await _unitOfWork.JobPostings.UpdateAsync(jobPosting);

                var booking = new Booking
                {
                    CustomerId = jobPosting.CustomerId,
                    WorkerId = application.WorkerId,
                    Title = jobPosting.Title,
                    Description = jobPosting.Description,
                    Requirements = jobPosting.Requirements,
                    City = jobPosting.City,
                    Address = jobPosting.Address,
                    Latitude = jobPosting.Latitude,
                    Longitude = jobPosting.Longitude,
                    StartDate = jobPosting.StartDate,
                    EndDate = jobPosting.EndDate,
                    EstimatedHours = jobPosting.EstimatedHours,
                    Budget = jobPosting.Budget,
                    RequireNonSmoker = jobPosting.RequireNonSmoker,
                    ServiceTypes = jobPosting.ServiceTypes,
                    WorkArrangement = jobPosting.WorkArrangement,
                    CommitmentPreference = jobPosting.CommitmentPreference,
                    RequiredSkills = jobPosting.RequiredSkills,
                    Status = ApplicationStatus.Pending,
                    BookingInactive = false,
                    CreatedAt = DateTime.UtcNow
                };

                var bookingResult = await _unitOfWork.Bookings.CreateAsync(booking);
                if (!bookingResult.IsSuccess)
                    return Result.Failure(bookingResult.Messages ?? new[] { "Failed to create booking." }, 500);

                application.BookingId = booking.Id;
                await _unitOfWork.JobApplications.UpdateAsync(application);

                var commitResult = await _unitOfWork.CommitAsync();
                return commitResult.IsSuccess
                    ? Result.Success(200, "Worker hired successfully and booking created.")
                    : Result.Failure(commitResult.Messages ?? new[] { "Failed to finalize hiring." }, 500);
            }
            catch (Exception ex)
            {
                return Result.Failure(new[] { ex.Message }, 500);
            }
        }

        public async Task<IResult> LeaveReviewForWorkerAsync(ReviewCreateDTO model, string customerId)
        {
            try
            {
                var bookingResult = await _unitOfWork.Bookings.FindByIdAsync(model.BookingId);
                if (!bookingResult.IsSuccess || bookingResult.Data == null)
                    return Result.Failure(new[] { "Booking not found." }, 404);

                var booking = bookingResult.Data;
                if (booking.CustomerId != customerId || booking.WorkerId != model.RevieweeId)
                    return Result.Failure(new[] { "Unauthorized to review this booking." }, 401);

                var existingReview = await _unitOfWork.Reviews.FindFirstAsync(
                    r => r.BookingId == model.BookingId && r.ReviewerId == customerId
                );

                if (existingReview.IsSuccess && existingReview.Data != null)
                    return Result.Failure(new[] { "You have already reviewed this booking." }, 400);

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
                return Result.Failure(new[] { ex.Message });
            }
        }

        public async Task<IResult<ReviewUpdateDTO>> GetMyReviewByBookingIdAsync(string bookingId, string customerId)
        {
            try
            {
                var reviewResult = await _unitOfWork.Reviews.FindFirstAsync(
                    r => r.BookingId == bookingId && r.ReviewerId == customerId
                );

                if (!reviewResult.IsSuccess || reviewResult.Data == null)
                    return Result<ReviewUpdateDTO>.Failure(new[] { "Review not found." }, 404);

                var dto = new ReviewUpdateDTO
                {
                    Id = reviewResult.Data.Id,
                    Rating = reviewResult.Data.Rating,
                    Comment = reviewResult.Data.Comment
                };

                return Result<ReviewUpdateDTO>.Success(dto, 200);
            }
            catch (Exception ex)
            {
                return Result<ReviewUpdateDTO>.Failure(new[] { ex.Message }, 500);
            }
        }

        public async Task<IResult> UpdateReviewAsync(ReviewUpdateDTO model, string customerId)
        {
            try
            {
                var reviewResult = await _unitOfWork.Reviews.FindByIdAsync(model.Id);

                if (!reviewResult.IsSuccess || reviewResult.Data == null)
                    return Result.Failure(new[] { "Review not found." }, 404);

                var review = reviewResult.Data;

                if (review.ReviewerId != customerId)
                    return Result.Failure(new[] { "Unauthorized to edit this review." }, 401);

                review.Rating = model.Rating;
                review.Comment = model.Comment;

                var updateResult = await _unitOfWork.Reviews.UpdateAsync(review);
                if (!updateResult.IsSuccess) return updateResult;

                return await _unitOfWork.CommitAsync();
            }
            catch (Exception ex)
            {
                return Result.Failure(new[] { ex.Message });
            }
        }
        
        public async Task<IResult> UpdateBookingAsync(BookingDetailDTO model, string customerId)
        {
            try
            {
                if (model == null)
                    return Result.Failure(new[] { "Booking payload is required." }, 400);

                if (string.IsNullOrWhiteSpace(model.Id))
                    return Result.Failure(new[] { "Booking ID is required." }, 400);

                if (string.IsNullOrWhiteSpace(customerId))
                    return Result.Failure(new[] { "Customer ID is required." }, 400);

                if (model.EndDate < model.StartDate)
                    return Result.Failure(new[] { "End date must be on or after start date." }, 400);

                var bookingResult = await _unitOfWork.Bookings.FindByIdAsync(model.Id);
                if (!bookingResult.IsSuccess || bookingResult.Data == null)
                    return Result.Failure(new[] { "Booking not found." }, 404);

                var booking = bookingResult.Data;

                // Owner check
                if (booking.CustomerId != customerId)
                    return Result.Failure(new[] { "Unauthorized action." }, 401);

                // Status lock: customer cannot edit once accepted/rejected/completed.
                if (booking.Status == ApplicationStatus.Accepted ||
                    booking.Status == ApplicationStatus.Rejected ||
                    booking.Status == ApplicationStatus.Completed)
                    return Result.Failure(
                        new[] { "This booking can no longer be edited in its current status." }, 409);

                if (booking.BookingInactive)
                    return Result.Failure(new[] { "This booking is inactive and cannot be edited." }, 409);

                if (model.ServiceTypes == null || !model.ServiceTypes.Any())
                    return Result.Failure(new[] { "At least one service type is required." }, 400);

                // Update editable fields
                booking.Title = model.Title;
                booking.Description = model.Description;
                booking.Requirements = model.Requirements;
                booking.City = model.City;
                booking.Address = model.Address;
                booking.StartDate = model.StartDate;
                booking.EndDate = model.EndDate;
                booking.EstimatedHours = model.EstimatedHours;
                booking.Budget = model.Budget;
                booking.RequireNonSmoker = model.RequireNonSmoker;
                booking.WorkArrangement = model.WorkArrangement;
                booking.CommitmentPreference = model.CommitmentPreference;
                booking.ServiceTypes = model.ServiceTypes?.ToList() ?? new List<ServiceType>();
                booking.RequiredSkills = model.RequiredSkills?.ToList() ?? new List<Skill>();

                // Re-geocode from City/Address
                var geoQuery = string.IsNullOrWhiteSpace(booking.Address)
                    ? booking.City
                    : $"{booking.Address}, {booking.City}";

                if (!string.IsNullOrWhiteSpace(geoQuery))
                {
                    var (lat, lon) = await _geocoding.GeocodeAsync(geoQuery);
                    booking.Latitude = lat;
                    booking.Longitude = lon;
                }

                var updateResult = await _unitOfWork.Bookings.UpdateAsync(booking);
                if (!updateResult.IsSuccess)
                    return updateResult;

                return await _unitOfWork.CommitAsync();
            }
            catch (Exception ex)
            {
                return Result.Failure(new[] { ex.Message }, 500);
            }
        }

        public async Task<IResult> CancelBookingAsync(string bookingId, string customerId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(bookingId))
                    return Result.Failure(new[] { "Booking ID is required." }, 400);

                if (string.IsNullOrWhiteSpace(customerId))
                    return Result.Failure(new[] { "Customer ID is required." }, 400);

                var bookingResult = await _unitOfWork.Bookings.FindByIdAsync(bookingId);
                if (!bookingResult.IsSuccess || bookingResult.Data == null)
                    return Result.Failure(new[] { "Booking not found." }, 404);

                var booking = bookingResult.Data;

                // Owner check
                if (booking.CustomerId != customerId)
                    return Result.Failure(new[] { "Unauthorized action." }, 401);

                if (booking.BookingInactive)
                    return Result.Failure(new[] { "Booking is already inactive." }, 409);

                // Cancellation lock matches your rule
                if (booking.Status == ApplicationStatus.Accepted ||
                    booking.Status == ApplicationStatus.Rejected ||
                    booking.Status == ApplicationStatus.Completed)
                    return Result.Failure(
                        new[] { "This booking can no longer be canceled in its current status." }, 409);

                booking.BookingInactive = true;
                booking.Status = ApplicationStatus.Cancelled;

                var updateResult = await _unitOfWork.Bookings.UpdateAsync(booking);
                if (!updateResult.IsSuccess)
                    return updateResult;

                return await _unitOfWork.CommitAsync();
            }
            catch (Exception ex)
            {
                return Result.Failure(new[] { ex.Message }, 500);
            }
        }
        
        public async Task<IResult> UpdateJobPostingAsync(JobPostingDetailDTO model, string customerId)
        {
            try
            {
                if (model == null)
                    return Result.Failure(new[] { "Job payload is required." }, 400);

                if (string.IsNullOrWhiteSpace(model.Id))
                    return Result.Failure(new[] { "Job ID is required." }, 400);

                if (string.IsNullOrWhiteSpace(customerId))
                    return Result.Failure(new[] { "Customer ID is required." }, 400);

                if (model.EndDate < model.StartDate)
                    return Result.Failure(new[] { "End date must be on or after start date." }, 400);

                if (model.ServiceTypes == null || !model.ServiceTypes.Any())
                    return Result.Failure(new[] { "At least one service type is required." }, 400);

                var jobResult = await _unitOfWork.JobPostings.FindByIdAsync(model.Id);
                if (!jobResult.IsSuccess || jobResult.Data == null)
                    return Result.Failure(new[] { "Job posting not found." }, 404);

                var job = jobResult.Data;

                if (job.CustomerId != customerId)
                    return Result.Failure(new[] { "Unauthorized action." }, 401);

                if (job.PostInactive)
                    return Result.Failure(new[] { "This job posting is inactive and cannot be edited." }, 409);

                if (job.Status == ApplicationStatus.Accepted ||
                    job.Status == ApplicationStatus.Rejected ||
                    job.Status == ApplicationStatus.Completed ||
                    job.Status == ApplicationStatus.Cancelled)
                    return Result.Failure(new[] { "This job posting can no longer be edited in its current status." },
                        409);

                job.Title = model.Title;
                job.Description = model.Description;
                job.Requirements = model.Requirements;
                job.City = model.City;
                job.Address = model.Address;
                job.StartDate = model.StartDate;
                job.EndDate = model.EndDate;
                job.EstimatedHours = model.EstimatedHours;
                job.Budget = model.Budget;
                job.RequireNonSmoker = model.RequireNonSmoker;
                job.WorkArrangement = model.WorkArrangement;
                job.CommitmentPreference = model.CommitmentPreference;
                job.ServiceTypes = model.ServiceTypes?.ToList() ?? new List<ServiceType>();
                job.RequiredSkills = model.RequiredSkills?.ToList() ?? new List<Skill>();

                var geoQuery = string.IsNullOrWhiteSpace(job.Address)
                    ? job.City
                    : $"{job.Address}, {job.City}";

                if (!string.IsNullOrWhiteSpace(geoQuery))
                {
                    var (lat, lon) = await _geocoding.GeocodeAsync(geoQuery);
                    job.Latitude = lat;
                    job.Longitude = lon;
                }

                var updateResult = await _unitOfWork.JobPostings.UpdateAsync(job);
                if (!updateResult.IsSuccess)
                    return updateResult;

                return await _unitOfWork.CommitAsync();
            }
            catch (Exception ex)
            {
                return Result.Failure(new[] { ex.Message }, 500);
            }
        }
        
        public async Task<IResult> CancelJobPostingAsync(string jobId, string customerId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(jobId))
                    return Result.Failure(new[] { "Job ID is required." }, 400);

                if (string.IsNullOrWhiteSpace(customerId))
                    return Result.Failure(new[] { "Customer ID is required." }, 400);

                var jobResult = await _unitOfWork.JobPostings.FindByIdAsync(jobId);
                if (!jobResult.IsSuccess || jobResult.Data == null)
                    return Result.Failure(new[] { "Job posting not found." }, 404);

                var job = jobResult.Data;

                if (job.CustomerId != customerId)
                    return Result.Failure(new[] { "Unauthorized action." }, 401);

                if (job.PostInactive)
                    return Result.Failure(new[] { "Job posting is already inactive." }, 409);

                if (job.Status == ApplicationStatus.Accepted ||
                    job.Status == ApplicationStatus.Rejected ||
                    job.Status == ApplicationStatus.Completed ||
                    job.Status == ApplicationStatus.Cancelled)
                    return Result.Failure(new[] { "This job posting can no longer be canceled in its current status." }, 409);

                job.PostInactive = true;
                job.Status = ApplicationStatus.Cancelled;

                var updateResult = await _unitOfWork.JobPostings.UpdateAsync(job);
                if (!updateResult.IsSuccess)
                    return updateResult;

                return await _unitOfWork.CommitAsync();
            }
            catch (Exception ex)
            {
                return Result.Failure(new[] { ex.Message }, 500);
            }
        }
        
        public async Task<IResult<JobApplicationDTO>> GetJobApplicationDetailsAsync(string applicationId, string customerId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(applicationId))
                    return Result<JobApplicationDTO>.Failure(new[] { "Application ID is required." }, 400);

                if (string.IsNullOrWhiteSpace(customerId))
                    return Result<JobApplicationDTO>.Failure(new[] { "Customer ID is required." }, 400);

                var appResult = await _unitOfWork.JobApplications.FindManyAsync(
                    a => a.Id == applicationId,
                    "Worker", "JobPosting"
                );

                var app = appResult.Data?.FirstOrDefault();

                if (!appResult.IsSuccess || app == null)
                    return Result<JobApplicationDTO>.Failure(new[] { "Application not found." }, 404);

                if (app.JobPosting == null || app.JobPosting.CustomerId != customerId)
                    return Result<JobApplicationDTO>.Failure(new[] { "Unauthorized access." }, 401);

                var dto = new JobApplicationDTO
                {
                    Id = app.Id,
                    JobPostingId = app.JobPostingId,
                    WorkerId = app.WorkerId,
                    WorkerName = app.Worker != null
                        ? $"{app.Worker.FirstName} {app.Worker.LastName}".Trim()
                        : "Unknown",
                    WorkerBio = app.Worker?.Bio ?? "No bio provided.",
                    WorkerMinHourlyRate = app.Worker?.MinHourlyRate,
                    WorkerMaxHourlyRate = app.Worker?.MaxHourlyRate,
                    MessageToCustomer = app.MessageToCustomer,
                    SoonestAvailableStartDate = app.SoonestAvailableStartDate,
                    IsCurrentlyWorking = app.IsCurrentlyWorking,
                    QuestionsAboutWork = app.QuestionsAboutWork,
                    Status = app.Status
                };

                return Result<JobApplicationDTO>.Success(dto, 200);
            }
            catch (Exception ex)
            {
                return Result<JobApplicationDTO>.Failure(new[] { ex.Message }, 500);
            }
        }
    }
}