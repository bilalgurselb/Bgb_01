using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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

        [HttpPost("signup")]
        public IActionResult Signup([FromBody] User newUser)
        {
            // 1) Yetkili e-posta kontrolü: sabit liste yerine DB
            var allowed = _context.AllowedEmails
     .FirstOrDefault(x => x.Email == newUser.Email && x.IsActive);

            if (string.IsNullOrWhiteSpace(newUser.Email))
                return BadRequest("E-posta adresi zorunludur.");

            if (allowed==null)
                return BadRequest("Bu e-posta ile kayıt yapılamaz.");

            // 2) Tekil e-posta
            if (_context.Users.Any(u => u.Email == newUser.Email))
                return BadRequest("Bu e-posta ile daha önce kayıt yapılmış.");

            // 🔹 Rol belirleme (Ledger'da NULL olsa bile kodda kesinleştiriyoruz)
            string role;
            if (newUser.Email.Equals("bborekci@sintankimya.com", StringComparison.OrdinalIgnoreCase))
                role = "Admin";
            else
                role = string.IsNullOrEmpty(allowed.Role) ? "User" : allowed.Role;

            newUser.Password = BCrypt.Net.BCrypt.HashPassword(newUser.Password);
            newUser.Role = role;
            newUser.IsActive = allowed.IsActive;

            _context.Users.Add(newUser);
            _context.SaveChanges();

            return Ok("Kayıt başarılı!");
        }
        [HttpPost("login")]
        public IActionResult Login([FromBody] User loginUser)
        {
            // 1) Kullanıcıyı e-posta ile bul
            var user = _context.Users.FirstOrDefault(u => u.Email == loginUser.Email);
            if (user == null)
                return Unauthorized("E-posta veya şifre hatalı.");

            // 📌 Kullanıcı aktif mi?
            if (!user.IsActive)
                return Unauthorized("Bu kullanıcı devre dışı bırakılmış. Giriş yapamaz.");

            // 2) Parolayı doğrula (hash karşılaştırma)
            var ok = BCrypt.Net.BCrypt.Verify(loginUser.Password, user.Password);
            if (!ok)
                return Unauthorized("E-posta veya şifre hatalı.");

            // 3) JWT üret (Rol ve Email claim’lerini ekliyoruz)
            var key = Encoding.UTF8.GetBytes(_config["Jwt:Key"]);
            var creds = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
        new Claim(ClaimTypes.Email, user.Email),
         new Claim(ClaimTypes.Name, user.Email),
        new Claim(ClaimTypes.Role, user.Role ?? "User") // 📌 Rol claim'i eklendi
    };

            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.UtcNow.AddHours(6),
                signingCredentials: creds
            );

            var accessToken = new JwtSecurityTokenHandler().WriteToken(token);
            return Ok(new { access_token = accessToken, token_type = "Bearer", role = user.Role });
        }


    }
}
