using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using SiparisApi.Data;
using SiparisApi.Dtos;
using SiparisApi.Models;
using SiparisApi.Services;

namespace SiparisApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class OrdersController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IEmailService _email;

        public OrdersController(AppDbContext context, IEmailService email)
        {
            _context = context;
            _email = email;
        }

        [HttpPost("create")]
        public async Task<IActionResult> Create([FromBody] OrderDto dto)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.Product) || dto.Quantity <= 0)
                return BadRequest("Eksik/Geçersiz sipariş verisi.");

            var userEmail = User.FindFirst("email")?.Value
                            ?? User.FindFirst("sub")?.Value
                            ?? "unknown@sintan.com";

            var order = new Order
            {
                Customer = dto.Customer,
                Product = dto.Product,
                Quantity = dto.Quantity,
                Unit = dto.Unit,
                Price = dto.Price,
                Currency = dto.Currency,
                OrderDate = DateTime.UtcNow,
                CreatedBy = userEmail,
                CreatedAt = DateTime.UtcNow,
                IsApprovedByFactory = false,
                IsApprovedBySales = false,
                IsChanged = false

            };

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            // ✅ Sipariş kaydedildikten sonra mail gönder
            try
            {
                var subject = "Yeni Sipariş Kaydı";
                var body = $@"Yeni sipariş oluşturuldu.

Ürün: {order.Product}
Miktar: {order.Quantity}
Birim Fiyat: {order.Price}
Oluşturan: {order.CreatedBy}
Tarih (UTC): {order.CreatedAt:yyyy-MM-dd HH:mm}
Sipariş ID: {order.Id}";

                await _email.SendEmailAsync(
                    "uretim@sintan.com", // Mail gönderilecek adres
                    subject,
                    body
                );
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Mail gönderim hatası: {ex.Message}");
            }

            try
            {
                var subject = "Yeni Sipariş Kaydı";
                var body =
$@"Yeni sipariş oluşturuldu.

Ürün: {order.Product}
Miktar: {order.Quantity}
Birim Fiyat: {order.Price}
Oluşturan: {order.CreatedBy}
Tarih (UTC): {order.CreatedAt:yyyy-MM-dd HH:mm}
Sipariş ID: {order.Id}";

                await _email.SendEmailAsync("uretim@sintan.com", subject, body);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[MAIL ERROR] {ex.Message}");
                // mail hatası siparişi bozmaz
            }

            return Ok(new { Message = "Sipariş kaydedildi ve mail gönderildi.", OrderId = order.Id });
        }
    }
}
