namespace Invoice.DTOs
{
    public class dtoSaveClientRequest
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string? PostCode { get; set; }
        public string Country { get; set; }
    }
}
