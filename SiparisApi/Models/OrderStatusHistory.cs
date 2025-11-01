namespace SiparisApi.Models
{
    public class OrderStatusHistory
    {
        public int Id { get; set; }
        public int OrderHeaderId { get; set; }
        public string Status { get; set; } = string.Empty;        
        public DateTime ChangedAt { get; set; } = DateTime.UtcNow;
        public int? ChangedById { get; set; }
        public User? ChangedBy { get; set; }
        public OrderHeader? OrderHeader { get; set; }
    }
}
