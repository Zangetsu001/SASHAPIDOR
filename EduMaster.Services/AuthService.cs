using EduMaster.DAL;
using EduMaster.Domain.ModelsDb;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace EduMaster.Services
{
    public class AuthService : IAuthService
    {
        private readonly ApplicationDbContext _db;
        private readonly IPasswordHasher _hasher;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AuthService(ApplicationDbContext db, IPasswordHasher hasher, IHttpContextAccessor httpContextAccessor)
        {
            _db = db;
            _hasher = hasher;
            _httpContextAccessor = httpContextAccessor;
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

            await SignInUserAsync(user.Login, user.Email, user.Role);
            return true;
        }

        public async Task<bool> LoginAsync(string login, string password)
        {
            var user = await _db.UserDb.FirstOrDefaultAsync(u => u.Login == login || u.Email == login);
            if (user == null) return false;

            if (!_hasher.VerifyPassword(password, user.PasswordHash)) return false;

            await SignInUserAsync(user.Login, user.Email, user.Role);
            return true;
        }

        private async Task SignInUserAsync(string login, string email, string role)
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext == null) return;

            // Получаем ID пользователя для сохранения в куки
            var userId = (await _db.UserDb.AsNoTracking().FirstOrDefaultAsync(u => u.Login == login))?.Id.ToString() ?? "";

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, login ?? string.Empty),
                new Claim(ClaimTypes.Email, email ?? string.Empty),
                new Claim(ClaimTypes.Role, role ?? string.Empty),
                // ВАЖНО: Добавляем ID пользователя
                new Claim(ClaimTypes.NameIdentifier, userId)
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await httpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);
        }
    }
}