using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
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
            var apiUrl = $"{_config["ApiBaseUrl"]}/api/Auth/login";

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
            string? name = jsonDoc.RootElement.TryGetProperty("nameSurname", out var n) ? n.GetString() : email;
            string? role = jsonDoc.RootElement.TryGetProperty("role", out var r) ? r.GetString() : "Kullanıcı";

            try
            {
                if (jsonDoc.RootElement.TryGetProperty("token", out var tokenProp))
                    token = tokenProp.GetString();
                else if (jsonDoc.RootElement.TryGetProperty("access_token", out var accessTokenProp))
                    token = accessTokenProp.GetString();

                if (string.IsNullOrEmpty(token))
                {
                    ViewBag.Error = "Sunucudan geçerli bir yanıt alınamadı.";
                    return View();
                }

                HttpContext.Session.SetString("AccessToken", token);
            }
            catch
            {
                ViewBag.Error = "Yanıt çözümleme hatası oluştu.";
                return View();
            }
            var handler = new JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(token);

            string nameSurname =
                jwt.Claims.FirstOrDefault(c => c.Type == "NameSurname")?.Value
                ?? jwt.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value
                ?? email;

            string _role =
                jwt.Claims.FirstOrDefault(c => c.Type == "Role")?.Value
                ?? jwt.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value
                ?? "User";
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, email),
                new Claim("NameSurname", nameSurname),
                new Claim("Role", _role),
                new Claim(ClaimTypes.Role,_role)
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            // 🔹 Kullanıcıyı giriş yapmış olarak işaretle
            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                new AuthenticationProperties
                {
                    IsPersistent = true,
                    ExpiresUtc = DateTime.UtcNow.AddHours(8)
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

