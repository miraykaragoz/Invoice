using Invoice.Data;
using Invoice.DTOs;
using Invoice.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net.Mail;
using System.Net;
using RestSharp;

namespace Invoice.Controllers;

[ApiController]
[Route("api/[controller]/[action]")]
public class InvoiceController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public InvoiceController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet("/Invoices/{page}")]
    public IActionResult GetInvoice(int page = 1)
    {
        var pageSize = 10;
        
        var totalInvoices = _context.Invoices
            .Where(x => x.IsDeleted == false)
            .Count();
        
        var totalPages = (int)Math.Ceiling((double)totalInvoices / pageSize);
        
        var invoices = _context.Invoices
            .Include(x => x.Client)
            .Include(x => x.Items)
            .Where(x => x.IsDeleted == false)
            .OrderBy(x => x.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();
        
        var users = _context.Users.First();

        var invoiceDetail = invoices.Select(invoice => new dtoGetInvoiceRequest
        {
            Id = invoice.Id,
            InvoiceNumber = invoice.InvoiceName,
            PaymentDue = invoice.PaymentDue,
            Status = invoice.PaymentStatus,
            ProjectDescription = invoice.ProjectDescription,
            InvoiceDate = invoice.InvoiceDate,
            PaymentTerm = invoice.PaymentTerm,
            ClientId = invoice.ClientId,
            UserId = users.Id,
            TotalPages = totalPages,
            Client = new dtoGetClientRequest
            {
                Id = invoice.Client.Id,
                Name = invoice.Client.Name,
                Email = invoice.Client.Email,
                Address = invoice.Client.Address,
                City = invoice.Client.City,
                PostCode = invoice.Client.PostCode,
                Country = invoice.Client.Country,
            },
            User = new User
            {
                Id = users.Id,
                StreetAddress = users.StreetAddress,
                City = users.City,
                PostCode = users.PostCode,
                Country = users.Country,
            },
            Items = invoice.Items.Select(x => new dtoSaveItemsRequest
            {
                Id = x.Id,
                Name = x.Name,
                Quantity = x.Quantity,
                Price = x.Price,
                TotalPrice = x.Quantity * x.Price,
            }).ToList(),
            TotalAmount = invoice.Items.Sum(x => x.Quantity * x.Price),
        }).ToList();

        return Ok(invoiceDetail);
    }

    [HttpPost("/InvoiceDetail/{id}")]
    public IActionResult InvoiceDetail(int id)
    {
        var invoice = _context.Invoices
            .Include(x => x.Client)
            .Include(x => x.Items)
            .FirstOrDefault(x => x.Id == id);

        if (invoice == null)
        {
            return NotFound("Fatura bulunamadı.");
        }

        var invoiceDetail = new InvoiceModel
        {
            Id = id,
            InvoiceName = invoice.InvoiceName,
            InvoiceDate = invoice.InvoiceDate,
            PaymentStatus = invoice.PaymentStatus,
            ClientId = invoice.ClientId,
            IsDeleted = invoice.IsDeleted,
            Client = new Client
            {
                Id = invoice.Client.Id,
                Name = invoice.Client.Name,
                Email = invoice.Client.Email,
                Address = invoice.Client.Address,
                PostCode = invoice.Client.PostCode,
                City = invoice.Client.City,
                Country = invoice.Client.Country,
            },
            Items = invoice.Items.Select(x => new Item
            {
                Id = x.Id,
                Name = x.Name,
                Quantity = x.Quantity,
                Price = x.Price,
                TotalPrice = x.Quantity * x.Price,
            }).ToList(),
            TotalAmount = invoice.Items.Sum(x => x.Quantity * x.Price),
        };

        return Ok(invoiceDetail);
    }

    [HttpPost]
    public IActionResult SaveInvoice([FromBody] dtoSaveInvoinceRequest model)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new { message = "Eksik veya hatalı giriş yaptınız." });
        }
        
        var captchaToken = Request.Form["g-recaptcha-response"];

        if (!VerifyCaptcha(captchaToken))
        {
            return BadRequest(new { message = "Geçersiz reCaptcha" });
        }
        
        if (model.Id is 0)
        {
            var Invoice = new InvoiceModel
            {
                InvoiceName = GenerateInvoiceName(),
                InvoiceDate = model.CreatedTime,
                PaymentStatus = model.PaymentStatus,
                ClientId = model.ClientId,
                ProjectDescription = model.ProjectDescription,
                PaymentDue = CalculatePaymentDue(model.CreatedTime, model.PaymentTerm),
                Items = model.Items.Select(x => new Item
                {
                    Name = x.Name,
                    Quantity = x.Quantity,
                    Price = x.Price,
                    TotalPrice = x.Price * x.Quantity,
                }).ToList(),
                TotalAmount = model.Items.Sum(x => x.Quantity * x.Price),
            };
            _context.Invoices.Add(Invoice);
            _context.SaveChanges();
            SendMail(Invoice);

            return Ok(new { message = "Fatura başarıyla kaydedildi." });
        }
        else
        {
            var invoice = _context.Invoices
                  .Include(x => x.Items)
                  .FirstOrDefault(x => x.Id == model.Id);

            if (invoice == null)
            {
                return NotFound("Fatura bulunamadı.");
            }

            invoice.InvoiceDate = model.CreatedTime;
            invoice.PaymentStatus = model.PaymentStatus;
            invoice.ClientId = model.ClientId;
            invoice.ProjectDescription = model.ProjectDescription;
            invoice.PaymentDue = CalculatePaymentDue(model.CreatedTime, model.PaymentTerm);
            invoice.Items.Clear();
            invoice.Items = model.Items.Select(x => new Item
            {
                Name = x.Name,
                Quantity = x.Quantity,
                Price = x.Price,
                TotalPrice = x.Price * x.Quantity,
            }).ToList();

            invoice.TotalAmount = model.Items.Sum(x => x.Quantity * x.Price);

            _context.Invoices.Update(invoice);
            _context.SaveChanges();

            return Ok(new { message = "Fatura başarıyla güncellendi." });
        }
    }

    [HttpDelete("{id}")]
    public IActionResult DeleteInvoice(int id)
    {
        var invoice = _context.Invoices.FirstOrDefault(x => x.Id == id);

        if (invoice == null)
        {
            return NotFound("Fatura bulunamadı.");
        }
        
        invoice.IsDeleted = true;
        _context.Invoices.Update(invoice);
        _context.SaveChanges();

        return Ok("Fatura başarıyla silindi.");
    }

    [HttpGet("/Invoices/Status/{status}")]
    public IActionResult GetInvoicesByStatus(Status status)
    {
        var invoices = _context.Invoices
            .Include(x => x.Client)
            .Include(x => x.Items)
            .Where(x => x.PaymentStatus == status)
            .ToList();

        var invoiceDtos = invoices.Select(invoice => new InvoiceModel
        {
            Id = invoice.Id,
            InvoiceName = invoice.InvoiceName,
            InvoiceDate = invoice.InvoiceDate,
            PaymentStatus = invoice.PaymentStatus,
            ClientId = invoice.ClientId,
            Client = new Client
            {
                Id = invoice.Client.Id,
                Name = invoice.Client.Name,
                Email = invoice.Client.Email,
                Address = invoice.Client.Address,
                PostCode = invoice.Client.PostCode,
                City = invoice.Client.City,
                Country = invoice.Client.Country
            },
            Items = invoice.Items.Select(item => new Item
            {
                Id = item.Id,
                Name = item.Name,
                Quantity = item.Quantity,
                Price = item.Price,
                TotalPrice = item.Quantity * item.Price,
            }).ToList()
        }).ToList();

        return Ok(invoiceDtos);
    }

    [ApiExplorerSettings(IgnoreApi = true)]
    public string GenerateInvoiceName()
    {
        Random random = new Random();

        char firstLetter = (char)random.Next('A', 'Z' + 1);
        char secondLetter = (char)random.Next('A', 'Z' + 1);
        int number = random.Next(1000, 9999);
        var invoiceName = _context.Invoices.FirstOrDefault(x => x.InvoiceName == $"#{firstLetter}{secondLetter}{number}");
        if (invoiceName != null)
        {
            char firstLetterAgain = (char)random.Next('A', 'Z' + 1);
            char secondLetterAgain = (char)random.Next('A', 'Z' + 1);
            int numberAgain = random.Next(1000, 9999);

            return $"#{firstLetter}{secondLetter}{number}";
        }
        else
        {
            return $"#{firstLetter}{secondLetter}{number}";
        }
    }

    [ApiExplorerSettings(IgnoreApi = true)]
    public DateTime CalculatePaymentDue(DateTime createdTime, PaymentTerm paymentTerm)
    {
        if (paymentTerm == PaymentTerm.ErtesiGün)
        {
            return createdTime.AddDays(1);
        }
        else if (paymentTerm == PaymentTerm.Sonraki7Gün)
        {
            return createdTime.AddDays(7);

        }
        else if (paymentTerm == PaymentTerm.Sonraki14Gün)
        {
            return createdTime.AddDays(14);
        }
        else
        {
            return createdTime.AddDays(30);
        }
    }
    
    [ApiExplorerSettings(IgnoreApi = true)]
    public IActionResult SendMail(InvoiceModel invoice)
    {
        var client = _context.Clients.FirstOrDefault(x => x.Id == invoice.ClientId);
        
        if (client == null)
        {
            return NotFound("Kullanıcı bulunamadı.");
        }
        
        var invoiceDetails = $@"
            <h1>Fatura Detayları</h1>
            <p><strong>Fatura Adı:</strong> {invoice.InvoiceName}</p>
            <p><strong>Fatura Tarihi:</strong> {invoice.InvoiceDate.ToShortDateString()}</p>
            <p><strong>Ödeme Durumu:</strong> {invoice.PaymentStatus}</p>
            <p><strong>Son Ödeme Tarihi:</strong> {invoice.PaymentDue.ToShortDateString()}</p>
            <p><strong>Açıklama:</strong> {invoice.ProjectDescription}</p>
            <h2>Ürünler</h2>
            <ul>
        ";
        
        foreach (var item in invoice.Items)
        {
            invoiceDetails += $@"
                <li>
                    {item.Name} - {item.Quantity} x {item.Price:C} = {item.TotalPrice:C}
                </li>";
        }

        invoiceDetails += "</ul>";

        var smtpClient = new SmtpClient("smtp.eu.mailgun.org", 587)
        {
            Credentials = new NetworkCredential("",
                ""),
            EnableSsl = true
        };

        var mailMessage = new MailMessage()
        {
            From = new MailAddress("", "Invoice App"),
            Subject = "Yeni Fatura Bilgileri",
            Body = invoiceDetails,
            IsBodyHtml = true
        };

        mailMessage.To.Add(new MailAddress(client.Email, client.Name));

        smtpClient.Send(mailMessage);
        return Ok(new { message = "Mail başarıyla gönderildi." });
    }
    
    [ApiExplorerSettings(IgnoreApi = true)]
    public bool VerifyCaptcha(string captchaToken)
    {
        var client = new RestClient("https://www.google.com/recaptcha");
        var request = new RestRequest("api/siteverify", Method.Post);
        request.AddParameter("secret", "");
        request.AddParameter("response", captchaToken);

        var response = client.Execute<CaptchaResponse>(request);

        if (response.Data.Success && response.Data.Score > 0.6)
        {
            return true;
        }
        return false;
    }
}