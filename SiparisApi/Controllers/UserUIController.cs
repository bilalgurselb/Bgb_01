using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace SiparisApi.Controllers
{
    public class UsersUIController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _config;

        public UsersUIController(IHttpClientFactory httpClientFactory, IConfiguration config)
        {
            _httpClientFactory = httpClientFactory;
            _config = config;
        }

        [HttpGet]
        public async Task<IActionResult> Manage()
        {
            var token = HttpContext.Session.GetString("AccessToken");
            if (token == null) return RedirectToAction("Login", "Account");

            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var baseUrl = _config["ApiSettings:BaseUrl"];

            var response = await client.GetAsync($"{baseUrl}/api/users");
            var json = await response.Content.ReadAsStringAsync();
            var users = JsonSerializer.Deserialize<List<UserViewModel>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            return View(users);
        }

        [HttpPost]
        public async Task<IActionResult> ToggleActive(int id)
        {
            var token = HttpContext.Session.GetString("AccessToken");
            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var baseUrl = _config["ApiSettings:BaseUrl"];

            await client.PutAsync($"{baseUrl}/api/users/{id}/toggle", null);
            return RedirectToAction("Manage");
        }

        [HttpPost]
        public async Task<IActionResult> ResetPassword(int id)
        {
            var token = HttpContext.Session.GetString("AccessToken");
            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var baseUrl = _config["ApiSettings:BaseUrl"];

            await client.PostAsync($"{baseUrl}/api/users/{id}/resetpassword", null);
            return RedirectToAction("Manage");
        }
    }

    public class UserViewModel
    {
        public int Id { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }
        public bool IsActive { get; set; }
    }
}
