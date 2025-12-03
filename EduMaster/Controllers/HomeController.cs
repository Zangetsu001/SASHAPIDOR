
using EduMaster.Domain.ModelsDb;
using EduMaster.Services;
using Google.Apis.Auth;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace EduMaster.Controllers
{
    public class HomeController : Controller
    {
        private readonly IAuthService _authService;
        private readonly ICourseService _courseService;
        private readonly ApplicationDbContext _context; // Нам нужен контекст, чтобы искать пользователя
        private readonly IEmailService _emailService; // Добавили
        private readonly IMemoryCache _cache;

        public HomeController(IAuthService authService, ICourseService courseService, ApplicationDbContext context, IEmailService emailService, IMemoryCache cache)
        {
            _authService = authService;
            _courseService = courseService;
            _context = context;
            _emailService = emailService;
            _cache = cache;
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
    

            var userName = User.Identity?.Name ?? "Гость";
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

            // Проверяем, есть ли уже такой юзер в БД
            var userExists = await _context.UserDb.AnyAsync(u => u.Email == model.Email || u.Login == model.Login);
            if (userExists) errors.Add("Пользователь с таким Email или Логином уже существует");

            if (errors.Any()) return Json(new { isSuccess = false, errors });

            // Генерируем код (4 цифры)
            var code = new Random().Next(1000, 9999).ToString();

            // Сохраняем данные пользователя и код в Кэш на 10 минут
            // Ключ кэша - это Email пользователя
            _cache.Set(model.Email, new RegistrationCacheModel
            {
                Login = model.Login,
                Password = model.Password,
                Code = code
            }, TimeSpan.FromMinutes(10));

            // Отправляем письмо
            try
            {
                var emailBody = GetHtmlEmailTemplate(model.Login, code);

                await _emailService.SendEmailAsync(model.Email, "Подтверждение регистрации EduMaster", emailBody);
            }
            catch (Exception ex)
            {
                return Json(new { isSuccess = false, errors = new[] { "Ошибка отправки письма: " + ex.Message } });
            }

            // Возвращаем успех и говорим фронтенду показать окно ввода кода
            return Json(new { isSuccess = true, requireCode = true, email = model.Email });
        }

        // ================= РЕГИСТРАЦИЯ (ШАГ 2: ПРОВЕРКА КОДА) =================
        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> ConfirmRegistration([FromBody] ConfirmViewModel model)
        {
            // Пытаемся достать данные из кэша по Email
            if (!_cache.TryGetValue(model.Email, out RegistrationCacheModel cachedUser))
            {
                return Json(new { isSuccess = false, message = "Время действия кода истекло или email неверный. Попробуйте снова." });
            }

            if (cachedUser.Code != model.Code)
            {
                return Json(new { isSuccess = false, message = "Неверный код подтверждения." });
            }

            // Если код верный - регистрируем реально
            var result = await _authService.RegisterAsync(model.Email, cachedUser.Login, cachedUser.Password);

            if (!result)
                return Json(new { isSuccess = false, message = "Ошибка при создании пользователя в БД." });

            // Удаляем из кэша
            _cache.Remove(model.Email);

            return Json(new { isSuccess = true });
        }

        // Вспомогательный класс для кэша (можно добавить внизу файла)
        private class RegistrationCacheModel
        {
            public string Login { get; set; }
            public string Password { get; set; }
            public string Code { get; set; }
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
        private string GetHtmlEmailTemplate(string login, string code)
        {
            string body = $@"
            <div style='font-family: Helvetica, Arial, sans-serif; min-width: 320px; max-width: 600px; margin: 0 auto; overflow: auto; line-height: 2;'>
                <div style='margin: 20px auto; width: 90%; padding: 20px 0;'>
                    <div style='border-bottom: 1px solid #eee; padding-bottom: 10px;'>
                        <a href='#' style='font-size: 1.4em; color: #00466a; text-decoration: none; font-weight: 600;'>EduMaster</a>
                    </div>
                    <p style='font-size: 1.1em;'>Здравствуйте, <b>{login}</b>!</p>
                    <p>Спасибо за регистрацию на платформе EduMaster. Для завершения создания аккаунта используйте следующий код подтверждения:</p>
                    <h2 style='background: #00466a; margin: 0 auto; width: max-content; padding: 0 10px; color: #fff; border-radius: 4px; letter-spacing: 4px;'>{code}</h2>
                    <p style='font-size: 0.9em;'>Этот код действителен в течение 10 минут.</p>
                    <hr style='border: none; border-top: 1px solid #eee;' />
                    <div style='float: right; padding: 8px 0; color: #aaa; font-size: 0.8em; line-height: 1; font-weight: 300;'>
                        <p>EduMaster Inc</p>
                        <p>Если вы не регистрировались, просто проигнорируйте это письмо.</p>
                    </div>
                </div>
            </div>";
            return body;
        }
    }
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
//private class RegistrationCacheModel
//{
//    public string Login { get; set; }
//    public string Password { get; set; }
//    public string Code { get; set; }
//}


// DTO для подтверждения (добавь в конец файла)
public class ConfirmViewModel
{
    public string Email { get; set; }
    public string Code { get; set; }
}

