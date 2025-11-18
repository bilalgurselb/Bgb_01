namespace SiparisApi.Models
{
    public class OrderListItemVm
    {
        public int Id { get; set; }
        public string ProductId { get; set; } = "";
        public string ProductName { get; set; } = "";
        public decimal Quantity { get; set; }
        public string PackingInfo { get; set; } = "";
        public decimal NetWeight { get; set; }
        public decimal Price { get; set; }

        // Toplam = Price x NetWeight  (TEK formül)
        public decimal Total => Price * NetWeight;

        public string? Description { get; set; }
    }

    public class OrderListVm
    {
        public int Id { get; set; }

        public string? CustomerId { get; set; }
        public string CustomerName { get; set; } = "";

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public string Status { get; set; } = "Onay Bekleniyor";
        public string Currency { get; set; } = "EURO";

        public string? PortOfDelivery { get; set; }
        public string? PlaceOfDelivery { get; set; }
        public string? PaymentTerm { get; set; }
        public string? Transport { get; set; }

        public string? SalesRepName { get; set; }

        public List<OrderListItemVm> Items { get; set; } = new();

        // Header toplamı = satır toplamlarının sum’ı
        public decimal TotalPrice => Items.Sum(i => i.Total);
    }
}
