using Invoice.Model;

namespace Invoice.DTOs;

public class dtoSaveInvoinceRequest
{
    public int Id { get; set; }
    public string ProjectDescription { get; set; }
    public DateTime CreatedTime { get; set; }
    public Status PaymentStatus { get; set; }
    public PaymentTerm PaymentTerm { get; set; }
    public int ClientId { get; set; }
    public ICollection<dtoSaveItemsRequest>? Items { get; set; }
}