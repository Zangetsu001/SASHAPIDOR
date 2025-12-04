using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace EduMaster.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _config;

        public EmailService(IConfiguration config)
        {
            _config = config;
        }

        public async Task SendEmailAsync(string to, string subject, string body, string? replyTo = null, string? fromName = null)
        {
            var smtpHost = _config["SMTP:Host"];
            var smtpPort = int.Parse(_config["SMTP:Port"]);
            var smtpUser = _config["SMTP:User"];
            var smtpPass = _config["SMTP:Pass"];

            using (var client = new SmtpClient(smtpHost, smtpPort))
            {
                client.EnableSsl = true;
                client.Credentials = new NetworkCredential(smtpUser, smtpPass);
                client.DeliveryMethod = SmtpDeliveryMethod.Network;
                client.UseDefaultCredentials = false;

                // 1. Формируем "Красивого" отправителя
                // Если имя пользователя передано, письмо придет от: "Иван Иванов (Сервис)" <admin@gmail.com>
                var senderName = string.IsNullOrEmpty(fromName) ? "EduMaster" : $"{fromName} (EduMaster)";
                var fromAddress = new MailAddress(smtpUser, senderName);

                using (var message = new MailMessage(fromAddress, new MailAddress(to)))
                {
                    message.Subject = subject;
                    message.Body = body;
                    message.IsBodyHtml = true;

                    // 2. Самое главное: настраиваем Reply-To
                    // Когда админ нажмет "Ответить", письмо уйдет пользователю
                    if (!string.IsNullOrEmpty(replyTo))
                    {
                        message.ReplyToList.Add(new MailAddress(replyTo));
                    }

                    await client.SendMailAsync(message);
                }
            }
        }
    }
}