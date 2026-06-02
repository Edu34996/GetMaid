using System.Security.Claims;
using Core.Abstracts.IServices;
using Core.Concretes.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebUi.Controllers
{
    [Authorize(Roles = "Customer")]
    public class CustomerController : Controller
    {
        private readonly ICustomerService _customerService;

        public CustomerController(ICustomerService customerService)
        {
            _customerService = customerService;
        }

        // GET: Customer/Dashboard
        [HttpGet]
        public async Task<IActionResult> Dashboard()
        {
            var customerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(customerId)) return Unauthorized();

            var profileResult = await _customerService.GetProfileAsync(customerId);
            if (!profileResult.IsSuccess || profileResult.Data == null)
            {
                TempData["ErrorMessage"] = string.Join(" ", profileResult.Messages ?? new[] { "Failed to load dashboard." });
                return View();
            }

            var childrenResult = await _customerService.GetMyChildrenAsync(customerId);
            ViewBag.Children = childrenResult.IsSuccess && childrenResult.Data != null
                ? childrenResult.Data
                : new List<ChildDTO>();

            return View(profileResult.Data);
        }

        // POST: Customer/UpdateProfile
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateProfile(CustomerProfileUpdateDTO model)
        {
            var customerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(customerId)) return Unauthorized();

            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Please correct the profile fields and try again.";
                return RedirectToAction(nameof(Dashboard));
            }

            var result = await _customerService.UpdateProfileAsync(customerId, model);

            if (result.IsSuccess)
                TempData["SuccessMessage"] = "Profile updated successfully.";
            else
                TempData["ErrorMessage"] = string.Join(" ", result.Messages);

            return RedirectToAction(nameof(Dashboard));
        }

        // POST: Customer/AddChild
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddChild(ChildDTO model)
        {
            var customerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(customerId)) return Unauthorized();

            var result = await _customerService.AddChildAsync(model ?? new ChildDTO(), customerId);

            if (result.IsSuccess)
                TempData["SuccessMessage"] = "Child added successfully.";
            else
                TempData["ErrorMessage"] = string.Join(" ", result.Messages);

            return RedirectToAction(nameof(Dashboard));
        }

        // GET: Customer/EditChild
        [HttpGet]
        public async Task<IActionResult> EditChild(string childId)
        {
            if (string.IsNullOrWhiteSpace(childId)) return BadRequest("Child ID is required.");

            var customerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(customerId)) return Unauthorized();

            var result = await _customerService.GetChildByIdAsync(childId, customerId);
            if (!result.IsSuccess || result.Data == null)
            {
                TempData["ErrorMessage"] = string.Join(" ", result.Messages ?? new[] { "Child not found." });
                return RedirectToAction(nameof(Dashboard));
            }

            return View(result.Data);
        }

        // POST: Customer/EditChild
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditChild(ChildDTO model)
        {
            var customerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(customerId)) return Unauthorized();

            if (string.IsNullOrWhiteSpace(model?.Id))
            {
                TempData["ErrorMessage"] = "Child ID is required for update.";
                return RedirectToAction(nameof(Dashboard));
            }

            var result = await _customerService.UpdateChildAsync(model, customerId);

            if (result.IsSuccess)
            {
                TempData["SuccessMessage"] = "Child updated successfully.";
                return RedirectToAction(nameof(Dashboard));
            }

            ModelState.AddModelError(string.Empty, string.Join(" ", result.Messages));
            return View(model);
        }

        // POST: Customer/RemoveChild
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveChild(string childId)
        {
            if (string.IsNullOrWhiteSpace(childId)) return BadRequest("Child ID is required.");

            var customerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(customerId)) return Unauthorized();

            var result = await _customerService.RemoveChildAsync(childId, customerId);

            if (result.IsSuccess)
                TempData["SuccessMessage"] = "Child removed successfully.";
            else
                TempData["ErrorMessage"] = string.Join(" ", result.Messages);

            return RedirectToAction(nameof(Dashboard));
        }

        // GET: Customer/MyJobs
        [HttpGet]
        public async Task<IActionResult> MyJobs()
        {
            var customerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(customerId)) return Unauthorized();

            var result = await _customerService.GetMyJobPostingsAsync(customerId);

            if (!result.IsSuccess)
            {
                TempData["ErrorMessage"] = string.Join(" ", result.Messages);
                return View(new List<JobPostingCardDTO>());
            }

            return View(result.Data);
        }

        // GET: Customer/CreateJob
        [HttpGet]
        public IActionResult CreateJob()
        {
            var model = new ServiceRequestCreateDTO
            {
                TargetWorkerId = null,
                StartDate = DateTime.Today.AddDays(1),
                EndDate = DateTime.Today.AddDays(1),
                ServiceTypes = new List<Core.Concretes.Enums.ServiceType>()
            };

            return View(model);
        }

        // POST: Customer/CreateJob
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateJob(ServiceRequestCreateDTO model)
        {
            if (!ModelState.IsValid) return View(model);

            var customerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(customerId)) return Unauthorized();

            model.TargetWorkerId = null;

            var result = await _customerService.CreateServiceRequestAsync(model, customerId);

            if (result.IsSuccess)
            {
                TempData["SuccessMessage"] = "Job posted successfully!";
                return RedirectToAction(nameof(MyJobs));
            }

            ModelState.AddModelError(string.Empty, string.Join(" ", result.Messages));
            return View(model);
        }

        // GET: Customer/WorkerShop
        [HttpGet]
        public async Task<IActionResult> WorkerShop(WorkerSearchFilterDTO filter)
        {
            filter ??= new WorkerSearchFilterDTO();

            var customerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(customerId)) return Unauthorized();

            var result = await _customerService.BrowseWorkersAsync(filter, customerId);

            if (!result.IsSuccess)
            {
                TempData["ErrorMessage"] = string.Join(" ", result.Messages);
                ViewBag.CurrentFilter = filter;
                ViewBag.CustomerLatitude = null;
                ViewBag.CustomerLongitude = null;
                return View(new List<WorkerCardDTO>());
            }

            // Map-centering info for WorkerShop map
            var profile = await _customerService.GetProfileAsync(customerId);
            if (profile.IsSuccess && profile.Data != null)
            {
                ViewBag.CustomerLatitude = profile.Data.Latitude;
                ViewBag.CustomerLongitude = profile.Data.Longitude;
            }
            else
            {
                ViewBag.CustomerLatitude = null;
                ViewBag.CustomerLongitude = null;
            }

            ViewBag.CurrentFilter = filter;
            return View(result.Data);
        }

        // GET: Customer/WorkerDetails
        [HttpGet]
        public async Task<IActionResult> WorkerDetails(string workerId)
        {
            if (string.IsNullOrWhiteSpace(workerId)) return BadRequest("Worker ID is required.");

            var result = await _customerService.GetWorkerDetailsAsync(workerId);

            if (!result.IsSuccess || result.Data == null)
            {
                TempData["ErrorMessage"] = string.Join(" ", result.Messages ?? new[] { "Worker not found." });
                return RedirectToAction(nameof(WorkerShop));
            }

            return View(result.Data);
        }

        // GET: Customer/BookWorker
        [HttpGet]
        public async Task<IActionResult> BookWorker(string workerId)
        {
            if (string.IsNullOrWhiteSpace(workerId))
                return BadRequest("Worker ID is required.");

            var workerResult = await _customerService.GetWorkerDetailsAsync(workerId);
            if (!workerResult.IsSuccess || workerResult.Data == null)
            {
                TempData["ErrorMessage"] = string.Join(" ", workerResult.Messages ?? new[] { "Worker not found." });
                return RedirectToAction(nameof(WorkerShop));
            }

            var worker = workerResult.Data;

            ViewBag.WorkerId = worker.Id;
            ViewBag.WorkerName = $"{worker.FirstName} {worker.LastName}".Trim();
            ViewBag.WorkerImage = worker.ProfilePictureUrl;

            var model = new ServiceRequestCreateDTO
            {
                TargetWorkerId = workerId,
                StartDate = DateTime.Today.AddDays(1),
                EndDate = DateTime.Today.AddDays(1),
                ServiceTypes = new List<Core.Concretes.Enums.ServiceType>()
            };

            return View(model);
        }

        // GET: Customer/MyBookings
        [HttpGet]
        public async Task<IActionResult> MyBookings()
        {
            var customerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(customerId)) return Unauthorized();

            var result = await _customerService.GetMyBookingsAsync(customerId);

            if (!result.IsSuccess)
            {
                TempData["ErrorMessage"] = string.Join(" ", result.Messages);
                return View(new List<BookingListItemDTO>());
            }

            return View(result.Data);
        }

        // GET: Customer/ReviewApplicants
        [HttpGet]
        public async Task<IActionResult> ReviewApplicants(string jobId)
        {
            if (string.IsNullOrEmpty(jobId)) return BadRequest("Job ID is required.");

            var customerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(customerId)) return Unauthorized();

            var result = await _customerService.GetJobApplicantsAsync(jobId, customerId);

            if (!result.IsSuccess)
            {
                TempData["ErrorMessage"] = string.Join(" ", result.Messages);
                return RedirectToAction(nameof(MyJobs));
            }

            return View(result.Data);
        }

        // POST: Customer/BookWorker
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BookWorker(ServiceRequestCreateDTO model)
        {
            if (!ModelState.IsValid) return View(model);

            var customerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(customerId)) return Unauthorized();

            var result = await _customerService.CreateServiceRequestAsync(model, customerId);

            if (result.IsSuccess)
            {
                TempData["SuccessMessage"] = "Booking requested successfully! It is now pending worker approval.";
                return RedirectToAction(nameof(MyBookings));
            }

            ModelState.AddModelError(string.Empty, string.Join(" ", result.Messages));
            return View(model);
        }

        // GET: Customer/LeaveReview
        [HttpGet]
        public async Task<IActionResult> LeaveReview(string bookingId, string revieweeId)
        {
            var customerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(customerId)) return Unauthorized();

            var existingReview = await _customerService.GetMyReviewByBookingIdAsync(bookingId, customerId);

            if (existingReview.IsSuccess)
                return RedirectToAction(nameof(EditReview), new { bookingId });

            var model = new ReviewCreateDTO
            {
                BookingId = bookingId,
                RevieweeId = revieweeId
            };

            return View(model);
        }

        // GET: Customer/EditReview
        [HttpGet]
        public async Task<IActionResult> EditReview(string bookingId)
        {
            var customerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(customerId)) return Unauthorized();

            var result = await _customerService.GetMyReviewByBookingIdAsync(bookingId, customerId);

            if (!result.IsSuccess)
            {
                TempData["ErrorMessage"] = "Could not find a review to edit.";
                return RedirectToAction(nameof(MyBookings));
            }

            return View(result.Data);
        }

        // POST: Customer/EditReview
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditReview(ReviewUpdateDTO model)
        {
            if (!ModelState.IsValid) return View(model);

            var customerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(customerId)) return Unauthorized();

            var result = await _customerService.UpdateReviewAsync(model, customerId);

            if (result.IsSuccess)
            {
                TempData["SuccessMessage"] = "Your review was successfully updated!";
                return RedirectToAction(nameof(MyBookings));
            }

            ModelState.AddModelError(string.Empty, string.Join(" ", result.Messages));
            return View(model);
        }

        // POST: Customer/LeaveReview
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LeaveReview(ReviewCreateDTO model)
        {
            if (!ModelState.IsValid) return View(model);

            var customerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(customerId)) return Unauthorized();

            var result = await _customerService.LeaveReviewForWorkerAsync(model, customerId);

            if (result.IsSuccess)
            {
                TempData["SuccessMessage"] = "Thank you! Your review has been submitted.";
                return RedirectToAction(nameof(MyBookings));
            }

            ModelState.AddModelError(string.Empty, string.Join(" ", result.Messages));
            return View(model);
        }
        
        [HttpGet]
        public async Task<IActionResult> BookingDetails(string bookingId)
        {
            var customerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            
            if (string.IsNullOrEmpty(customerId)) return Unauthorized();

            var result = await _customerService.GetBookingDetailsAsync(bookingId, customerId);

            if (!result.IsSuccess || result.Data == null)
            {
                TempData["ErrorMessage"] = string.Join(" ", result.Messages ?? new[] { "Booking not found." });
                return RedirectToAction(nameof(MyBookings));
            }

            ViewBag.BookingMode = "Customer";
            return View("~/Views/Shared/BookingDetails.cshtml", result.Data);
        }
        
        // GET: Customer/JobDetails
        [HttpGet]
        public async Task<IActionResult> JobDetails(string jobId)
        {
            if (string.IsNullOrWhiteSpace(jobId)) return BadRequest("Job ID is required.");

            var customerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(customerId)) return Unauthorized();

           var result = await _customerService.GetJobPostingDetailsAsync(jobId, customerId);
           
           if (!result.IsSuccess || result.Data == null)
           {
               TempData["ErrorMessage"] = string.Join(" ", result.Messages ?? new[] { "Job not found." });
               return RedirectToAction(nameof(MyJobs));
           }
           
           var applicantsResult = await _customerService.GetJobApplicantsAsync(jobId, customerId);
           ViewBag.Applicants = applicantsResult.IsSuccess && applicantsResult.Data != null
               ? applicantsResult.Data
               : new List<JobApplicationDTO>();

            ViewBag.JobMode = "Customer";
            return View("~/Views/Shared/JobDetails.cshtml", result.Data);
        }
        
        // GET: Customer/ApplicationDetails
        [HttpGet]
        public async Task<IActionResult> ApplicationDetails(string applicationId)
        {
            if (string.IsNullOrWhiteSpace(applicationId)) return BadRequest("Application ID is required.");

            var customerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(customerId)) return Unauthorized();

            var result = await _customerService.GetJobApplicationDetailsAsync(applicationId, customerId);

            if (!result.IsSuccess || result.Data == null)
            {
                TempData["ErrorMessage"] = string.Join(" ", result.Messages ?? new[] { "Application not found." });
                return RedirectToAction(nameof(MyJobs));
            }

            ViewBag.ApplicationMode = "Customer";
            return View("~/Views/Shared/ApplicationDetails.cshtml", result.Data);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateBooking(BookingDetailDTO model)
        {
            var customerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(customerId)) return Unauthorized();

            var result = await _customerService.UpdateBookingAsync(model, customerId);

            if (result.IsSuccess)
            {
                TempData["SuccessMessage"] = "Booking updated successfully.";
                return RedirectToAction(nameof(BookingDetails), new { bookingId = model.Id });
            }

            ModelState.AddModelError(string.Empty, string.Join(" ", result.Messages));
            ViewBag.BookingMode = "customer";
            return View("~/Views/Shared/BookingDetails.cshtml", model);
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelBooking(string bookingId)
        {
            var customerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(customerId)) return Unauthorized();

            var result = await _customerService.CancelBookingAsync(bookingId, customerId);

            if (result.IsSuccess)
                TempData["SuccessMessage"] = "Booking cancelled successfully.";
            else
                TempData["ErrorMessage"] = string.Join(" ", result.Messages);

            return RedirectToAction(nameof(MyBookings));
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateJobPosting(JobPostingDetailDTO model)
        {
            var customerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(customerId)) return Unauthorized();

            var result = await _customerService.UpdateJobPostingAsync(model, customerId);

            if (result.IsSuccess)
            {
                TempData["SuccessMessage"] = "Job posting updated successfully.";
                return RedirectToAction(nameof(JobDetails), new { jobId = model.Id });
            }

            ModelState.AddModelError(string.Empty, string.Join(" ", result.Messages));
            ViewBag.JobMode = "Customer";
    
            var applicantsResult = await _customerService.GetJobApplicantsAsync(model.Id, customerId);
            ViewBag.Applicants = applicantsResult.IsSuccess && applicantsResult.Data != null
                ? applicantsResult.Data
                : new List<JobApplicationDTO>();
    
            return View("~/Views/Shared/JobDetails.cshtml", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelJobPosting(string jobId)
        {
            var customerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(customerId)) return Unauthorized();

            var result = await _customerService.CancelJobPostingAsync(jobId, customerId);

            if (result.IsSuccess)
                TempData["SuccessMessage"] = "Job posting cancelled successfully.";
            else
                TempData["ErrorMessage"] = string.Join(" ", result.Messages);

            return RedirectToAction(nameof(MyJobs));
        }
        
        [HttpGet]
        public IActionResult FinishedJobs()
        {
            // Placeholder until service-level filtering for completed jobs is added
            return View(new List<JobPostingCardDTO>());
        }

        [HttpGet]
        public IActionResult FinishedBookings()
        {
            // Placeholder until service-level filtering for completed bookings is added
            return View(new List<BookingListItemDTO>());
        }

        [HttpGet]
        public IActionResult MessageBox()
        {
            return View();
        }
        
        
    }
}