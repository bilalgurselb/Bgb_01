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
        [HttpGet("checkuser")]
        public IActionResult CheckUser(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return BadRequest("E-posta zorunludur.");

            var user = _context.Users.FirstOrDefault(u => u.Email == email);
            if (user != null)
                return Ok(); // Kullanıcı kayıtlı

            var allowed = _context.AllowedEmails.FirstOrDefault(a => a.Email == email && a.IsActive);
            if (allowed != null)
                return NotFound(); // Allowed listede ama kayıtlı değil

            return BadRequest(); // Hiçbir listede yok
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] User loginUser)
        {
            // 1️⃣ Kullanıcı Users tablosunda var mı?
            var user = _context.Users.FirstOrDefault(u => u.Email == loginUser.Email);

            // 2️⃣ Yoksa AllowedEmails tablosuna bakalım
            if (user == null)
            {
                var allowed = _context.AllowedEmails.FirstOrDefault(x => x.Email == loginUser.Email && x.IsActive);
                if (allowed != null)
                {
                    // 🔹 Allowed listede var → kullanıcı otomatik oluşturulacak
                    var newUser = new User
                    {
                        Email = loginUser.Email,
                        Password = BCrypt.Net.BCrypt.HashPassword(loginUser.Password),
                        Role = string.IsNullOrEmpty(allowed.Role) ? "User" : allowed.Role,
                        IsActive = true
                    };

                    _context.Users.Add(newUser);
                    _context.SaveChanges();

                    user = newUser; // devam etsin
                }
                else
                {
                    return Unauthorized("Bu e-posta ile kayıt yapılamaz.");
                }
            }

            // 📌 Kullanıcı aktif mi?
            if (!user.IsActive)
                return Unauthorized("Bu kullanıcı devre dışı bırakılmış. Giriş yapamaz.");

            // 3️⃣ Parola kontrolü
            var ok = BCrypt.Net.BCrypt.Verify(loginUser.Password, user.Password);
            if (!ok)
                return Unauthorized("E-posta veya şifre hatalı.");

            // 4️⃣ JWT üret
            var key = Encoding.UTF8.GetBytes(_config["Jwt:Key"]);
            var creds = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
        new Claim(ClaimTypes.Email, user.Email),
        new Claim(ClaimTypes.Name, user.Email),
        new Claim(ClaimTypes.Role, user.Role ?? "User")
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
