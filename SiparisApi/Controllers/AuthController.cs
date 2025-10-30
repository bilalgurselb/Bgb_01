using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using SiparisApi.Data;
using SiparisApi.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace SiparisApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
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
        public IActionResult Login([FromBody] User loginUser)
        {
            if (loginUser == null || string.IsNullOrWhiteSpace(loginUser.Email) || string.IsNullOrWhiteSpace(loginUser.Password))
                return BadRequest("Email ve şifre zorunludur.");

            // 🔹 Kullanıcı Users tablosunda var mı kontrol et
            var user = _context.Users.FirstOrDefault(u => u.Email == loginUser.Email);

            if (user == null)
            {
                // 🔹 Kullanıcı bulunamadı → Allowed listede mi kontrol et
                var allowed = _context.AllowedEmails.FirstOrDefault(a => a.Email == loginUser.Email);

                if (allowed != null)
                {
                    // ✅ Allowed listede ama Users tablosunda yok
                    // Frontend bu durumda ikinci şifre kutusunu açar
                    return NotFound("Allowed but not registered");
                }

                // ❌ Ne Users'ta ne Allowed listede
                return BadRequest("Email not allowed");
            }

            // 🔹 Kullanıcı varsa → Şifreyi doğrula
            if (user.Password != loginUser.Password)
                return Unauthorized("Hatalı şifre.");

            // 🔹 Kullanıcı aktif mi?
            if (!user.IsActive)
                return Unauthorized("Hesap aktif değil.");

            // 🔹 JWT Token oluştur
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_config["Jwt:Key"]);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim(ClaimTypes.Role, user.Role ?? "User")
                }),
                Expires = DateTime.UtcNow.AddHours(2),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
                Audience = _config["Jwt:Audience"],
                Issuer = _config["Jwt:Issuer"]
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            var jwtToken = tokenHandler.WriteToken(token);

            // 🔹 Başarılı yanıt dön
            return Ok(new { token = jwtToken });
        }

        // 🔹 SIGNUP (Allowed listede olup yeni kayıt olan kullanıcılar)
        [HttpPost("signup")]
        public IActionResult Signup([FromBody] User signupUser)
        {
            if (signupUser == null || string.IsNullOrWhiteSpace(signupUser.Email) || string.IsNullOrWhiteSpace(signupUser.Password))
                return BadRequest("Email ve şifre zorunludur.");

            // Zaten kayıtlı mı?
            var existingUser = _context.Users.FirstOrDefault(u => u.Email == signupUser.Email);
            if (existingUser != null)
                return BadRequest("Bu kullanıcı zaten kayıtlı.");

            // Allowed listede mi?
            var allowed = _context.AllowedEmails.FirstOrDefault(a => a.Email == signupUser.Email);
            if (allowed == null)
                return Unauthorized("Bu e-posta kayıt listesinde değil.");

            // Yeni kullanıcı oluştur
            var newUser = new User
            {
                Email = signupUser.Email,
                Password = signupUser.Password,
                Role = allowed.Role ?? "User",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _context.Users.Add(newUser);
            _context.SaveChanges();

            // JWT Token oluştur (hemen giriş yapabilsin)
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_config["Jwt:Key"]);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.Email, newUser.Email),
                    new Claim(ClaimTypes.Role, newUser.Role)
                }),
                Expires = DateTime.UtcNow.AddHours(2),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
                Audience = _config["Jwt:Audience"],
                Issuer = _config["Jwt:Issuer"]
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            var jwtToken = tokenHandler.WriteToken(token);

            return Ok(new { token = jwtToken });
        }

        // 🔹 CHECK USER (Allowed / Registered kontrol)
        [HttpGet("checkuser")]
        public IActionResult CheckUser(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return BadRequest("Email zorunludur.");

            var user = _context.Users.FirstOrDefault(u => u.Email == email);
            if (user != null)
                return Ok("Kullanıcı kayıtlı.");

            var allowed = _context.AllowedEmails.FirstOrDefault(a => a.Email == email);
            if (allowed != null)
                return NotFound("Allowed but not registered");

            return BadRequest("Email not allowed");
        }
    }
}
