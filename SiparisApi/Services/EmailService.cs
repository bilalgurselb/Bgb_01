using System.Net;
using System.Net.Mail;

namespace SiparisApi.Services
{
    public interface IEmailService
    {
        Task SendEmailAsync(string to, string subject, string body, bool isHtml = false);
        Task SendOrderApprovedEmailAsync(string toEmail, int orderId);
        Task SendOrderCreatedEmailAsync(string toEmail, string productName, int quantity, string createdBy, int orderId);
    }

    public class EmailService : IEmailService
    {
        private readonly IConfiguration _config;

        public EmailService(IConfiguration config)
        {
            _config = config;
        }

        public async Task SendEmailAsync(string to, string subject, string body, bool isHtml = false)
        {
            var host = _config["Smtp:Host"];
            var port = int.Parse(_config["Smtp:Port"] ?? "587");
            var user = _config["Smtp:Username"];
            var pass = _config["Smtp:Password"];

            using var client = new SmtpClient(host, port)
            {
                EnableSsl = true,
                Credentials = new NetworkCredential(user, pass)
            };

            var msg = new MailMessage(user, to, subject, body) { IsBodyHtml = isHtml };
            await client.SendMailAsync(msg);
        }

        // Var olan iş mantığını korur: "Sipariş Onaylandı" bildirimi
        public Task SendOrderApprovedEmailAsync(string toEmail, int orderId)
        {
            var subject = "Sipariş Onaylandı";
            var body = $"Siparişiniz (ID: {orderId}) üretim tarafından onaylandı.";
            return SendEmailAsync(toEmail, subject, body);
        }

        // “Kayıt yapıldı” bildirimi (demo/patron sunumu için)
        public Task SendOrderCreatedEmailAsync(string toEmail, string productName, int quantity, string createdBy, int orderId)
        {
            var subject = "Yeni Sipariş Kaydı";
            var body =
$@"Yeni sipariş oluşturuldu.

Ürün: {productName}
Miktar: {quantity}
Oluşturan: {createdBy}
Sipariş ID: {orderId}";
            return SendEmailAsync(toEmail, subject, body);
        }
    }
}
