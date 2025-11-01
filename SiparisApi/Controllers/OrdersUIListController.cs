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

        // 🔹 SİPARİŞ LİSTESİ
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var token = HttpContext.Session.GetString("AccessToken");
            if (token == null)
                return RedirectToAction("Login", "Account");

            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var baseUrl = _config["ApiSettings:BaseUrl"];

            var response = await client.GetAsync($"{baseUrl}/api/orders");
            var json = await response.Content.ReadAsStringAsync();

            var orders = JsonSerializer.Deserialize<List<OrderViewModel>>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return View("IndexO", orders);
        }

        // 🔹 ONAY / ONAY KALDIR
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

        // 🔹 YENİ SİPARİŞ SAYFASI
        [HttpGet]
        public IActionResult Create()
        {
            return View("Create");
        }

        // 🔹 YENİ SİPARİŞ POST (API'ye JSON gönder)
        [HttpPost]
        public async Task<IActionResult> Create(OrderViewModel model)
        {
            var token = HttpContext.Session.GetString("AccessToken");
            if (token == null)
                return RedirectToAction("Login", "Account");

            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var baseUrl = _config["ApiSettings:BaseUrl"];

            var json = JsonSerializer.Serialize(model);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await client.PostAsync($"{baseUrl}/api/orders", content);

            if (response.IsSuccessStatusCode)
                TempData["Success"] = "Yeni sipariş başarıyla oluşturuldu.";
            else
                TempData["Error"] = "Sipariş oluşturulamadı.";

            return RedirectToAction("Index");
        }

        // 🔹 SİPARİŞ DÜZENLEME SAYFASI
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var token = HttpContext.Session.GetString("AccessToken");
            if (token == null)
                return RedirectToAction("Login", "Account");

            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var baseUrl = _config["ApiSettings:BaseUrl"];

            var response = await client.GetAsync($"{baseUrl}/api/orders/{id}");
            if (!response.IsSuccessStatusCode)
                return RedirectToAction("Index");

            var json = await response.Content.ReadAsStringAsync();
            var order = JsonSerializer.Deserialize<OrderViewModel>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return View("Edit", order);
        }

        // 🔹 SİPARİŞ DÜZENLEME (API'ye PUT)
        [HttpPost]
        public async Task<IActionResult> Edit(OrderViewModel model)
        {
            var token = HttpContext.Session.GetString("AccessToken");
            if (token == null)
                return RedirectToAction("Login", "Account");

            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var baseUrl = _config["ApiSettings:BaseUrl"];

            var json = JsonSerializer.Serialize(model);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await client.PutAsync($"{baseUrl}/api/orders/{model.Id}", content);

            if (response.IsSuccessStatusCode)
                TempData["Success"] = "Sipariş başarıyla güncellendi.";
            else
                TempData["Error"] = "Güncelleme sırasında hata oluştu.";

            return RedirectToAction("Index");
        }
    }

    // 🔹 ViewModel — UI tarafı sadece gösterim ve API transferi için
    public class OrderViewModel
    {
        public int Id { get; set; }
        public string? Customer { get; set; }
        public string? Product { get; set; }
        public decimal Quantity { get; set; }
        public decimal? Price { get; set; }
        public string? CreatedBy { get; set; }
        public int Status { get; set; }
        public bool IsApprovedByFactory { get; set; }
        public bool IsApprovedBySales { get; set; }
    }
}
