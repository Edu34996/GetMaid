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
        public async Task<IActionResult> Dashboard()
        {
            var workerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(workerId)) return Unauthorized();

            // FIX: IWorkerService defines GetDashboardProfileAsync
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

        // GET: Worker/JobBoard
        public async Task<IActionResult> JobBoard()
        {
            var workerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(workerId)) return Unauthorized();

            var result = await _workerService.GetOpenJobPostingsAsync(workerId);

            if (!result.IsSuccess)
            {
                TempData["ErrorMessage"] = string.Join(" ", result.Messages);
                // FIX: service returns JobPostingCardDTO list
                return View(new List<JobPostingCardDTO>());
            }

            return View(result.Data);
        }

        // POST: Worker/ApplyForJob
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApplyForJob(string jobId)
        {
            var workerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(workerId)) return Unauthorized();

            var result = await _workerService.ApplyForJobAsync(jobId, workerId);

            if (result.IsSuccess)
            {
                TempData["SuccessMessage"] = "You have successfully applied for the job!";
            }
            else
            {
                TempData["ErrorMessage"] = string.Join(" ", result.Messages);
            }

            return RedirectToAction(nameof(JobBoard));
        }

        // GET: Worker/MyBookings
        public async Task<IActionResult> MyBookings()
        {
            var workerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(workerId)) return Unauthorized();

            var result = await _workerService.GetMyBookingsAsync(workerId);

            if (!result.IsSuccess)
            {
                TempData["ErrorMessage"] = string.Join(" ", result.Messages);
                // FIX: service returns BookingListItemDTO list
                return View(new List<BookingListItemDTO>());
            }

            return View(result.Data);
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