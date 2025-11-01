namespace SiparisApi.Models
{
    public class OrderItem
    {
        public int Id { get; set; }

        // FK
        public int OrderHeaderId { get; set; }
        public OrderHeader? OrderHeader { get; set; }

        // Satır (DETAIL) alanları
        
      //  public string? Unit { get; set; }           // pcs, pallets, vs.
        public decimal Quantity { get; set; }       // 10–15 satır ihtimali
        public decimal Price { get; set; }        // Birim fiyat
        public SintanStok? Product { get; set; }                                             // Satır bazında farklı para birimi opsiyonel
        public string? Description { get; set; }    // Satır notu
        public int ProductId { get; set; }
        public bool? IsApprovedByFactory { get; set; }
        public bool? IsApprovedBySales { get; set; }
        public int? RowNumber { get; set; }
        public string? PackingInfo { get; set; }
        public decimal? NetWeight { get; set; }


        // Görünürlükte CreatedBy yerine Header.CreatedBy kullanılacak
    }
}
