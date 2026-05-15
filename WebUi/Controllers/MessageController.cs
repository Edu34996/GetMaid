using System.Security.Claims;
using Core.Abstracts.IServices;
using Core.Concretes.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebUi.Controllers
{
    // Both Customers and Workers can access this controller
    [Authorize(Roles = "Customer,Worker")]
    public class MessageController : Controller
    {
        private readonly IMessageService _messageService;

        public MessageController(IMessageService messageService)
        {
            _messageService = messageService;
        }

        // GET: Message/Chat?otherUserId=xxx
        [HttpGet]
        public async Task<IActionResult> Chat(string otherUserId)
        {
            if (string.IsNullOrEmpty(otherUserId)) 
            {
                TempData["ErrorMessage"] = "User not specified.";
                // Fallback to home if they somehow navigate here without an ID
                return RedirectToAction("Index", "Home"); 
            }

            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(currentUserId)) return Unauthorized();

            var result = await _messageService.GetChatThreadAsync(currentUserId, otherUserId);

            if (!result.IsSuccess)
            {
                TempData["ErrorMessage"] = string.Join(" ", result.Messages);
            }

            // We pass the ReceiverId via ViewBag so our HTML form knows who to send the next message to
            ViewBag.ReceiverId = otherUserId;
            
            // We pass the chronological list of messages as the primary view model
            return View(result.Data ?? new List<MessageDTO>());
        }

        // POST: Message/SendMessage
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendMessage(string receiverId, string content)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(currentUserId)) return Unauthorized();

            if (string.IsNullOrWhiteSpace(content))
            {
                TempData["ErrorMessage"] = "Message cannot be empty.";
                return RedirectToAction(nameof(Chat), new { otherUserId = receiverId });
            }

            var model = new MessageCreateDTO 
            {
                ReceiverId = receiverId,
                Content = content
            };

            var result = await _messageService.SendMessageAsync(model, currentUserId);

            if (!result.IsSuccess)
            {
                TempData["ErrorMessage"] = string.Join(" ", result.Messages);
            }

            // Instantly redirect back to the same chat thread to see the new message
            return RedirectToAction(nameof(Chat), new { otherUserId = receiverId });
        }
        // GET: Message/Inbox
        [HttpGet]
        public async Task<IActionResult> Inbox()
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(currentUserId)) return Unauthorized();

            var result = await _messageService.GetMyInboxAsync(currentUserId);

            if (!result.IsSuccess)
            {
                TempData["ErrorMessage"] = string.Join(" ", result.Messages);
            }

            return View(result.Data ?? new List<ConversationDTO>());
        }
    }
}