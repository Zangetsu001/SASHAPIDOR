using EduMaster.DAL;
using EduMaster.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;
using System;

namespace EduMaster.Controllers
{
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IPasswordHasher _hasher;

        public ProfileController(ApplicationDbContext db, IPasswordHasher hasher)
        {
            _db = db;
            _hasher = hasher;
        }

        // Страница профиля (переименована в UserProfile)
        public async Task<IActionResult> UserProfile()
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdStr == null) return RedirectToAction("Index", "Home");

            var user = await _db.UserDb.FindAsync(Guid.Parse(userIdStr));
            return View(user);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(string email, string login, string password)
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var user = await _db.UserDb.FindAsync(Guid.Parse(userIdStr));

            if (user != null)
            {
                if (!string.IsNullOrWhiteSpace(email)) user.Email = email;
                if (!string.IsNullOrWhiteSpace(login)) user.Login = login;
                if (!string.IsNullOrWhiteSpace(password)) user.PasswordHash = _hasher.HashPassword(password);

                await _db.SaveChangesAsync();
            }
            return RedirectToAction("UserProfile");
        }
    }
}