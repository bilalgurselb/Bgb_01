using System.ComponentModel.DataAnnotations;

namespace SiparisApi.Models
{

    public class SintanStok
    {
      //  public int Id { get; set; }

        [MaxLength(200)]
        public string? STOK_ADI { get; set; }

        [MaxLength(100)]
        public string? STOK_KODU { get; set; }

        [MaxLength(50)]
        public string? KOD_1 { get; set; }

        public decimal? PAY1 { get; set; }

        public decimal? AMBALAJ_AGIRLIGI { get; set; }

        [MaxLength(20)]
        public string? OLCU_BR1 { get; set; }

        public decimal? PALET_AMBALAJ_ADEDI { get; set; }

        [MaxLength(20)]
        public string? OLCU_BR2 { get; set; }

        public decimal? PALET_NET_AGIRLIGI { get; set; }

        [MaxLength(10)]
        public string? A { get; set; }

        [MaxLength(20)]
        public string? OLCU_BR3 { get; set; }

        public decimal? PAY2 { get; set; }

        public decimal? CEVRIM_DEGERI_1 { get; set; }

        public decimal? ASGARI_STOK { get; set; }

        public decimal? BIRIM_AGIRLIK { get; set; }

        public decimal? NAKLIYET_TUT { get; set; }

        [MaxLength(200)]
        public string? OZEL_SAHA { get; set; }
    }
}
