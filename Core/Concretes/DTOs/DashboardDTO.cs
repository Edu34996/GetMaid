namespace Core.Concretes.DTOs
{
    public class ChildDTO
    {
        public int Id { get; set; }
        public string FirstName { get; set; } = null!;
        public int Age { get; set; }
        public string? SpecialCareInstructions { get; set; }
    }

    public class CustomerDTO
    {
        public string Id { get; set; } = null!;
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public string Address { get; set; } = null!;
        public string City { get; set; } = null!;
    }
}