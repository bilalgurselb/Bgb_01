using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Json;

namespace SiparisApi.Controllers
{
    public class AccountController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public AccountController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
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

            // ✅ Dinamik API adresi (local ya da Azure fark etmeksizin)
            var apiUrl = $"{Request.Scheme}://{Request.Host}/api/Auth/login";

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
                // ✅ Doğru property adı: "token"
                var token = jsonDoc.RootElement.GetProperty("token").GetString();

                if (string.IsNullOrEmpty(token))
                {
                    ViewBag.Error = "Sunucudan geçerli bir yanıt alınamadı.";
                    return View();
                }

                // ✅ Session'a kaydet
                HttpContext.Session.SetString("AccessToken", token);
            }
            catch
            {
                ViewBag.Error = "Yanıt çözümleme hatası oluştu.";
                return View();
            }

            // ✅ Başarılı giriş → Sipariş oluşturma ekranına
            return RedirectToAction("Create", "OrdersUI");
        }
    }
}
