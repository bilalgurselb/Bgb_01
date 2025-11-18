using System.ComponentModel.DataAnnotations.Schema;
using System.Net;

namespace SiparisApi.Models
{
    public class OrderHeader
    {
        public int Id { get; set; }

        public string? CustomerId { get; set; }
        
        public int SalesRepId { get; set; }
        public User? SalesRep { get; set; }
        public DateTime OrderDate { get; set; } 
        public DateTime? DeliveryDate { get; set;}
        public string? PaymentTerm { get; set; }
        public string? Transport { get; set; }
        public string? DeliveryTerm { get; set; }
        public int? DueDays { get; set; }
        public string? Currency { get; set; }          
        public string Status { get; set; } = "Onay Bekleniyor";
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; } = DateTime.UtcNow;
        public string? PortOfDelivery { get; set; }
        public string? PlaceOfDelivery { get; set; }        
        public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
        public bool IsNew { get; set; } = true;
        public bool IsUpdated { get; set; } = false;
        public int? CreatedById { get; set; } 
        public User? CreatedBy { get; set; }
        [NotMapped]
        public string? CustomerName { get; set; }
        [NotMapped]
        public string StatusColor =>
           (Status ?? "").Trim() switch
           {
               "Onay Bekleniyor" => "#e6a700", // 🟡
               "Onaylandı / Üretimde" => "#009879", // 🟢
               "Üretimde" => "#009879", // yönetim tarafı bu metni set ediyorsa
               "Üretildi" => "#007BFF", // 🔵
               "Tamamlandı / Teslim Edildi" => "#6f42c1", // 🟣
               "İptal" => "#dc3545", // 🔴
               "Kısmi İptal" => "#dc3545",
               _ => "#6c757d"  // gri - bilinmiyor/boş
           };
        [NotMapped]
        public string SalesRepName => SalesRep?.AllowedEmail?.NameSurname ?? "-";

    }
}
