using Microsoft.AspNetCore.Mvc;

namespace SiparisApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrdersController : ControllerBase
    {
        private static List<Order> Orders = new();

        [HttpGet]
        public IActionResult GetOrders()
        {
            return Ok(Orders);
        }

        [HttpPost]
        public IActionResult AddOrder([FromBody] Order order)
        {
            order.Id = Guid.NewGuid();
            Orders.Add(order);
            return Ok(order);
        }
    }

  public class Order
{
    public Guid Id { get; set; }
    public required string Customer { get; set; }
    public required string Product { get; set; }
    public int Quantity { get; set; }
}

}
