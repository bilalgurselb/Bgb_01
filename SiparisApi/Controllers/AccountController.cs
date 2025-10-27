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
            var client = _httpClientFactory.CreateClient();
            var json = JsonSerializer.Serialize(new { Email = email, Password = password });
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PostAsync("https://localhost:5001/api/auth/login", content);

            if (!response.IsSuccessStatusCode)
            {
                ViewBag.Error = "E-posta veya şifre hatalı.";
                return View();
            }

            var responseBody = await response.Content.ReadAsStringAsync();
            var token = JsonDocument.Parse(responseBody).RootElement.GetProperty("access_token").GetString();

            // Token'ı session'da sakla
            HttpContext.Session.SetString("AccessToken", token);

            return RedirectToAction("Create", "Orders");
        }
    }
}

