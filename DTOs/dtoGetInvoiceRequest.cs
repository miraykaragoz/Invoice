using Invoice.Model;

namespace Invoice.DTOs
{
    public class dtoGetInvoiceRequest
    {
    public int Id { get; set; }
        public Status Status { get; set; }
        public string InvoiceNumber { get; set; }
        public DateTime InvoiceDate { get; set; }
        public DateTime PaymentDue { get; set; }
        public int ClientId { get; set; }
        public dtoGetClientRequest? Client { get; set; }
        public Double TotalAmount { get; set; } 
        public string ProjectDescription { get; set; }
        public PaymentTerm PaymentTerm { get; set; }
        public List<dtoSaveItemsRequest> Items { get; set; }
        public int UserId { get; set; }
        public User User { get; set; }
        public int TotalPages { get; set; }
    }
}
