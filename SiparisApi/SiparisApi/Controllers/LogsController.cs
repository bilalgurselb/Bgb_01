using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using global::SiparisApi.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SiparisApi.Data;
using SiparisApi.Models;

namespace SiparisApi.Controllers
{
   

    namespace SiparisApi.Controllers
    {
        [Authorize(Roles = "Admin")] // 📌 sadece admin görebilir
        [ApiController]
        [Route("api/[controller]")]
        public class LogsController : ControllerBase
        {
            private readonly AppDbContext _context;

            public LogsController(AppDbContext context)
            {
                _context = context;
            }

            // 📜 Tüm logları getir
            [HttpGet]
            public IActionResult GetLogs()
            {
                var logs = _context.Logs
                    .OrderByDescending(l => l.Timestamp)
                    .ToList();
                return Ok(logs);
            }

            // 📦 Belirli kullanıcıya ait loglar
            [HttpGet("user/{email}")]
            public IActionResult GetLogsByUser(string email)
            {
                var logs = _context.Logs
                    .Where(l => l.UserEmail == email)
                    .OrderByDescending(l => l.Timestamp)
                    .ToList();

                return Ok(logs);
            }
        }
    }

}
