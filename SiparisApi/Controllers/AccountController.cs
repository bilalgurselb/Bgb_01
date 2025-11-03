using Microsoft.AspNetCore.Mvc;
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

            try
            {
                var jsonDoc = JsonDocument.Parse(responseBody);
                string token = null;

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

            // 🔹 Başarılı giriş veya otomatik kayıt sonrası yönlendirme
            // return RedirectToAction("Create", "OrdersUI");
            return RedirectToAction("Index", "OrdersUIList");
        }

    }
}

