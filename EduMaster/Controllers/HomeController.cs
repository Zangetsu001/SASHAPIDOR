using EduMaster.DAL;
using EduMaster.Domain.ModelsDb;
using EduMaster.Services;
using Google.Apis.Auth;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;

namespace EduMaster.Controllers
{
    public class HomeController : Controller
    {
        private readonly IAuthService _authService;
        private readonly ICourseService _courseService;
        private readonly ApplicationDbContext _context;
        private readonly IEmailService _emailService;
        private readonly IMemoryCache _cache;
        private readonly IConfiguration _configuration;

        // Имя файла для хранения подписок пользователей
        private const string CoursesFileName = "user_courses.json";

        public HomeController(
            IAuthService authService,
            ICourseService courseService,
            ApplicationDbContext context,
            IEmailService emailService,
            IMemoryCache cache,
            IConfiguration configuration)
        {
            _authService = authService;
            _courseService = courseService;
            _context = context;
            _emailService = emailService;
            _cache = cache;
            _configuration = configuration;
        }

        // ================= ГЛАВНАЯ СТРАНИЦА =================
        public async Task<IActionResult> Index()
        {
            // 1. Загружаем активные курсы
            var courses = await _courseService.GetActiveCoursesAsync();
            ViewBag.AllCourses = courses;

            // 2. Загружаем категории для карточек на главной
            ViewBag.Categories = await _context.CategoryDb.OrderBy(c => c.name).ToListAsync();

            return View();
        }

        // ================= ЛИЧНЫЙ КАБИНЕТ (JSON ВЕРСИЯ) =================
        [Authorize]
        [HttpGet]
        public IActionResult Dashboard()
        {
            var userName = User.Identity?.Name;
            if (string.IsNullOrEmpty(userName)) return RedirectToAction("Index");

            // 1. Получаем список ID курсов из JSON файла (вместо Сессии)
            var userCourseIds = GetCoursesForUser(userName);

            // 2. Загружаем полные данные курсов из БД по этим ID
            if (userCourseIds != null && userCourseIds.Any())
            {
                ViewBag.MyCourses = _context.CourseDb
                    .Where(c => userCourseIds.Contains(c.Id))
                    .ToList();
            }
            else
            {
                ViewBag.MyCourses = new List<CourseDb>();
            }

            return PartialView("_DashboardPartial", userName);
        }

        // ================= ЗАПИСАТЬСЯ НА КУРС (JSON) =================
        [Authorize]
        [HttpPost]
        public IActionResult AddCourse(Guid id)
        {
            var userName = User.Identity?.Name;
            if (string.IsNullOrEmpty(userName)) return Unauthorized();

            // Читаем всё, добавляем, сохраняем обратно в файл
            var allData = LoadAllEnrollments();

            if (!allData.ContainsKey(userName))
            {
                allData[userName] = new List<Guid>();
            }

            if (!allData[userName].Contains(id))
            {
                allData[userName].Add(id);
                SaveAllEnrollments(allData);
            }

            return Json(new { success = true });
        }

        // ================= ОТПИСАТЬСЯ ОТ КУРСА (JSON) =================
        [Authorize]
        [HttpPost]
        public IActionResult RemoveCourse(Guid id)
        {
            var userName = User.Identity?.Name;
            if (string.IsNullOrEmpty(userName)) return Unauthorized();

            var allData = LoadAllEnrollments();

            if (allData.ContainsKey(userName) && allData[userName].Contains(id))
            {
                allData[userName].Remove(id);
                SaveAllEnrollments(allData);
            }

            return Json(new { success = true });
        }

        // ================= ОТПРАВКА СООБЩЕНИЯ (КРАСИВЫЙ ШАБЛОН) =================
        [HttpPost]
        public async Task<IActionResult> SendMessage([FromBody] MessageViewModel model)
        {
            // 1. Автозаполнение данных, если пользователь авторизован
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                var emailClaim = User.Claims.FirstOrDefault(c => ClaimTypes.Email == c.Type);
                var nameClaim = User.Claims.FirstOrDefault(c => ClaimTypes.Name == c.Type);

                if (emailClaim != null)
                {
                    model.Email = emailClaim.Value;
                }

                if (string.IsNullOrWhiteSpace(model.Name))
                {
                    model.Name = nameClaim?.Value ?? User.Identity.Name ?? "Пользователь";
                }
            }

            if (string.IsNullOrWhiteSpace(model.Name) || string.IsNullOrWhiteSpace(model.Email) || string.IsNullOrWhiteSpace(model.Message))
            {
                return Json(new { success = false, message = "Пожалуйста, заполните все поля." });
            }

            try
            {
                // Берем почту админа (куда слать) из конфига
                string adminEmail = _configuration["SMTP:User"];

                // 2. Красивый HTML шаблон письма
                string body = $@"
                <!DOCTYPE html>
                <html>
                <head>
                    <style>
                        body {{ font-family: Arial, sans-serif; background-color: #f4f4f4; padding: 20px; }}
                        .container {{ max-width: 600px; margin: 0 auto; background: #fff; padding: 20px; border-radius: 8px; box-shadow: 0 2px 5px rgba(0,0,0,0.1); }}
                        h2 {{ color: #0056b3; border-bottom: 2px solid #eee; padding-bottom: 10px; }}
                        .info {{ margin-bottom: 20px; }}
                        .info p {{ margin: 5px 0; }}
                        .message-box {{ background: #f9f9f9; padding: 15px; border-left: 4px solid #0056b3; font-style: italic; }}
                        .footer {{ margin-top: 30px; font-size: 12px; color: #888; text-align: center; }}
                    </style>
                </head>
                <body>
                    <div class='container'>
                        <h2>📩 Новое сообщение с сайта</h2>
                        <div class='info'>
                            <p><b>От кого:</b> {model.Name}</p>
                            <p><b>Email:</b> <a href='mailto:{model.Email}'>{model.Email}</a></p>
                            <p><b>Тема:</b> {model.Subject ?? "Без темы"}</p>
                        </div>
                        <p><b>Текст сообщения:</b></p>
                        <div class='message-box'>
                            {model.Message.Replace("\n", "<br>")}
                        </div>
                        <div class='footer'>
                            Это письмо отправлено автоматически с сайта EduMaster.<br>
                            Нажмите 'Ответить', чтобы написать пользователю напрямую.
                        </div>
                    </div>
                </body>
                </html>";

                // 3. Отправляем письмо
                await _emailService.SendEmailAsync(
                    to: adminEmail,
                    subject: $"EduMaster: {model.Subject ?? "Обратная связь"}",
                    body: body,
                    replyTo: model.Email,    // <--- Ответ уйдет на почту пользователя
                    fromName: model.Name     // <--- Имя отправителя будет именем пользователя
                );

                return Json(new { success = true, message = "Сообщение отправлено! Мы свяжемся с вами." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Ошибка отправки: " + ex.Message });
            }
        }

        // ================= РЕГИСТРАЦИЯ (ШАГ 1: ОТПРАВКА КОДА) =================
        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> Register([FromBody] RegisterViewModel model)
        {
            if (string.IsNullOrWhiteSpace(model.Email) || string.IsNullOrWhiteSpace(model.Login) ||
                string.IsNullOrWhiteSpace(model.Password) || model.Password != model.PasswordConfirm)
            {
                return Json(new { isSuccess = false, errors = new[] { "Проверьте правильность данных." } });
            }

            var userExists = await _context.UserDb.AnyAsync(u => u.Email == model.Email || u.Login == model.Login);
            if (userExists) return Json(new { isSuccess = false, errors = new[] { "Пользователь с таким Email или Логином уже существует." } });

            // Генерируем код
            var code = new Random().Next(1000, 9999).ToString();

            // Сохраняем в кэш на 10 минут
            _cache.Set(model.Email, new RegistrationCacheModel
            {
                Login = model.Login,
                Password = model.Password,
                Code = code
            }, TimeSpan.FromMinutes(10));

            try
            {
                var emailBody = GetHtmlEmailTemplate(model.Login, code);
                // Для системных писем (код подтверждения) ReplyTo не нужен, имя стандартное
                await _emailService.SendEmailAsync(model.Email, "Подтверждение регистрации EduMaster", emailBody);
            }
            catch (Exception ex)
            {
                return Json(new { isSuccess = false, errors = new[] { "Ошибка отправки письма: " + ex.Message } });
            }

            return Json(new { isSuccess = true, requireCode = true, email = model.Email });
        }

        // ================= РЕГИСТРАЦИЯ (ШАГ 2: ПРОВЕРКА КОДА) =================
        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> ConfirmRegistration([FromBody] ConfirmViewModel model)
        {
            if (!_cache.TryGetValue(model.Email, out RegistrationCacheModel cachedUser))
            {
                return Json(new { isSuccess = false, message = "Код истек или email неверен." });
            }

            if (cachedUser.Code != model.Code)
            {
                return Json(new { isSuccess = false, message = "Неверный код подтверждения." });
            }

            var result = await _authService.RegisterAsync(model.Email, cachedUser.Login, cachedUser.Password);

            if (!result) return Json(new { isSuccess = false, message = "Ошибка при создании пользователя в БД." });

            _cache.Remove(model.Email);
            return Json(new { isSuccess = true });
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

        // ================= ВХОД ЧЕРЕЗ GOOGLE =================
        [HttpPost]
        public async Task<IActionResult> GoogleLogin(string credential)
        {
            try
            {
                var settings = new GoogleJsonWebSignature.ValidationSettings()
                {
                    Audience = new List<string>() { "658972345156-chmbutbtvpqmeflh5jq3hmge1omvqtlt.apps.googleusercontent.com" }
                };

                var payload = await GoogleJsonWebSignature.ValidateAsync(credential, settings);
                var user = await _context.UserDb.FirstOrDefaultAsync(u => u.Email == payload.Email);

                if (user == null)
                {
                    var randomPassword = Guid.NewGuid().ToString();
                    var login = payload.Name ?? payload.GivenName ?? "GoogleUser";

                    bool registerResult = await _authService.RegisterAsync(payload.Email, login, randomPassword);

                    if (!registerResult) return Json(new { success = false, message = "Не удалось создать пользователя." });

                    user = await _context.UserDb.FirstOrDefaultAsync(u => u.Email == payload.Email);
                }

                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.Login),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim(ClaimTypes.Role, user.Role)
                };

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity), new AuthenticationProperties { IsPersistent = true });

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Ошибка Google: " + ex.Message });
            }
        }

        // ================= ВЫХОД =================
        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            // Сессия больше не используется для хранения курсов, но очистить ее можно
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home");
        }

        // ================= ВСПОМОГАТЕЛЬНЫЕ МЕТОДЫ (JSON & EMAIL) =================

        // 1. Чтение файла подписок
        private Dictionary<string, List<Guid>> LoadAllEnrollments()
        {
            if (!System.IO.File.Exists(CoursesFileName))
            {
                return new Dictionary<string, List<Guid>>();
            }

            try
            {
                var json = System.IO.File.ReadAllText(CoursesFileName);
                if (string.IsNullOrWhiteSpace(json)) return new Dictionary<string, List<Guid>>();

                return JsonSerializer.Deserialize<Dictionary<string, List<Guid>>>(json)
                       ?? new Dictionary<string, List<Guid>>();
            }
            catch
            {
                return new Dictionary<string, List<Guid>>();
            }
        }

        // 2. Запись файла подписок
        private void SaveAllEnrollments(Dictionary<string, List<Guid>> data)
        {
            var json = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
            System.IO.File.WriteAllText(CoursesFileName, json);
        }

        // 3. Получение курсов конкретного юзера
        private List<Guid> GetCoursesForUser(string userName)
        {
            var allData = LoadAllEnrollments();
            return allData.ContainsKey(userName) ? allData[userName] : new List<Guid>();
        }

        private string GetHtmlEmailTemplate(string login, string code)
        {
            return $@"
            <div style='font-family: Helvetica, Arial, sans-serif; padding: 20px;'>
                <div style='border-bottom: 1px solid #eee; padding-bottom: 10px;'>
                    <h2 style='color: #00466a;'>EduMaster</h2>
                </div>
                <p>Здравствуйте, <b>{login}</b>!</p>
                <p>Ваш код подтверждения:</p>
                <h2 style='background: #00466a; color: #fff; padding: 10px 20px; display: inline-block; border-radius: 5px;'>{code}</h2>
                <p style='color: #888; font-size: 0.9em;'>Код действителен 10 минут.</p>
            </div>";
        }

        private class RegistrationCacheModel
        {
            public string Login { get; set; }
            public string Password { get; set; }
            public string Code { get; set; }
        }

        public IActionResult AboutUs() => View();
        public IActionResult Services() => View();
        public IActionResult Contacts() => View();
        public IActionResult SiteInformation() => View();
        public IActionResult Privacy() => View();
        public IActionResult Error() => View();
    }

    // ================= DTO КЛАССЫ =================

    public class RegisterViewModel
    {
        public string Email { get; set; }
        public string Login { get; set; }
        public string Password { get; set; }
        public string PasswordConfirm { get; set; }
    }

    public class LoginViewModel
    {
        public string LoginOrEmail { get; set; }
        public string Password { get; set; }
    }

    public class ConfirmViewModel
    {
        public string Email { get; set; }
        public string Code { get; set; }
    }

    public class MessageViewModel
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public string Subject { get; set; }
        public string Message { get; set; }
    }
}