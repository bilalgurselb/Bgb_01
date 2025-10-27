using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using System.Text.Json;

namespace SiparisApi.Controllers
{
    public class AdminUIController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _config;

        public AdminUIController(IHttpClientFactory httpClientFactory, IConfiguration config)
        {
            _httpClientFactory = httpClientFactory;
            _config = config;
        }

        [HttpGet]
        public IActionResult Dashboard()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Logs()
        {
            var token = HttpContext.Session.GetString("AccessToken");
            if (token == null) return RedirectToAction("Login", "Account");

            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var baseUrl = _config["ApiSettings:BaseUrl"];

            var response = await client.GetAsync($"{baseUrl}/api/logs");
            var logsJson = await response.Content.ReadAsStringAsync();

            var logs = JsonSerializer.Deserialize<List<LogViewModel>>(logsJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            return View(logs);
        }
    }

    public class LogViewModel
    {
        public string UserEmail { get; set; }
        public string Action { get; set; }
        public string Endpoint { get; set; }
        public DateTime Timestamp { get; set; }
        public string Details { get; set; }
    }
}

