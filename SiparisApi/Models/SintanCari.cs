using System.ComponentModel.DataAnnotations;
using System.Numerics;

namespace SiparisApi.Models
{
    public class SintanCari
    {
      //  public int Id { get; set; }

        [MaxLength(200)]
        public string? CARI_ISIM { get; set; }

        [MaxLength(15)]
        public string? CARI_KOD { get; set; }

        [MaxLength(100)]
        public string? GRUP_KODU { get; set; }

        [MaxLength(200)]
        public string? GRUP_ISMI { get; set; }

        [MaxLength(50)]
        public short? KOD_1 { get; set; }

        [MaxLength(50)]
        public short? KOD_2 { get; set; }

        [MaxLength(250)]
        public string? ADRES { get; set; }

        [MaxLength(50)]
        public string? IL { get; set; }

        [MaxLength(50)]
        public string? ILCE { get; set; }

        [MaxLength(20)]
        public string? TELEFON { get; set; }

        [MaxLength(20)]
        public string? FAKS { get; set; }

        [MaxLength(100)]
        public string? VERGI_DAIRESI { get; set; }

        [MaxLength(50)]
        public string? VERGI_NUMARASI { get; set; }

        [MaxLength(20)]
        public int? POSTAKODU { get; set; }

        public short? VADE_GUNU { get; set; }

        [MaxLength(50)]
        public string? PLASIYER_KODU { get; set; }

        [MaxLength(4)]
        public string? ULKE_KODU { get; set; }

        [MaxLength(200)]
        public string? EMAIL { get; set; }

        [MaxLength(100)]
        public string? WEB { get; set; }

        [MaxLength(15)]
        public string? TC_NO { get; set; }
    }
}
