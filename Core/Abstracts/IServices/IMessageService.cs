using Core.Concretes.DTOs;
using Utils.Responses;

namespace Core.Abstracts.IServices
{
    public interface IMessageService
    {
        // Sends a new message to the database
        Task<IResult> SendMessageAsync(MessageCreateDTO model, string senderId);
        
        // Pulls the entire chronological chat history between two specific users
        Task<IResult<IEnumerable<MessageDTO>>> GetChatThreadAsync(string currentUserId, string otherUserId);
    }
}