namespace SiparisApi.Models
{
    public class OrderItem
    {
        public int Id { get; set; }
        public int OrderHeaderId { get; set; }
        public OrderHeader? OrderHeader { get; set; }
              
        public decimal Quantity { get; set; }
        public decimal Price { get; set; }                                                  
        public string? Description { get; set; }
        public string? ProductId { get; set; }

        public bool? IsApprovedByFactory { get; set; }
        public bool? IsApprovedBySales { get; set; }
        public int RowNumber { get; set; }
        public string? PackingInfo { get; set; }
        public decimal NetWeight { get; set; }
        public SintanStok? SintanStok { get; set; }
    }
}
