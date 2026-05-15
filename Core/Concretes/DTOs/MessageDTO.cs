using System.ComponentModel.DataAnnotations;

namespace Core.Concretes.DTOs
{
    // Used when a user types a message and hits "Send"
    public class MessageCreateDTO
    {
        [Required]
        public string ReceiverId { get; set; } = null!;

        [Required(ErrorMessage = "Message cannot be empty.")]
        [MaxLength(1000)]
        public string Content { get; set; } = null!;
    }

    // Used to display the threaded chat bubbles on the screen
    public class MessageDTO
    {
        public string Id { get; set; } = null!;
        public string SenderId { get; set; } = null!;
        public string ReceiverId { get; set; } = null!;
        public string SenderName { get; set; } = null!;
        public string Content { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
        public bool IsMine { get; set; } // UI Flag: True = Right Chat Bubble, False = Left Chat Bubble
    }
    
    public class ConversationDTO
    {
        public string OtherUserId { get; set; } = null!;
        public string OtherUserName { get; set; } = null!;
        public string LastMessage { get; set; } = null!;
        public DateTime LastMessageAt { get; set; }
        public bool IsRead { get; set; }
    }
}