
using EduMaster.Domain.ModelsDb;
using Microsoft.EntityFrameworkCore;

namespace EduMaster.Services
{
    public class AuthService : IAuthService
    {
        private readonly ApplicationDbContext _db;
        private readonly IPasswordHasher _hasher;

        public AuthService(ApplicationDbContext db, IPasswordHasher hasher)
        {
            _db = db;
            _hasher = hasher;
        }

        public async Task<bool> RegisterAsync(string email, string login, string password)
        {
            
            bool exist = await _db.UserDb.AnyAsync(u => u.Email == email || u.Login == login);
            if (exist) return false;

            var user = new UserDb
            {
                Id = Guid.NewGuid(),
                Email = email,
                Login = login,
                PasswordHash = _hasher.HashPassword(password),
                Role = "Student",
                CreatedAt = DateTime.UtcNow
            };

          
            _db.UserDb.Add(user);
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<bool> LoginAsync(string login, string password)
        {
           
            var user = await _db.UserDb.FirstOrDefaultAsync(u => u.Login == login || u.Email == login);
            if (user == null) return false;

            return _hasher.VerifyPassword(password, user.PasswordHash);
        }
    }
}