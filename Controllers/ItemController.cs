using Invoice.Data;
using Invoice.DTOs;
using Invoice.Model;
using Microsoft.AspNetCore.Mvc;

namespace Invoice.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class ItemController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ItemController(ApplicationDbContext context)
        {
            _context = context;
        }
        
        [HttpGet("/Items")]
        public List<Item> GetItems()
        {
            return _context.Items.ToList();
        }

        [HttpPost("/SaveItems")]
        public IActionResult SaveItems([FromBody] dtoSaveItemsRequest model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { message = "Eksik veya hatalı giriş yaptınız." });
            }
            
            var data = new Item();
            
            data.TotalPrice = data.Quantity * data.Price;
            
            if (model.Id is not 0)
            {
                data = _context.Items.Find(model.Id);
                data.Name = model.Name;
                data.Price = model.Price;
                data.InvoiceId = model.Id;
                data.Quantity = model.Quantity;
                _context.Items.Update(data);
            }
            else
            {
                data.Name = model.Name;
                data.Price = model.Price;
                data.Quantity = model.Quantity;
                _context.Items.Add(data);
            }

            _context.SaveChanges();

            return Ok("Ürün başarıyla eklendi.");
        }
        
        [HttpDelete("{id}")]
        public string DeleteItem(int id)
        {
            try
            {
                var data = _context.Items.Find(id);
                
                _context.Remove(data);
                _context.SaveChanges();
                
                return "Ürün başarıyla silindi.";
            }
            catch (Exception e)
            {
                return "Silme işlemi sırasında bir hata oluştu.\n" + e.Message;
            }
        }
    }
}
