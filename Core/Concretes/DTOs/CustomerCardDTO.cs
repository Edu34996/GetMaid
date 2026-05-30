using Core.Concretes.Enums;

namespace Core.Concretes.DTOs
{
    /// <summary>
    /// Lightweight customer card for use in lists/grids.
    /// Used when displaying customer info in booking/application contexts.
    /// </summary>
    public class CustomerCardDTO
    {
        public string Id { get; set; } = null!;
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;

        public string City { get; set; } = null!;
        public FamilyStatus FamilyStatus { get; set; }
        public bool HasPets { get; set; }
    }
}