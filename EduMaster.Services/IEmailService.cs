using System.Threading.Tasks;

namespace EduMaster.Services
{
    public interface IEmailService
    {
        // Добавили параметры replyTo (почта пользователя) и fromName (имя пользователя)
        Task SendEmailAsync(string to, string subject, string body, string? replyTo = null, string? fromName = null);
    }
}