namespace SiparisApi.Models
{
    public class AllowedEmail
    {
        public int Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
    }
}
