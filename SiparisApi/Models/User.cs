namespace SiparisApi.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsActive { get; set; } = true;      // devre dışı bırak/aktif et
        public string Role { get; set; } = "User";      // Admin, Factory, User
    }
}
