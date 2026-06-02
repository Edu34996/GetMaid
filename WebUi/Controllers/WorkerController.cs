using System.Security.Claims;
using Core.Abstracts.IServices;
using Core.Concretes.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebUi.Controllers
{
    [Authorize(Roles = "Worker")]
    public class WorkerController : Controller
    {
        private readonly IWorkerService _workerService;

        public WorkerController(IWorkerService workerService)
        {
            _workerService = workerService;
        }

        // GET: Worker/Dashboard
        [HttpGet]
        public async Task<IActionResult> Dashboard()
        {
            var workerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(workerId)) return Unauthorized();

            var result = await _workerService.GetDashboardProfileAsync(workerId);

            if (!result.IsSuccess)
            {
                TempData["ErrorMessage"] = string.Join(" ", result.Messages);
                return View();
            }

            return View(result.Data);
        }

        // POST: Worker/UpdateProfile
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateProfile(WorkerProfileUpdateDTO model)
        {
            if (!ModelState.IsValid) return View("Dashboard", model);

            var workerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(workerId)) return Unauthorized();

            var result = await _workerService.UpdateProfileAsync(workerId, model);

            if (result.IsSuccess)
            {
                TempData["SuccessMessage"] = "Profile updated successfully.";
                return RedirectToAction(nameof(Dashboard));
            }

            ModelState.AddModelError(string.Empty, string.Join(" ", result.Messages));
            return View("Dashboard", model);
        }

        // GET: Worker/JobBoard (open jobs to apply)
        [HttpGet]
        public async Task<IActionResult> JobBoard()
        {
            var workerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(workerId)) return Unauthorized();

            var result = await _workerService.GetOpenJobPostingsAsync(workerId);

            if (!result.IsSuccess)
            {
                TempData["ErrorMessage"] = string.Join(" ", result.Messages);
                return View(new List<JobPostingCardDTO>());
            }

            return View(result.Data);
        }
        
        // GET: Worker/MyJobs
        [HttpGet]
        public async Task<IActionResult> MyJobs()
        {
            var workerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(workerId)) return Unauthorized();

            var result = await _workerService.GetMyAppliedJobsAsync(workerId);

            if (!result.IsSuccess)
            {
                TempData["ErrorMessage"] = string.Join(" ", result.Messages);
                return View(new List<JobPostingCardDTO>());
            }

            return View(result.Data);
        }

        // GET: Worker/ApplyToJob
        [HttpGet]
        public async Task<IActionResult> ApplyToJob(string jobPostingId)
        {
            if (string.IsNullOrWhiteSpace(jobPostingId))
            {
                TempData["ErrorMessage"] = "Job posting ID is required.";
                return RedirectToAction(nameof(JobBoard));
            }

            var workerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(workerId)) return Unauthorized();

            var jobsResult = await _workerService.GetOpenJobPostingsAsync(workerId);
            if (!jobsResult.IsSuccess || jobsResult.Data == null)
            {
                TempData["ErrorMessage"] = string.Join(" ", jobsResult.Messages ?? new[] { "Could not load job posting." });
                return RedirectToAction(nameof(JobBoard));
            }

            var job = jobsResult.Data.FirstOrDefault(j => j.Id == jobPostingId);
            if (job == null)
            {
                TempData["ErrorMessage"] = "Job posting not found or no longer available.";
                return RedirectToAction(nameof(JobBoard));
            }

            ViewBag.JobPostingId = jobPostingId;
            ViewBag.JobTitle = job.Title;
            ViewBag.JobCity = job.City;
            ViewBag.JobAddress = string.Empty;

            return View(new JobApplicationCreateDTO());
        }

        // POST: Worker/ApplyForJob
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApplyForJob(string jobPostingId, JobApplicationCreateDTO model)
        {
            var workerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(workerId)) return Unauthorized();

            if (string.IsNullOrWhiteSpace(jobPostingId))
            {
                TempData["ErrorMessage"] = "Job posting ID is required.";
                return RedirectToAction(nameof(JobBoard));
            }

            if (!ModelState.IsValid)
            {
                var jobsResult = await _workerService.GetOpenJobPostingsAsync(workerId);
                var job = jobsResult.IsSuccess ? jobsResult.Data?.FirstOrDefault(j => j.Id == jobPostingId) : null;

                ViewBag.JobPostingId = jobPostingId;
                ViewBag.JobTitle = job?.Title ?? "Job Posting";
                ViewBag.JobCity = job?.City ?? "N/A";
                ViewBag.JobAddress = string.Empty;

                return View("ApplyToJob", model);
            }

            var result = await _workerService.ApplyForJobAsync(jobPostingId, workerId, model);

            if (result.IsSuccess)
            {
                TempData["SuccessMessage"] = "You have successfully applied for the job!";
                return RedirectToAction(nameof(JobBoard));
            }

            TempData["ErrorMessage"] = string.Join(" ", result.Messages);
            return RedirectToAction(nameof(JobBoard));
        }

        // GET: Worker/MyBookings
        [HttpGet]
        public async Task<IActionResult> MyBookings()
        {
            var workerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(workerId)) return Unauthorized();

            var result = await _workerService.GetMyBookingsAsync(workerId);

            if (!result.IsSuccess)
            {
                TempData["ErrorMessage"] = string.Join(" ", result.Messages);
                return View(new List<BookingListItemDTO>());
            }

            return View(result.Data);
        }
        
        [HttpGet]
        public async Task<IActionResult> BookingDetails(string bookingId)
        {
            var workerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(workerId)) return Unauthorized();

            var result = await _workerService.GetBookingDetailsAsync(bookingId, workerId);

            if (!result.IsSuccess || result.Data == null)
            {
                TempData["ErrorMessage"] = string.Join(" ", result.Messages ?? new[] { "Booking not found." });
                return RedirectToAction(nameof(MyBookings));
            }

            ViewBag.BookingMode = "worker";
            return View("~/Views/Shared/BookingDetails.cshtml", result.Data);
        }

        // POST: Worker/RespondToBooking
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RespondToBooking(string bookingId, bool confirm)
        {
            var workerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(workerId)) return Unauthorized();

            var result = await _workerService.RespondToBookingAsync(bookingId, workerId, confirm);

            if (result.IsSuccess)
            {
                TempData["SuccessMessage"] = confirm ? "Booking confirmed!" : "Booking rejected.";
            }
            else
            {
                TempData["ErrorMessage"] = string.Join(" ", result.Messages);
            }

            return RedirectToAction(nameof(MyBookings));
        }

        // GET: Worker/LeaveReview
        [HttpGet]
        public async Task<IActionResult> LeaveReview(string bookingId, string revieweeId)
        {
            var workerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(workerId)) return Unauthorized();

            var existingReview = await _workerService.GetMyReviewByBookingIdAsync(bookingId, workerId);

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

        // GET: Worker/EditReview
        [HttpGet]
        public async Task<IActionResult> EditReview(string bookingId)
        {
            var workerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(workerId)) return Unauthorized();

            var result = await _workerService.GetMyReviewByBookingIdAsync(bookingId, workerId);

            if (!result.IsSuccess)
            {
                TempData["ErrorMessage"] = "Could not find a review to edit.";
                return RedirectToAction(nameof(MyBookings));
            }

            return View(result.Data);
        }

        // POST: Worker/EditReview
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditReview(ReviewUpdateDTO model)
        {
            if (!ModelState.IsValid) return View(model);

            var workerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(workerId)) return Unauthorized();

            var result = await _workerService.UpdateReviewAsync(model, workerId);

            if (result.IsSuccess)
            {
                TempData["SuccessMessage"] = "Your review was successfully updated!";
                return RedirectToAction(nameof(MyBookings));
            }

            ModelState.AddModelError(string.Empty, string.Join(" ", result.Messages));
            return View(model);
        }

        // POST: Worker/LeaveReview
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LeaveReview(ReviewCreateDTO model)
        {
            if (!ModelState.IsValid) return View(model);

            var workerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(workerId)) return Unauthorized();

            var result = await _workerService.LeaveReviewForCustomerAsync(model, workerId);

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