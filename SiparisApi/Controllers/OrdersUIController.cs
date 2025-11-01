
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SiparisApi.Data;
using SiparisApi.Models;

namespace SiparisApi.Controllers
{
    public class OrdersUIController : Controller
    {
        private readonly AppDbContext _context;
        private readonly ILogger<OrdersUIController> _logger;

        public OrdersUIController(AppDbContext context, ILogger<OrdersUIController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // 🟢 1. Listeleme
        public async Task<IActionResult> Index()
        {
            var userEmail = HttpContext.Session.GetString("UserEmail");
            var userRole = HttpContext.Session.GetString("UserRole");

            var orders = await _context.Orders
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();

            ViewBag.UserEmail = userEmail;
            ViewBag.UserRole = userRole;

            return View(orders);
        }

        // 🟢 2. Yeni Sipariş
        [HttpGet]
        public IActionResult Create() => View();

        [HttpPost]
        public async Task<IActionResult> Create(Order order)
        {
            if (!ModelState.IsValid) return View(order);

            order.CreatedAt = DateTime.UtcNow;
            order.OrderDate = DateTime.UtcNow;
            order.CreatedBy = HttpContext.Session.GetString("UserEmail");
            order.Status = "Onay Bekleniyor";

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            _context.OrderStatusHistories.Add(new OrderStatusHistory
            {
                OrderHeaderId = order.Id,
                Status = order.Status,
                ChangedBy = order.CreatedBy ?? "system"
            });
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        // 🟢 3. Düzenleme
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order == null) return NotFound();

            var userEmail = HttpContext.Session.GetString("UserEmail");
            var userRole = HttpContext.Session.GetString("UserRole");

            if (order.Status != "Onay Bekleniyor" && userRole != "Admin")
            {
                TempData["Error"] = "Bu sipariş onaylı, düzenlenemez.";
                return RedirectToAction("Index");
            }

            if (order.CreatedBy != userEmail && userRole != "Admin")
            {
                TempData["Error"] = "Sadece kendi siparişinizi düzenleyebilirsiniz.";
                return RedirectToAction("Index");
            }

            return View(order);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(Order order)
        {
            if (!ModelState.IsValid) return View(order);

            var existingOrder = await _context.Orders.AsNoTracking().FirstOrDefaultAsync(o => o.Id == order.Id);
            if (existingOrder == null) return NotFound();

            existingOrder.Product = order.Product;
            existingOrder.Customer = order.Customer;
            existingOrder.Quantity = order.Quantity;
            existingOrder.Unit = order.Unit;
            existingOrder.Price = order.Price;
            existingOrder.Currency = order.Currency;
            existingOrder.PaymentTerm = order.PaymentTerm;
            existingOrder.Transport = order.Transport;
            existingOrder.DeliveryTerm = order.DeliveryTerm;
            existingOrder.DueDays = order.DueDays;
            existingOrder.IsChanged = true;
            existingOrder.Status = "Onay Bekleniyor";

            await _context.SaveChangesAsync();

            _context.OrderStatusHistories.Add(new OrderStatusHistory
            {
                OrderHeaderId = existingOrder.Id,
                Status = "Düzenlendi",
                ChangedBy = existingOrder.CreatedBy ?? "system"
            });
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        // 🟢 4. Durum Güncelleme (Admin)
        [HttpPost]
        public async Task<IActionResult> ChangeStatus(int id, string status)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order == null) return NotFound();

            order.Status = status;
            order.IsApprovedByFactory = status == "Üretimde" || status == "Onaylandı";
            await _context.SaveChangesAsync();

            _context.OrderStatusHistories.Add(new OrderStatusHistory
            {
                OrderHeaderId = id,
                Status = status,
                ChangedBy = HttpContext.Session.GetString("UserEmail") ?? "system"
            });
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        // 🟢 5. Admin onayı kaldırma
        [HttpPost]
        public async Task<IActionResult> RevokeApproval(int id)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order == null) return NotFound();

            order.Status = "Onay Bekleniyor";
            order.IsApprovedByFactory = false;
            order.IsApprovedBySales = false;
            await _context.SaveChangesAsync();

            _context.OrderStatusHistories.Add(new OrderStatusHistory
            {
                OrderHeaderId = id,
                Status = "Onay Kaldırıldı",
                ChangedBy = HttpContext.Session.GetString("UserEmail") ?? "admin"
            });
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }
    }
}
