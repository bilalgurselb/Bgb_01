using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using SiparisApi.Data;
using SiparisApi.Services;
using System.IdentityModel.Tokens.Jwt;
using System.Text;

namespace SiparisApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DiagnosticsController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly IEmailService _email;
        private readonly IConfiguration _config;

        public DiagnosticsController(AppDbContext db, IEmailService email, IConfiguration config)
        {
            _db = db;
            _email = email;
            _config = config;
        }

        [HttpGet("health")]
        public async Task<IActionResult> Health()
        {
            var result = new Dictionary<string, object>();

            // 🧩 SQL testi
            try
            {
                var canConnect = await _db.Database.CanConnectAsync();
                result["Database"] = canConnect ? "✅ Bağlantı başarılı" : "❌ Erişim yok";
            }
            catch (Exception ex)
            {
                result["Database"] = $"❌ Hata: {ex.Message}";
            }

            // 🔑 JWT testi
            try
            {
                var key = _config["Jwt:Key"];
                if (string.IsNullOrEmpty(key))
                    result["JWT"] = "⚠️ Anahtar bulunamadı (Jwt__Key eksik)";
                else
                {
                    var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
                    var creds = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
                    var token = new JwtSecurityToken(
                        issuer: _config["Jwt:Issuer"],
                        audience: _config["Jwt:Audience"],
                        expires: DateTime.Now.AddMinutes(1),
                        signingCredentials: creds);
                    result["JWT"] = "✅ Geçerli yapı";
                }
            }
            catch (Exception ex)
            {
                result["JWT"] = $"❌ Hata: {ex.Message}";
            }

            // 📧 Mail servisi
            try
            {
                var smtpHost = _config["Email:SmtpServer"];
                result["EmailService"] = !string.IsNullOrEmpty(smtpHost)
                    ? $"✅ Yapılandırılmış ({smtpHost})"
                    : "⚠️ SMTP yapılandırması bulunamadı";
            }
            catch (Exception ex)
            {
                result["EmailService"] = $"❌ Hata: {ex.Message}";
            }

            // 🌐 CORS kontrolü
            try
            {
                var origins = _config.GetSection("CorsOrigins").Get<string[]>() ?? Array.Empty<string>();
                result["CORS"] = origins.Length > 0
                    ? $"✅ {origins.Length} kaynak tanımlı"
                    : "⚠️ CORS listesi boş (tüm kaynaklara açık olabilir)";
            }
            catch (Exception ex)
            {
                result["CORS"] = $"❌ Hata: {ex.Message}";
            }

            // 🔄 Genel durum
            var healthy = result.Values.All(v => v.ToString()?.StartsWith("✅") == true);
            result["Durum"] = healthy ? "🟢 Sistem sağlıklı" : "🟠 Kısmi sorunlar var";

            return Ok(result);
        }
    }
}

