using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SiparisApi.Data;
using SiparisApi.Models;
using SiparisApi.Services;
using System.Security.Claims;

namespace SiparisApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrdersController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IEmailService _emailService;

        public OrdersController(AppDbContext context, IEmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        // 🔹 1️⃣ Yeni Sipariş Oluşturma
        [Authorize]
        [HttpPost("create")]
        public async Task<IActionResult> CreateOrder([FromBody] OrderHeader order)
        {
            var userEmail = User.FindFirst(ClaimTypes.Email)?.Value;
            var user = await _context.Users
                .Include(u => u.AllowedEmail)
                .FirstOrDefaultAsync(u => u.Email == userEmail);

            if (user == null)
                return Unauthorized("Kullanıcı bulunamadı.");

            order.CreatedById = user.Id;
            order.CreatedAt = DateTime.UtcNow;
            order.UpdatedAt = DateTime.UtcNow;
            order.IsNew = true;
            order.IsUpdated = false;

            _context.OrderHeaders.Add(order);
            await _context.SaveChangesAsync();

            // 🔸 Log kaydı
            _context.Logs.Add(new Log
            {
                UserId = user.Id,
                Action = "CreateOrder",
                Endpoint = "/api/orders/create",
                Details = $"{user.Email} yeni bir sipariş oluşturdu (ID: {order.Id}).",
                Timestamp = DateTime.UtcNow
            });
            await _context.SaveChangesAsync();

            // 🔸 Aktif kullanıcıların e-posta adreslerini al
            var recipients = await _context.Users
                .Include(u => u.AllowedEmail)
                .Where(u => u.AllowedEmail != null && u.AllowedEmail.IsActive)
                .Select(u => u.Email)
                .ToListAsync();

            // 🔸 Mail gönderimi
            foreach (var mail in recipients)
            {
                try
                {
                    await _emailService.SendEmailAsync(
                        mail,
                        "Yeni Sipariş Oluşturuldu",
                        $"<b>{user.AllowedEmail?.NameSurname}</b> tarafından yeni sipariş oluşturuldu.<br>" +
                        $"Sipariş ID: <b>{order.Id}</b><br>" +
                        $"Tarih: {order.CreatedAt:dd.MM.yyyy HH:mm}",
                        isHtml: true
                    );
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Mail gönderimi başarısız ({mail}): {ex.Message}");
                }
            }

            return Ok(new { message = "Sipariş başarıyla oluşturuldu.", order.Id });
        }

        // 🔹 2️⃣ Sipariş Güncelleme
        [Authorize]
        [HttpPut("update/{id}")]
        public async Task<IActionResult> UpdateOrder(int id, [FromBody] OrderHeader dto)
        {
            var userEmail = User.FindFirst(ClaimTypes.Email)?.Value;
            var user = await _context.Users
                .Include(u => u.AllowedEmail)
                .FirstOrDefaultAsync(u => u.Email == userEmail);

            if (user == null)
                return Unauthorized();

            var order = await _context.OrderHeaders.FirstOrDefaultAsync(o => o.Id == id);
            if (order == null)
                return NotFound("Sipariş bulunamadı.");

            var role = user.AllowedEmail?.Role ?? "User";

            // 🔒 Yönetici tarafından onaylanmış/üretimdeyse sadece admin değiştirebilir
            if (order.Status == "Üretimde" || order.Status == "Onaylandı")
            {
                if (role != "Admin")
                    return Forbid("Bu sipariş üretimde/onaylı. Sadece admin düzenleyebilir.");
            }

            // 🔧 Güncelleme
            order.UpdatedAt = DateTime.UtcNow;
            order.PortOfDelivery = dto.PortOfDelivery;
            order.PlaceOfDelivery = dto.PlaceOfDelivery;
            order.IsUpdated = true;
            order.IsNew = false;

            await _context.SaveChangesAsync();

            // 🔸 Log kaydı
            _context.Logs.Add(new Log
            {
                UserId = user.Id,
                Action = "UpdateOrder",
                Endpoint = $"/api/orders/update/{id}",
                Details = $"{user.Email} siparişi güncelledi (ID: {id}).",
                Timestamp = DateTime.UtcNow
            });
            await _context.SaveChangesAsync();

            // 🔸 Aktif kullanıcıların e-posta adreslerini al
            var recipients = await _context.Users
                .Include(u => u.AllowedEmail)
                .Where(u => u.AllowedEmail != null && u.AllowedEmail.IsActive)
                .Select(u => u.Email)
                .ToListAsync();

            // 🔸 Mail bildirimi
            foreach (var mail in recipients)
            {
                try
                {
                    await _emailService.SendEmailAsync(
                        mail,
                        "Sipariş Güncellendi",
                        $"<b>{user.AllowedEmail?.NameSurname}</b> tarafından sipariş (ID: {id}) güncellendi.<br>" +
                        $"Tarih: {order.UpdatedAt:dd.MM.yyyy HH:mm}",
                        isHtml: true
                    );
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Mail gönderimi başarısız ({mail}): {ex.Message}");
                }
            }

            return Ok("Sipariş başarıyla güncellendi.");
        }

        // 🔹 3️⃣ Sipariş Listeleme
        [Authorize]
        [HttpGet("list")]
        public async Task<IActionResult> GetOrders()
        {
            var userEmail = User.FindFirst(ClaimTypes.Email)?.Value;
            var user = await _context.Users
                .Include(u => u.AllowedEmail)
                .FirstOrDefaultAsync(u => u.Email == userEmail);

            if (user == null)
                return Unauthorized();

            var role = user.AllowedEmail?.Role ?? "User";

            var orders = await _context.OrderHeaders
                .Include(o => o.CreatedBy)
                .Include(o => o.Items)
                .Select(o => new
                {
                    o.Id,
                    o.CustomerId,
                    o.Status,
                    o.CreatedAt,
                    o.UpdatedAt,
                    o.IsNew,
                    o.IsUpdated,
                    CreatedByEmail = o.CreatedBy != null ? o.CreatedBy.Email : "-",
                    TotalPrice = role == "Admin" || role == "Yönetici"
                        ? o.Items.Sum(i => i.Price * i.Quantity)
                        : (o.CreatedBy != null && o.CreatedBy.Email == user.Email
                            ? o.Items.Sum(i => i.Price * i.Quantity)
                            : (decimal?)null)
                })
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();

            // 🔸 Log
            _context.Logs.Add(new Log
            {
                UserId = user.Id,
                Action = "ListOrders",
                Endpoint = "/api/orders/list",
                Details = $"{user.Email} ({role}) sipariş listesini görüntüledi.",
                Timestamp = DateTime.UtcNow
            });
            await _context.SaveChangesAsync();

            return Ok(orders);
        }
        // 🔹 Tekil Sipariş Getir (Order Detail)
        [Authorize]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetOrder(int id)
        {
            var userEmail = User.FindFirst(ClaimTypes.Email)?.Value;
            var user = await _context.Users
                .Include(u => u.AllowedEmail)
                .FirstOrDefaultAsync(u => u.Email == userEmail);

            if (user == null)
                return Unauthorized();

            var order = await _context.OrderHeaders
                .Include(o => o.CreatedBy)
                .Include(o => o.Items)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
                return NotFound("Sipariş bulunamadı.");

            // Log kaydı
            _context.Logs.Add(new Log
            {
                UserId = user.Id,
                Action = "GetOrder",
                Endpoint = $"/api/orders/{id}",
                Details = $"{user.Email} sipariş detayını görüntüledi (ID: {id})",
                Timestamp = DateTime.UtcNow
            });
            await _context.SaveChangesAsync();

            return Ok(order);
        }
        // ================================================
        // 🔹 Lookup API'leri (Müşteri, Ürün, Satış Temsilcisi)
        // ================================================

        [HttpGet("lookups/customers")]
        public IActionResult GetCustomers()
        {
            try
            {
                var list = _context.SintanCari
                     .AsNoTracking()
                    .Select(c => new
                    {
                        id = c.CARI_KOD,
                        name = c.CARI_ISIM,
                        city  =  c.ILCE,
                        country = c.IL,
                        phone = c.TELEFON
                    })
                    .ToList();

                return Ok(list);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpGet("lookups/products")]
    //    [AllowAnonymous]
        public IActionResult GetAllProducts()
        {
            var list = _context.SintanStok
                .AsNoTracking()
                .Select(x => new
                {
                    id = x.STOK_KODU,
                    name = x.STOK_ADI,
                    packWeight = x.AMBALAJ_AGIRLIGI,
                    palletCount = x.PALET_AMBALAJ_ADEDI,
                    palletNet = x.PALET_NET_AGIRLIGI,
                    transportCost = x.NAKLIYET_TUT
                })
                .ToList();

            return Ok(list);
        }



        [HttpGet("lookups/salesreps")]
        public IActionResult GetSalesReps()
        {
            try
            {
                var list = _context.AllowedEmails
                    .Select(u => new
                    {
                        id = u.Id,
                        name = u.NameSurname
                    })
                    .ToList();

                return Ok(list);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }
        [HttpGet("lookups/cities")]
        public IActionResult GetCities()
        {
            try
            {
                var list = _context.SintanCari
                    .Where(c => c.IL != null && c.IL.Trim() != "")
                    .Select(static c => c.IL.Trim())
                    .Distinct()
                    .OrderBy(c => c)
                    .ToList();

                return Ok(list);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

    }
}
