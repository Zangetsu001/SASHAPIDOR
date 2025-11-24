using System.Threading.Tasks;

namespace EduMaster.Services
{
    public interface IAuthService
    {
        Task<bool> RegisterAsync(string email, string login, string password);
        Task<bool> LoginAsync(string login, string password);
    }
}
