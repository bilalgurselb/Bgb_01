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
        public async Task<IActionResult> Login(string email, string password)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                ViewBag.Error = "E-posta ve şifre zorunludur.";
                return View();
            }

            var client = _httpClientFactory.CreateClient();

            // 🔹 Azure App Service URL (sabit, güvenli domain)
            var apiUrl = "https://bilalgurseliparis-eyehgshwhfg4a7ew.northeurope-01.azurewebsites.net/api/Auth/login";

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
                ViewBag.Error = "E-posta veya şifre hatalı.";
                return View();
            }

            var responseBody = await response.Content.ReadAsStringAsync();

            try
            {
                var jsonDoc = JsonDocument.Parse(responseBody);
                string token = null;

                // 🔹 Token property'si farklı isimlerde olabilir ("access_token" veya "token")
                if (jsonDoc.RootElement.TryGetProperty("token", out var tokenProp))
                    token = tokenProp.GetString();
                else if (jsonDoc.RootElement.TryGetProperty("access_token", out var accessTokenProp))
                    token = accessTokenProp.GetString();

                if (string.IsNullOrEmpty(token))
                {
                    ViewBag.Error = "Sunucudan geçerli bir yanıt alınamadı.";
                    return View();
                }

                // 🔹 JWT token'ı Session'a kaydet
                HttpContext.Session.SetString("AccessToken", token);
            }
            catch
            {
                ViewBag.Error = "Yanıt çözümleme hatası oluştu.";
                return View();
            }

            // 🔹 Başarılı giriş → Sipariş oluşturma ekranına yönlendirme
            return RedirectToAction("Create", "OrdersUI");
        }
    }
}
