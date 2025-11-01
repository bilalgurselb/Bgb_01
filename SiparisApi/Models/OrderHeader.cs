﻿using System.ComponentModel.DataAnnotations.Schema;

namespace SiparisApi.Models
{
    public class OrderHeader
    {
        public int Id { get; set; }

        // Başlık alanları (hepsi HEAD level)
        public int? CustomerId { get; set; }
        public int? SalesRepId { get; set; }          // CreatedBy yerine ek açıklık
        public DateTime OrderDate { get; set; } = DateTime.UtcNow;
        public DateTime? DeliveryDate { get; set; }
        public string? PaymentTerm { get; set; }
        public string? Transport { get; set; }
        public string? DeliveryTerm { get; set; }
        public int DueDays { get; set; }
        public string? Currency { get; set; }          // Varsayılan para birimi (satır override edebilir)
      //  public int? CreatedById { get; set; }
      //  public User? CreatedById { get; set; }
        // Durum & zaman damgaları
        public string Status { get; set; } = "Onay Bekleniyor";
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; } = DateTime.UtcNow;
        public string? PortOfDelivery { get; set; }
        public string? PlaceOfDelivery { get; set; }
        public SintanCari? Customer { get; set; }
        public User? SalesRep { get; set; }      
        // Navigations
        public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
        public bool IsNew { get; set; } = false;
        public bool IsUpdated { get; set; } = false;
        public int? CreatedById { get; set; }
        public User? CreatedBy { get; set; }
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

    }
}
