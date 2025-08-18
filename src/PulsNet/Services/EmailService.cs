using System.Net.Mail;
using System.Net;

namespace PulsNet.Services
{
    public sealed class EmailService
    {
        private readonly ConfigService _config;

        public EmailService(ConfigService config)
        {
            _config = config;
        }

        public async Task SendAsync(string to, string subject, string body, CancellationToken ct)
        {
            var s = _config.Config.Smtp;
            using var client = new SmtpClient(s.Host, s.Port)
            {
                EnableSsl = s.UseStartTls,
                Credentials = new NetworkCredential(s.Username, s.Password)
            };
            var message = new MailMessage(s.From, to, subject, body)
            {
                IsBodyHtml = true
            };
            await client.SendMailAsync(message, ct);
        }
    }
}

