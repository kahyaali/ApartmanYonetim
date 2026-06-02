using MailKit.Net.Smtp;
using MimeKit;

namespace ApartmanYonetim.Services
{

    public interface IEmailService
    {
        Task SendAsync(string toEmail, string subject, string htmlBody);
    }
    public class EmailService : IEmailService
    {

        private readonly IConfiguration _config;

        public EmailService(IConfiguration config)
        {
            _config = config;
        }
        public async Task SendAsync(string toEmail, string subject, string htmlBody)
        {
            var settings = _config.GetSection("EmailSettings");

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(
                         settings["FromName"] ?? "Apartman Yönetimi",
                         settings["Username"] ?? ""));
            message.To.Add(MailboxAddress.Parse(toEmail));
            message.Subject = subject;

            var builder = new BodyBuilder { HtmlBody = htmlBody };
            message.Body = builder.ToMessageBody();

            using var client = new SmtpClient();
            await client.ConnectAsync(settings["Host"], int.Parse(settings["Port"]!), MailKit.Security.SecureSocketOptions.StartTls);
            await client.AuthenticateAsync(settings["Username"], settings["Password"]);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);
        }
    }
}
