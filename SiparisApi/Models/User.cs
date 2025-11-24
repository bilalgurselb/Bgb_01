using System.ComponentModel.DataAnnotations.Schema;

namespace SiparisApi.Models
{
    public class User
    {
        // 🔹 Veritabanında yer alan alanlar
        public int Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public int? AllowedId { get; set; }  // FK kolon
        public int? AllowedEmailId { get; set; }
        public AllowedEmail? AllowedEmail { get; set; }  // Navigation FK
       
        // 🔹 Veritabanında olmayan ama UI ve log için gerekli alanlar
        [NotMapped]
        public string? NameSurname { get; set; }  // Hoşgeldiniz Bilal Bey vs.

        [NotMapped]
        public string? Role { get; set; }  // Admin / User / Factory

        [NotMapped]
        public bool IsActive { get; set; } = true;  // AllowedEmail üzerinden okunacak

        // 🔹 Kullanıcı eylemleri için (loglarda kim oluşturdu, kim güncelledi)
        [NotMapped]
        public string DisplayName =>
            !string.IsNullOrEmpty(NameSurname) ? NameSurname : Email;
    }

}

