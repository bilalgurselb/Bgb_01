namespace SiparisApi.Models
{
    public class Log
    {
        public int Id { get; set; }
        public string UserEmail { get; set; }
        public string Action { get; set; }
        public string Endpoint { get; set; }
        public DateTime Timestamp { get; set; }
        public string Details { get; set; }
    }
}
