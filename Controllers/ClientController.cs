using Invoice.Data;
using Invoice.DTOs;
using Invoice.Model;
using Microsoft.AspNetCore.Mvc;

namespace Invoice.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class ClientController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ClientController(ApplicationDbContext context)
        {
            _context = context;
        }
        
        [HttpGet]
        public IActionResult GetClients()
        {
            var users = _context.Clients.ToList();
            
            return Ok(users);
        }
        
        [HttpGet("/Client/{FullName}")]
        public List<Client> GetClient(string FullName)
        {
            var clients = _context.Clients
                         .Where(x => x.Name.Contains(FullName))
                         .ToList();
            
            return clients;
        }

        [HttpPost("/SaveClient")]
        public IActionResult SaveClient(dtoSaveClientRequest model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { message = "Eksik veya hatalı giriş yaptınız." });
            }
            
            var data = new Client();

            if (model.Id is not 0)
            {
                data = _context.Clients.Find(model.Id);
                data.Name = model.Name;
                data.Email = model.Email;
                data.Address = model.Address;
                data.City = model.City;
                data.PostCode = model.PostCode;
                data.Country = model.Country;
                _context.Clients.Update(data);
            }
            else
            {
                data.Name = model.Name;
                data.Email = model.Email;
                data.Address = model.Address;
                data.City = model.City;
                data.PostCode = model.PostCode;
                data.Country = model.Country;
                _context.Clients.Add(data);
            }

            _context.SaveChanges();

            return Ok("Müşteri Başarıyla eklendi.");
        }

        [HttpDelete("{id}")]
        public IActionResult DeleteClient(int id)
        {
            var client = _context.Clients.Find(id);
            
            if (client == null)
            {
                return NotFound("Müşteri bulunamadı.");
            }
            
            _context.Clients.Remove(client);
            _context.SaveChanges();
            
            return Ok("Müşteri başarıyla silindi.");
        }
    }
}
