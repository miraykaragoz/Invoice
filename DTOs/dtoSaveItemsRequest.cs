namespace Invoice.DTOs;

public class dtoSaveItemsRequest
{
    public int Id { get; set; }
    public string Name { get; set; }
    
    public int Quantity { get; set; }
    
    public Double Price { get; set; }
    public Double TotalPrice { get; set; }


}