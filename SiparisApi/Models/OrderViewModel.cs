using SiparisApi.Models;

namespace SiparisApi.Models
{
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
