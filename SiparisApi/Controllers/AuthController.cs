using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
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

       
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {

            if (dto == null || string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrWhiteSpace(dto.Password))
                return BadRequest("E-posta ve şifre zorunludur.");

                     var user = _context.Users.FirstOrDefault(u => u.Email == dto.Email);
                       if (user == null )
                return Unauthorized("Kullanıcı bulunamadı.");
            

            var allowed = _context.AllowedEmails.FirstOrDefault(a => a.Id == user.AllowedId);
            if (allowed == null)
                return Unauthorized("Bu e-posta sistem erişimine kapalı.");
            if (!allowed.IsActive)
                return Unauthorized("Bu hesap şu anda pasif durumda.");


            if (!BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
                return Unauthorized("Hatalı şifre.");

            user.NameSurname = allowed.NameSurname;
            user.Role = allowed.Role;
            user.IsActive = allowed.IsActive;

            var _key = _config["Jwt:Key"];
            if (string.IsNullOrEmpty(_key))
                throw new InvalidOperationException("❌ JWT anahtarı (Jwt:Key) appsettings.json içinde tanımlı olmalı.");

            var key = Encoding.UTF8.GetBytes(_key);
            var claims = new[]
            {
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, allowed.Role ?? ""),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
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
            var jwt = tokenHandler.WriteToken(token);

            // 🔐 Cookie Authentication için giriş (Razor tarafı için)
            var cookieClaims = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(cookieClaims));

            // 🧠 Token'ı Session'a ve Cookie'ye yaz
            HttpContext.Session.SetString("AccessToken", jwt);
            Response.Cookies.Append("AccessToken", jwt, new CookieOptions
            {
                HttpOnly = false,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTimeOffset.UtcNow.AddHours(8)
            });

            return Ok(new
            {
                token = jwt,
                User = new
                {
                    user.Email,
                    user.NameSurname,
                    user.Role
                }
            });
        }

        [HttpPost("signup")]
        public IActionResult Signup([FromBody] SignupDto dto)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrWhiteSpace(dto.Password))
                return BadRequest("E-posta ve şifre zorunludur.");

            var allowed = _context.AllowedEmails.FirstOrDefault(a => a.Email == dto.Email);
            if (allowed == null)
                return Unauthorized("Bu e-posta kayıt listesinde bulunmuyor.");

            if (!allowed.IsActive)
                return Unauthorized("Bu hesap aktif değil. Lütfen yöneticinizle iletişime geçin.");

            if (_context.Users.Any(u => u.Email == dto.Email))
                return BadRequest("Bu kullanıcı zaten kayıtlı.");

            var passwordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);

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
