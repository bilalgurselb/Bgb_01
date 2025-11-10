using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using SiparisApi.Data;
using SiparisApi.Dtos;
using SiparisApi.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace SiparisApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _config;

        public AuthController(AppDbContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        // 🔹 LOGIN (Giriş)
        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginDto dto)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrWhiteSpace(dto.Password))
                return BadRequest("E-posta ve şifre zorunludur.");

            // 1️⃣ Kullanıcı var mı?
            var user = _context.Users.FirstOrDefault(u => u.Email == dto.Email);
            if (user == null)
                return Unauthorized("Kullanıcı bulunamadı.");

            // 2️⃣ AllowedEmail kaydını getir
            var allowed = _context.AllowedEmails.FirstOrDefault(a => a.Id == user.AllowedId);
            if (allowed == null)
                return Unauthorized("Bu e-posta sistem erişimine kapalı.");
            if (!allowed.IsActive)
                return Unauthorized("Bu hesap şu anda pasif durumda.");

            // 3️⃣ Şifre kontrolü
            if (!BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
                return Unauthorized("Hatalı şifre.");

            // 4️⃣ Kullanıcı bilgilerini UI’da göstermek için hydrate et
            user.NameSurname = allowed.NameSurname;
            user.Role = allowed.Role;
            user.IsActive = allowed.IsActive;

            // 5️⃣ Token oluştur
            var _key = _config["Jwt:Key"];
            if (string.IsNullOrEmpty(_key))
                throw new InvalidOperationException("❌ JWT anahtarı (Jwt:Key) appsettings.json içinde tanımlı olmalı.");
            var key = Encoding.UTF8.GetBytes(_key);
            var claims = new[]
            {
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, allowed.Role ?? ""),
                new Claim(ClaimTypes.Role, allowed.Role ?? "User"),
                new Claim("Role", allowed.Role ?? ""),
                new Claim("NameSurname", allowed.NameSurname ?? "")
            };

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddHours(8),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
                Issuer = _config["Jwt:Issuer"],
                Audience = _config["Jwt:Audience"]
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);

            return Ok(new
            {
                token = tokenHandler.WriteToken(token),
                User = new
                {
                    user.Email,
                    user.NameSurname,
                    user.Role
                }
            });
        }

        // 🔹 SIGNUP (İlk kayıt — AllowedEmail kontrolü)
        [HttpPost("signup")]
        public IActionResult Signup([FromBody] SignupDto dto)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrWhiteSpace(dto.Password))
                return BadRequest("E-posta ve şifre zorunludur.");

            // 1️⃣ AllowedEmail listesinde mi?
            var allowed = _context.AllowedEmails.FirstOrDefault(a => a.Email == dto.Email);
            if (allowed == null)
                return Unauthorized("Bu e-posta kayıt listesinde bulunmuyor.");

            if (!allowed.IsActive)
                return Unauthorized("Bu hesap aktif değil. Lütfen yöneticinizle iletişime geçin.");

            // 2️⃣ Zaten kayıtlı mı?
            if (_context.Users.Any(u => u.Email == dto.Email))
                return BadRequest("Bu kullanıcı zaten kayıtlı.");

            // 3️⃣ Şifre hash
            var passwordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);

            // 4️⃣ Yeni kullanıcı kaydı
            var newUser = new User
            {
                Email = dto.Email,
                PasswordHash = passwordHash,
                AllowedId = allowed.Id
            };

            _context.Users.Add(newUser);
            _context.SaveChanges();

            return Ok("Kayıt işlemi tamamlandı. Artık giriş yapabilirsiniz.");
        }
    }


}
