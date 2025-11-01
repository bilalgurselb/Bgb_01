using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SiparisApi.Data;
using SiparisApi.Models;
using System.Security.Cryptography;
using System.Text;

namespace SiparisApi.Controllers
{
    [Authorize(Roles = "Admin")] // 📌 sadece adminler erişebilir
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly AppDbContext _context;

        public UsersController(AppDbContext context)
        {
            _context = context;
        }

        // 📜 Tüm kullanıcıları listele
        [HttpGet]
        public IActionResult GetAllUsers()
        {
            var users = _context.Users.ToList();
            return Ok(users);
        }

        // 👤 Yeni kullanıcı ekle
        [HttpPost]
        public IActionResult AddUser([FromBody] User newUser)
        {
            if (_context.Users.Any(u => u.Email == newUser.Email))
                return BadRequest("Bu e-posta ile kullanıcı zaten kayıtlı.");

            newUser.PasswordHash = HashPassword(newUser.PasswordHash);
            newUser.IsActive = true; // yeni kullanıcı varsayılan olarak aktif gelir

            _context.Users.Add(newUser);
            _context.SaveChanges();

            AddLog("CreateUser", "POST /users", $"Yeni kullanıcı eklendi: {newUser.Email}");

            return Ok(newUser);
        }

        // 🔐 Şifre sıfırla
        [HttpPut("{id}/reset-password")]
        public IActionResult ResetPassword(int id, [FromBody] string newPassword)
        {
            var user = _context.Users.Find(id);
            if (user == null)
                return NotFound("Kullanıcı bulunamadı.");

            user.PasswordHash = HashPassword(newPassword);
            _context.SaveChanges();

            AddLog("ResetPassword", $"PUT /users/{id}/reset-password", $"Kullanıcı şifresi sıfırlandı: {user.Email}");

            return Ok("Şifre başarıyla sıfırlandı.");
        }

        // 🚫 Kullanıcı devre dışı bırak / aktif et
        [HttpPut("{id}/toggle-status")]
        public IActionResult ToggleUserStatus(int id)
        {
            var user = _context.Users.Find(id);
            if (user == null)
                return NotFound("Kullanıcı bulunamadı.");

            user.IsActive = !user.IsActive;
            _context.SaveChanges();

            var status = user.IsActive ? "Aktif edildi" : "Devre dışı bırakıldı";

            AddLog("ToggleUserStatus", $"PUT /users/{id}/toggle-status", $"Kullanıcı durumu değişti: {user.Email} → {status}");

            return Ok($"Kullanıcı {status}.");
        }

        // 🧰 Yardımcı: Şifre hash fonksiyonu
        private string HashPassword(string password)
        {
            using var sha = SHA256.Create();
            var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(bytes);
        }

        // 🧰 Yardımcı: Log kaydı (OrdersController’daki ile aynı mantık)
        private void AddLog(string action, string endpoint, string details)
        {
            var email = User.Identity?.Name ?? "Admin";

            var log = new Log
            {
                UserEmail = email,
                Action = action,
                Endpoint = endpoint,
                Timestamp = DateTime.Now,
                Details = details
            };

            _context.Logs.Add(log);
            _context.SaveChanges();
        }
    }
}
