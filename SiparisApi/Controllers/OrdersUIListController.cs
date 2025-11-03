using Microsoft.AspNetCore.Mvc;
using SiparisApi.Models;
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

        // 🔹 SİPARİŞ LİSTESİ (API: /api/orders/list)
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var token = HttpContext.Session.GetString("AccessToken") ?? Request.Cookies["AccessToken"];
            if (token == null)
                return RedirectToAction("Login", "Account");

            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var baseUrl = _config["ApiSettings:BaseUrl"];

            // API tarafında mevcut olan endpoint
            var response = await client.GetAsync($"{baseUrl}/api/orders/list");
            if (!response.IsSuccessStatusCode)
                return View("IndexO", Enumerable.Empty<OrderHeader>());

            var json = await response.Content.ReadAsStringAsync();

            // View'in beklediği tipe uyduruyoruz (IndexO.cshtml -> IEnumerable<OrderHeader>)
            var orders = JsonSerializer.Deserialize<List<OrderHeader>>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? new List<OrderHeader>();

            return View("IndexO", orders);
        }

        // 🔹 Yeni Sipariş Formu (GET)
        [HttpGet]
        public IActionResult Create()
        {
            return View("Create");
        }

        // 🔹 Yeni Sipariş Kaydetme (POST)  (API: /api/orders/create)
        [HttpPost]
        public async Task<IActionResult> Create(OrderHeader model)
        {
            var token = HttpContext.Session.GetString("AccessToken");
            if (token == null)
                return RedirectToAction("Login", "Account");

            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var baseUrl = _config["ApiSettings:BaseUrl"];

            var json = JsonSerializer.Serialize(model);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await client.PostAsync($"{baseUrl}/api/orders/create", content);

            if (response.IsSuccessStatusCode)
            {
                TempData["Success"] = "Yeni sipariş başarıyla oluşturuldu.";
                return RedirectToAction("Index");
            }

            TempData["Error"] = "Sipariş oluşturulamadı. Lütfen bilgileri kontrol edin.";
            return View("Create", model);
        }

        // 🔹 Sipariş Düzenleme (GET)  (Not: API'de /api/orders/{id} yoksa eklenecek)
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var token = HttpContext.Session.GetString("AccessToken");
            if (token == null)
                return RedirectToAction("Login", "Account");

            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var baseUrl = _config["ApiSettings:BaseUrl"];

            // Eğer API'de GET /api/orders/{id} yoksa, lütfen ekleyelim.
            var response = await client.GetAsync($"{baseUrl}/api/orders/{id}");
            if (!response.IsSuccessStatusCode)
                return RedirectToAction("Index");

            var json = await response.Content.ReadAsStringAsync();
            var order = JsonSerializer.Deserialize<OrderHeader>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return View("Edit", order);
        }

        // 🔹 Sipariş Güncelleme (POST)  (API: /api/orders/update/{id})
        [HttpPost]
        public async Task<IActionResult> Edit(int id, OrderHeader model)
        {
            var token = HttpContext.Session.GetString("AccessToken");
            if (token == null)
                return RedirectToAction("Login", "Account");

            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var baseUrl = _config["ApiSettings:BaseUrl"];

            var json = JsonSerializer.Serialize(model);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PutAsync($"{baseUrl}/api/orders/update/{id}", content);

            if (response.IsSuccessStatusCode)
            {
                TempData["Success"] = "Sipariş başarıyla güncellendi.";
                return RedirectToAction("Index");
            }

            TempData["Error"] = "Sipariş güncellenemedi. Lütfen tekrar deneyin.";
            return View("Edit", model);
        }

        // 🔹 ONAY / ONAY KALDIR (mevcutla aynı kaldı)
        [HttpPost]
        public async Task<IActionResult> Approve(int id)
        {
            var token = HttpContext.Session.GetString("AccessToken");
            if (token == null)
                return RedirectToAction("Login", "Account");

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
            if (token == null)
                return RedirectToAction("Login", "Account");

            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var baseUrl = _config["ApiSettings:BaseUrl"];

            await client.PutAsync($"{baseUrl}/api/orders/{id}/revoke", null);
            return RedirectToAction("Index");
        }
    }

    // 🔹 UI ViewModel (gerekirse kullanırsın; IndexO şu an OrderHeader bekliyor)
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
