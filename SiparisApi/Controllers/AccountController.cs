using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
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

        public AccountController(IHttpClientFactory httpClientFactory, IConfiguration config)
        {
            _httpClientFactory = httpClientFactory;
            _config = config;
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
            // 🔹 Şifre tekrar kontrolü
            if (!string.IsNullOrEmpty(confirmPassword))
            {
                if (password != confirmPassword)
                {
                    ViewBag.Error = "Şifreler birbiriyle uyuşmuyor.";
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

