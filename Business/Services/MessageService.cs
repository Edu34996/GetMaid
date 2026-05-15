using Core.Abstracts;
using Core.Abstracts.IServices;
using Core.Concretes.DTOs;
using Core.Concretes.Entities;
using Microsoft.AspNetCore.Identity;
using Utils.Responses;

namespace Business.Services
{
    public class MessageService : IMessageService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<ApplicationUser> _userManager;

        public MessageService(IUnitOfWork unitOfWork, UserManager<ApplicationUser> userManager)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
        }

        public async Task<IResult> SendMessageAsync(MessageCreateDTO model, string senderId)
        {
            try
            {
                // Prevent users from messaging themselves
                if (senderId == model.ReceiverId)
                {
                    return Result.Failure(["You cannot send a message to yourself."]);
                }

                var message = new Message
                {
                    SenderId = senderId,
                    ReceiverId = model.ReceiverId,
                    Content = model.Content,
                    IsRead = false
                };

                var createResult = await _unitOfWork.Messages.CreateAsync(message);
                if (!createResult.IsSuccess) return createResult;

                return await _unitOfWork.CommitAsync();
            }
            catch (Exception ex)
            {
                return Result.Failure([ex.Message]);
            }
        }

        public async Task<IResult<IEnumerable<MessageDTO>>> GetChatThreadAsync(string currentUserId, string otherUserId)
        {
            try
            {
                // 1. Fetch all messages where these two users are the sender/receiver
                var messagesResult = await _unitOfWork.Messages.FindManyAsync(m => 
                    (m.SenderId == currentUserId && m.ReceiverId == otherUserId) || 
                    (m.SenderId == otherUserId && m.ReceiverId == currentUserId)
                );

                if (!messagesResult.IsSuccess || messagesResult.Data == null)
                {
                    return Result<IEnumerable<MessageDTO>>.Failure(["Failed to retrieve chat thread."]);
                }

                // 2. We need the names of the users for the UI. 
                // Because Customer and Worker both inherit from ApplicationUser, we can query Identity directly!
                var currentUser = await _userManager.FindByIdAsync(currentUserId);
                var otherUser = await _userManager.FindByIdAsync(otherUserId);

                if (currentUser == null || otherUser == null)
                {
                    return Result<IEnumerable<MessageDTO>>.Failure(["One or both users could not be found."]);
                }

                // 3. Map to DTOs, sort by time, and set the "IsMine" flag for the UI
                var thread = messagesResult.Data
                    .OrderBy(m => m.CreatedAt) // Chronological order (oldest at top, newest at bottom)
                    .Select(m => new MessageDTO
                    {
                        Id = m.Id,
                        SenderId = m.SenderId,
                        ReceiverId = m.ReceiverId,
                        Content = m.Content,
                        CreatedAt = m.CreatedAt,
                        
                        // If the SenderId matches the current user, flag it so the UI puts it on the right side
                        IsMine = m.SenderId == currentUserId, 
                        
                        // Assign the correct first name based on who sent it
                        SenderName = m.SenderId == currentUserId ? currentUser.FirstName : otherUser.FirstName
                    }).ToList();

                // Bonus logic: Mark messages as read here if needed later

                return Result<IEnumerable<MessageDTO>>.Success(thread);
            }
            catch (Exception ex)
            {
                return Result<IEnumerable<MessageDTO>>.Failure([ex.Message]);
            }
        }
        
        public async Task<IResult<IEnumerable<ConversationDTO>>> GetMyInboxAsync(string userId)
        {
            try
            {
                // Fetch all messages involving this user
                var messagesResult = await _unitOfWork.Messages.FindManyAsync(m => 
                    m.SenderId == userId || m.ReceiverId == userId
                );

                if (!messagesResult.IsSuccess || messagesResult.Data == null)
                {
                    return Result<IEnumerable<ConversationDTO>>.Failure(["Failed to load inbox."]);
                }

                // Group by the OTHER user's ID
                var latestMessages = messagesResult.Data
                    .GroupBy(m => m.SenderId == userId ? m.ReceiverId : m.SenderId)
                    .Select(group => group.OrderByDescending(m => m.CreatedAt).First())
                    .ToList();

                var inbox = new List<ConversationDTO>();

                // Map the user names securely
                foreach (var msg in latestMessages)
                {
                    var otherUserId = msg.SenderId == userId ? msg.ReceiverId : msg.SenderId;
                    var otherUser = await _userManager.FindByIdAsync(otherUserId);

                    inbox.Add(new ConversationDTO
                    {
                        OtherUserId = otherUserId,
                        OtherUserName = otherUser?.FirstName ?? "Unknown User",
                        LastMessage = msg.Content,
                        LastMessageAt = msg.CreatedAt,
                        IsRead = msg.IsRead || msg.SenderId == userId // Unread only if THEY sent it
                    });
                }

                // Sort by most recent conversation at the top
                return Result<IEnumerable<ConversationDTO>>.Success(inbox.OrderByDescending(c => c.LastMessageAt));
            }
            catch (Exception ex)
            {
                return Result<IEnumerable<ConversationDTO>>.Failure([ex.Message]);
            }
        }
    }
}