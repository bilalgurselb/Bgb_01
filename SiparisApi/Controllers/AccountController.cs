using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SiparisApi.Data;
using SiparisApi.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace SiparisApi.Controllers
{
    public class AccountController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _config;
        private readonly AppDbContext _context;

        public AccountController(IHttpClientFactory httpClientFactory, IConfiguration config, AppDbContext context)
        {
            _httpClientFactory = httpClientFactory;
            _config = config;
            _context = context;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string email, string password, string confirmPassword)
        {
            // 🔹 Giriş kontrolü

            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                ViewBag.Error = "E-posta ve şifre zorunludur.";
                return View();
            }
            // ➤ Ön kontrol (login API'ye gitmeden)
            var user = _context.Users.FirstOrDefault(u => u.Email == email);
            var allowed = _context.AllowedEmails.FirstOrDefault(a => a.Email == email);

            // 1️⃣ Hem Users hem Allowed yok → gerçekten kayıt yok
            if (user == null && allowed == null)
            {
                ViewBag.Error = "Bu e-posta için sistemde yetki tanımlı değil.";
                return View();
            }

            // 2️⃣ Allowed var ama pasif → giriş yok
            if (allowed != null && !allowed.IsActive)
            {
                ViewBag.Error = "Bu hesap pasif durumda. Lütfen yöneticinizle görüşün.";
                return View();
            }

            if (user == null && allowed != null && allowed.IsActive)
            {
                if (string.IsNullOrWhiteSpace(confirmPassword))
                {
                    ViewBag.SignupMode = true;
                    ViewBag.Info = "Bu e-posta için ilk giriş. Lütfen şifrenizi tekrar girerek kaydı tamamlayın.";
                    return View();
                }
                if (password != confirmPassword)
                {
                    ViewBag.Error = "Şifreler birbiriyle uyuşmuyor.";
                    return View();
                }
                var clientForSignup = _httpClientFactory.CreateClient();
                var signupUrl = $"{_config["ApiSettings:BaseUrl"]}/api/Auth/signup";
                var signupPayload = JsonSerializer.Serialize(new { Email = email, Password = password });
                var signupContent = new StringContent(signupPayload, Encoding.UTF8, "application/json");

                var signupResponse = await clientForSignup.PostAsync(signupUrl, signupContent);
                if (!signupResponse.IsSuccessStatusCode)
                {
                    ViewBag.SignupMode = true;
                    ViewBag.Error = await signupResponse.Content.ReadAsStringAsync();
                    return View();
                }
            }
                var client = _httpClientFactory.CreateClient();
                //var apiUrl = $"{_config["ApiBaseUrl"]}/api/Auth/login";
                var apiUrl = $"{_config["ApiSettings:BaseUrl"]}/api/Auth/login";
                var payload = JsonSerializer.Serialize(new { Email = email, Password = password });
                var content = new StringContent(payload, Encoding.UTF8, "application/json");

            HttpResponseMessage response;
            try
            {
                response = await client.PostAsync(apiUrl, content);
            }
            catch (Exception ex)
            {
              ViewBag.Error = "Sunucuya ulaşılamadı: " + ex.Message;
              return View();
            }

            if (!response.IsSuccessStatusCode)
            {
              var msg = await response.Content.ReadAsStringAsync();
              ViewBag.Error = msg;
              return View();
            }
              var responseBody = await response.Content.ReadAsStringAsync();
              var jsonDoc = JsonDocument.Parse(responseBody);
              string? token = jsonDoc.RootElement.GetProperty("token").GetString();
             
          

                if (string.IsNullOrEmpty(token))
                {
                    ViewBag.Error = "Sunucudan geçerli bir yanıt alınamadı.";
                    return View();
                }
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);
            var claims = jwtToken.Claims.ToList();
            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);


            HttpContext.Session.SetString("AccessToken", token);
                Response.Cookies.Append("AccessToken", token, new CookieOptions
                {
                    HttpOnly = false, // ❗ Eğer JavaScript'te fetch için kullanıyorsan bu doğru
                    Secure = true,    // ✔️ HTTPS zorunlu hale gelir
                    SameSite = SameSiteMode.Lax,
                    Expires = DateTimeOffset.UtcNow.AddHours(8)
                });          

            return RedirectToAction("Index", "OrdersUIList");
        }
            [HttpGet]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            Response.Cookies.Delete("AccessToken");
            return RedirectToAction("Login", "Account");
        }
    }
}

