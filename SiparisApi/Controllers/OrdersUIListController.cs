using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace SiparisApi.Controllers
{
    public class OrdersUIListController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _config;

        public OrdersUIListController(IHttpClientFactory httpClientFactory, IConfiguration config)
        {
            _httpClientFactory = httpClientFactory;
            _config = config;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var token = HttpContext.Session.GetString("AccessToken");
            if (token == null) return RedirectToAction("Login", "Account");

            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var baseUrl = _config["ApiSettings:BaseUrl"];

            var response = await client.GetAsync($"{baseUrl}/api/orders");
            var json = await response.Content.ReadAsStringAsync();

            var orders = JsonSerializer.Deserialize<List<OrderViewModel>>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return View(orders);
        }

        [HttpPost]
        public async Task<IActionResult> Approve(int id)
        {
            var token = HttpContext.Session.GetString("AccessToken");
            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var baseUrl = _config["ApiSettings:BaseUrl"];

            await client.PutAsync($"{baseUrl}/api/orders/{id}/approve", null);
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> RevokeApproval(int id)
        {
            var token = HttpContext.Session.GetString("AccessToken");
            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var baseUrl = _config["ApiSettings:BaseUrl"];

            await client.PutAsync($"{baseUrl}/api/orders/{id}/revoke", null);
            return RedirectToAction("Index");
        }
    }

    public class OrderViewModel
    {
        public int Id { get; set; }
        public string Customer { get; set; }
        public string Product { get; set; }
        public int Quantity { get; set; }
        public string CreatedBy { get; set; }
        public bool IsApprovedByFactory { get; set; }
        public bool IsApprovedBySales { get; set; }
    }
}

