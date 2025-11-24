using System.Threading.Tasks;

namespace EduMaster.Services
{
    public interface IEmailService
    {
        Task SendEmailAsync(string to, string subject, string body);
    }
}
