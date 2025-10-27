using Microsoft.AspNetCore.Mvc;
using SiparisApi.Data;
using SiparisApi.Models;

namespace SiparisApi.Controllers
{
    public class OrdersUIController : Controller
    {
        private readonly AppDbContext _context;

        public OrdersUIController(AppDbContext context)
        {
            _context = context;
        }

        // 📦 Sipariş listesi
        public IActionResult Index()
        {
            var orders = _context.Orders.ToList();
            return View(orders);
        }

        // 🆕 Yeni sipariş formunu aç
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        // 🆕 Yeni sipariş kaydet
        [HttpPost]
        public IActionResult Create(Order order)
        {
            if (ModelState.IsValid)
            {
                order.CreatedBy = User.Identity?.Name ?? "Sistem";
                order.IsApprovedByFactory = false;
                order.IsApprovedBySales = false;
                _context.Orders.Add(order);
                _context.SaveChanges();

                ViewBag.Message = "Sipariş başarıyla oluşturuldu.";
                return RedirectToAction("Index");
            }

            return View(order);
        }

        // ✏️ Sipariş düzenleme formunu aç
        [HttpGet]
        public IActionResult Edit(int id)
        {
            var order = _context.Orders.FirstOrDefault(o => o.Id == id);
            if (order == null)
                return NotFound();

            if (order.IsApprovedByFactory)
                return BadRequest("Onaylı sipariş düzenlenemez.");

            return View(order);
        }

        // ✏️ Sipariş düzenleme işlemi
        [HttpPost]
        public IActionResult Edit(Order updatedOrder)
        {
            var order = _context.Orders.FirstOrDefault(o => o.Id == updatedOrder.Id);
            if (order == null)
                return NotFound();

            if (order.IsApprovedByFactory)
                return BadRequest("Onaylı sipariş düzenlenemez.");

            order.Customer = updatedOrder.Customer;
            order.Product = updatedOrder.Product;
            order.Quantity = updatedOrder.Quantity;
            order.Unit = updatedOrder.Unit;
            order.Price = updatedOrder.Price;
            order.Currency = updatedOrder.Currency;
            order.OrderDate = updatedOrder.OrderDate;
            order.DeliveryDate = updatedOrder.DeliveryDate;
            order.PaymentTerm = updatedOrder.PaymentTerm;
            order.Transport = updatedOrder.Transport;
            order.DeliveryTerm = updatedOrder.DeliveryTerm;
            order.DueDays = updatedOrder.DueDays;

            _context.SaveChanges();
            return RedirectToAction("Index");
        }

        // ✅ Fabrika onayı ver
        [HttpPost]
        public IActionResult Approve(int id)
        {
            var order = _context.Orders.FirstOrDefault(o => o.Id == id);
            if (order == null)
                return NotFound();

            order.IsApprovedByFactory = true;
            _context.SaveChanges();

            // 📩 TODO: Buraya mail gönderme işlemi eklenecek
            return RedirectToAction("Index");
        }

        // ❌ Onayı geri al (sadece admin yetkisi gerekebilir)
        [HttpPost]
        public IActionResult RevokeApproval(int id)
        {
            var order = _context.Orders.FirstOrDefault(o => o.Id == id);
            if (order == null)
                return NotFound();

            order.IsApprovedByFactory = false;
            _context.SaveChanges();

            return RedirectToAction("Index");
        }
    }
}
