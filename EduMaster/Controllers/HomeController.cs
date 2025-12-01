using Microsoft.AspNetCore.Mvc;
using EduMaster.Services;
using Microsoft.AspNetCore.Authorization;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
using System;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using EduMaster.Domain.ModelsDb;
using EduMaster.DAL;
using Microsoft.EntityFrameworkCore;
using Google.Apis.Auth;
using System.Security.Claims;

namespace EduMaster.Controllers
{
    public class HomeController : Controller
    {
        private readonly IAuthService _authService;
        private readonly ICourseService _courseService;
        private readonly ApplicationDbContext _context; // Нам нужен контекст, чтобы искать пользователя

        public HomeController(IAuthService authService, ICourseService courseService, ApplicationDbContext context)
        {
            _authService = authService;
            _courseService = courseService;
            _context = context;
        }

        // ================= ГЛАВНАЯ =================

        public async Task<IActionResult> Index()
        {
            var courses = await _courseService.GetActiveCoursesAsync();
            ViewBag.AllCourses = courses;
            return View();
        }

        // ================= DASHBOARD =================

        [Authorize]
        [HttpGet]
        public IActionResult Dashboard()
        {
            // 1. Если это не AJAX запрос, лучше перенаправить на Главную (Index), 
            // так как метода _DashboardPartial не существует.
            //if (Request.Headers["X-Requested-With"] != "XMLHttpRequest")
            //{
            //    return RedirectToAction("Index");
            //}

            var userName = User.Identity?.Name ?? "Гость";

            // Логика получения курсов (без изменений)
            var stored = HttpContext.Session.GetString("myCourses");

            List<Guid> ids = string.IsNullOrEmpty(stored)
                ? new List<Guid>()
                : System.Text.Json.JsonSerializer.Deserialize<List<Guid>>(stored);

            ViewBag.MyCourses = _context.CourseDb
                .Where(c => ids.Contains(c.Id))
                .ToList();

            ViewBag.AllCourses = _context.CourseDb
                .Where(c => !ids.Contains(c.Id))
                .ToList();

            // 2. ВАЖНО: Возвращаем "Dashboard", так как твой файл называется Dashboard.cshtml
            return PartialView("_DashboardPartial", userName);
        }


        // ================= ЗАПИСАТЬСЯ =================

        [Authorize]
        [HttpPost]
        public IActionResult AddCourse(Guid id)
        {
            var stored = HttpContext.Session.GetString("myCourses");

            List<Guid> ids = string.IsNullOrEmpty(stored)
                ? new List<Guid>()
                : System.Text.Json.JsonSerializer.Deserialize<List<Guid>>(stored);

            if (!ids.Contains(id))
                ids.Add(id);

            HttpContext.Session.SetString("myCourses",
                System.Text.Json.JsonSerializer.Serialize(ids));

            return Json(new { success = true });
        }




        // ================= ОТПИСАТЬСЯ =================
        [Authorize]
        [HttpPost]
        public IActionResult RemoveCourse(Guid id)
        {
            var stored = HttpContext.Session.GetString("myCourses");

            List<Guid> ids = string.IsNullOrEmpty(stored)
                ? new List<Guid>()
                : System.Text.Json.JsonSerializer.Deserialize<List<Guid>>(stored);

            if (ids.Contains(id))
                ids.Remove(id);

            HttpContext.Session.SetString("myCourses",
                System.Text.Json.JsonSerializer.Serialize(ids));

            return Json(new { success = true });
        }

        // ================= ВЫХОД =================

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }

        // ================= ВХОД ЧЕРЕЗ GOOGLE (ИСПРАВЛЕННЫЙ) =================

        [HttpPost]
        public async Task<IActionResult> GoogleLogin(string credential)
        {
            try
            {
                // 1. Настройки валидации (ВАЖНО: ВСТАВЬТЕ СВОЙ CLIENT ID!)
                var settings = new GoogleJsonWebSignature.ValidationSettings()
                {
                    Audience = new List<string>() { "658972345156-chmbutbtvpqmeflh5jq3hmge1omvqtlt.apps.googleusercontent.com" }
                };

                var payload = await GoogleJsonWebSignature.ValidateAsync(credential, settings);

                // 2. Ищем пользователя в базе по Email
                var user = await _context.UserDb.FirstOrDefaultAsync(u => u.Email == payload.Email);

                if (user == null)
                {
                    // 3. Если пользователя нет — используем ваш сервис регистрации!
                    // Генерируем случайный пароль, так как он не нужен при входе через Google
                    var randomPassword = Guid.NewGuid().ToString();
                    var login = payload.Name ?? payload.GivenName ?? "GoogleUser";

                    // Используем _authService, который сам захэширует пароль как надо
                    bool registerResult = await _authService.RegisterAsync(payload.Email, login, randomPassword);

                    if (!registerResult)
                    {
                        return Json(new { success = false, message = "Не удалось создать пользователя (возможно, логин занят)." });
                    }

                    // Получаем созданного пользователя, чтобы взять его данные (Role и т.д.)
                    user = await _context.UserDb.FirstOrDefaultAsync(u => u.Email == payload.Email);
                }

                // 4. Входим в систему (создаем куки)
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.Login),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim(ClaimTypes.Role, user.Role)
                };

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var authProperties = new AuthenticationProperties { IsPersistent = true };

                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(claimsIdentity),
                    authProperties);

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Ошибка Google: " + ex.Message });
            }
        }

        // ================= РЕГИСТРАЦИЯ =================

        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> Register([FromBody] RegisterViewModel model)
        {
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(model.Email)) errors.Add("Email обязателен");
            if (string.IsNullOrWhiteSpace(model.Login)) errors.Add("Логин обязателен");
            if (string.IsNullOrWhiteSpace(model.Password)) errors.Add("Пароль обязателен");
            if (model.Password != model.PasswordConfirm) errors.Add("Пароли не совпадают");

            if (errors.Any()) return Json(new { isSuccess = false, errors });

            try
            {
                var result = await _authService.RegisterAsync(model.Email, model.Login, model.Password);

                if (!result)
                    return Json(new { isSuccess = false, errors = new[] { "Пользователь уже существует" } });

                return Json(new { isSuccess = true });
            }
            catch (Exception ex)
            {
                return Json(new { isSuccess = false, errors = new[] { ex.Message } });
            }
        }

        // ================= ВХОД =================

        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> Login([FromBody] LoginViewModel model)
        {
            if (string.IsNullOrWhiteSpace(model.LoginOrEmail) || string.IsNullOrWhiteSpace(model.Password))
                return Json(new { isSuccess = false, errors = new[] { "Введите логин и пароль" } });

            var result = await _authService.LoginAsync(model.LoginOrEmail, model.Password);

            if (!result)
                return Json(new { isSuccess = false, errors = new[] { "Неверный логин или пароль" } });

            return Json(new { isSuccess = true });
        }

        // ================= ПРОСТЫЕ VIEW =================

        public IActionResult AboutUs() => View();
        public IActionResult Services() => View();
        public IActionResult Contacts() => View();
        public IActionResult SiteInformation() => View();
        public IActionResult Privacy() => View();
        public IActionResult Error() => View();
    }

    // ================= DTO =================

    public class RegisterViewModel
    {
        public string Email { get; set; } = string.Empty;
        public string Login { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string PasswordConfirm { get; set; } = string.Empty;
    }

    public class LoginViewModel 
    {
        public string LoginOrEmail { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}