using Core.Abstracts.Bases;

namespace Core.Concretes.Entities
{
    public class Message : BaseEntity
    {
        public string SenderId { get; set; } = null!;
        public string ReceiverId { get; set; } = null!;
        
        public string Content { get; set; } = null!;
        public bool IsRead { get; set; } = false;
    }
}