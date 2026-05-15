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
            
            // For testing purposes, we will fetch children data simultaneously
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
        // ... (Keep your existing Dashboard and AddChild methods) ...

        // GET: Customer/MyJobs
        public async Task<IActionResult> MyJobs()
        {
            var customerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(customerId)) return Unauthorized();

            var result = await _customerService.GetMyJobPostingsAsync(customerId);
            
            if (!result.IsSuccess)
            {
                TempData["ErrorMessage"] = string.Join(" ", result.Messages);
                return View(new List<JobPostingDTO>());
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
        public async Task<IActionResult> CreateJob(JobPostingCreateDTO model)
        {
            if (!ModelState.IsValid) return View(model);

            var customerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(customerId)) return Unauthorized();

            var result = await _customerService.CreateJobPostingAsync(model, customerId);

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
            // The filter parameter will automatically bind to the query string or form data
            filter ??= new WorkerSearchFilterDTO(); // Ensure it's never null

            var result = await _customerService.BrowseWorkersAsync(filter);

            if (!result.IsSuccess)
            {
                TempData["ErrorMessage"] = string.Join(" ", result.Messages);
                return View(new List<WorkerDashboardDTO>());
            }

            // We pass the filter back to the view via ViewBag so the form remembers what the user selected
            ViewBag.CurrentFilter = filter;

            return View(result.Data);
        }
        // ... (Keep existing Dashboard, AddChild, WorkerShop, and MyJobs methods) ...

        // GET: Customer/BookWorker
        // This is triggered when the customer clicks "Book" on a worker's profile in the Shop
        [HttpGet]
        public IActionResult BookWorker(string workerId)
        {
            if (string.IsNullOrEmpty(workerId)) return BadRequest("Worker ID is required.");

            // We pre-fill the DTO with the WorkerId so it can be stored in a hidden field in the form
            var model = new BookingCreateDTO
            {
                WorkerId = workerId,
                ScheduledDate = DateTime.Today.AddDays(1) // Default to tomorrow
            };

            return View(model);
        }

        // POST: Customer/BookWorker
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BookWorker(BookingCreateDTO model)
        {
            if (!ModelState.IsValid) return View(model);

            var customerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(customerId)) return Unauthorized();

            var result = await _customerService.CreateBookingAsync(model, customerId);

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
                return View(new List<BookingDTO>());
            }

            return View(result.Data);
        }
        // ... (Keep existing methods: Dashboard, AddChild, CreateJob, MyJobs, etc.) ...

        // GET: Customer/ReviewApplicants
        // This loads the page showing everyone who applied for a specific job
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
        // This is triggered when the customer clicks the "Hire" button on a specific applicant
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
        // ... (Keep existing methods: MyBookings, ReviewApplicants, etc.) ...
        
        // GET: Customer/LeaveReview (UPDATED WITH SMART ROUTING)
        [HttpGet]
        public async Task<IActionResult> LeaveReview(string bookingId, string revieweeId)
        {
            var customerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(customerId)) return Unauthorized();

            // SMART ROUTING: Check if they already reviewed this booking
            var existingReview = await _customerService.GetMyReviewByBookingIdAsync(bookingId, customerId);
            
            // If it succeeds, they already wrote one! Redirect them to the edit page.
            if (existingReview.IsSuccess)
            {
                return RedirectToAction(nameof(EditReview), new { bookingId = bookingId });
            }

            // Otherwise, load the blank creation form
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