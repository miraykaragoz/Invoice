using Invoice.Data;
using Invoice.Model;
using Microsoft.AspNetCore.Mvc;

namespace Invoice.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class UserController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public UserController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("/User")]
        public User GetUser()
        {
            return _context.Users.First();
        }

        [HttpPost("/SaveUser")]
        public IActionResult SaveUser(User model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { message = "Eksik bilgiler girdiniz." });
            }
            var data = new User();

            if (model.Id is not 0)
            {
                data = _context.Users.First();
                data.StreetAddress = model.StreetAddress;
                data.City = model.City;
                data.PostCode = model.PostCode;
                data.Country = model.Country;
                _context.Users.Update(data);
            }

            _context.SaveChanges();

            return Ok(data);
        }
    }
}
