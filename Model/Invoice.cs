using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Invoice.Model;

public class InvoiceModel
{
    [Key]
    public int Id { get; set; }

    [Required]
    public string InvoiceName { get; set; }
    public string? ProjectDescription { get; set; }
    public DateTime InvoiceDate { get; set; }
    public DateTime PaymentDue { get; set; }
    public Status PaymentStatus { get; set; } = Status.Taslak;
    public Double TotalAmount { get; set; }
    [NotMapped]
    public PaymentTerm PaymentTerm { get; set; }
    public int ClientId { get; set; }

    [ForeignKey("ClientId")]
    public Client? Client { get; set; }
    public ICollection<Item> Items { get; set; } = new List<Item>();
    public bool IsDeleted { get; set; } = false;
    [NotMapped]
    [JsonIgnore]
    public User User { get; set; }
}
public enum PaymentTerm
{
    ErtesiGün,
    Sonraki7Gün,
    Sonraki14Gün,
    Sonraki30Gün
}

public enum Status
{
    Beklemede,
    Ödendi,
    Taslak
}