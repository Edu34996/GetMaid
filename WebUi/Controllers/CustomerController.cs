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
        public async Task<IActionResult> Dashboard()
        {
            var customerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(customerId)) return Unauthorized();

            var result = await _customerService.GetProfileAsync(customerId);

            // For testing purposes, fetch children data simultaneously
            var childrenResult = await _customerService.GetMyChildrenAsync(customerId);
            ViewBag.Children = childrenResult.Data ?? new List<ChildDTO>();

            return View(result.Data);
        }

        // POST: Customer/AddChild
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddChild(ChildDTO model)
        {
            var customerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(customerId)) return Unauthorized();

            var result = await _customerService.AddChildAsync(model, customerId);

            if (result.IsSuccess) return RedirectToAction(nameof(Dashboard));

            return View("Dashboard");
        }

        // GET: Customer/MyJobs
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
            return View();
        }

        // POST: Customer/CreateJob
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateJob(ServiceRequestCreateDTO model)
        {
            if (!ModelState.IsValid) return View(model);

            var customerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(customerId)) return Unauthorized();

            // Creating a job posting => ensure no target worker
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
        public async Task<IActionResult> WorkerShop(WorkerSearchFilterDTO filter)
        {
            filter ??= new WorkerSearchFilterDTO();

            var result = await _customerService.BrowseWorkersAsync(filter);

            if (!result.IsSuccess)
            {
                TempData["ErrorMessage"] = string.Join(" ", result.Messages);
                return View(new List<WorkerCardDTO>());
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
        public IActionResult BookWorker(string workerId)
        {
            if (string.IsNullOrEmpty(workerId)) return BadRequest("Worker ID is required.");

            var model = new ServiceRequestCreateDTO
            {
                TargetWorkerId = workerId,
                StartDate = DateTime.Today.AddDays(1),
                EndDate = DateTime.Today.AddDays(1),
                ServiceTypes = new List<Core.Concretes.Enums.ServiceType>()
            };

            return View(model);
        }

        // POST: Customer/BookWorker
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BookWorker(ServiceRequestCreateDTO model)
        {
            if (!ModelState.IsValid) return View(model);

            var customerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(customerId)) return Unauthorized();

            // Creating a booking => TargetWorkerId should be present from the form
            var result = await _customerService.CreateServiceRequestAsync(model, customerId);

            if (result.IsSuccess)
            {
                TempData["SuccessMessage"] = "Booking requested successfully! It is now pending worker approval.";
                return RedirectToAction(nameof(MyBookings));
            }

            ModelState.AddModelError(string.Empty, string.Join(" ", result.Messages));
            return View(model);
        }

        // GET: Customer/MyBookings
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

        // POST: Customer/HireWorker
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> HireWorker(string applicationId)
        {
            if (string.IsNullOrEmpty(applicationId)) return BadRequest("Application ID is required.");

            var customerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(customerId)) return Unauthorized();

            var result = await _customerService.HireWorkerForJobAsync(applicationId, customerId);

            if (result.IsSuccess)
            {
                TempData["SuccessMessage"] = "Worker successfully hired! The job is now assigned.";
            }
            else
            {
                TempData["ErrorMessage"] = string.Join(" ", result.Messages);
            }

            return RedirectToAction(nameof(MyJobs));
        }

        // GET: Customer/LeaveReview
        [HttpGet]
        public async Task<IActionResult> LeaveReview(string bookingId, string revieweeId)
        {
            var customerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(customerId)) return Unauthorized();

            var existingReview = await _customerService.GetMyReviewByBookingIdAsync(bookingId, customerId);

            if (existingReview.IsSuccess)
            {
                return RedirectToAction(nameof(EditReview), new { bookingId });
            }

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
    }
}