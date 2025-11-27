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

namespace EduMaster.Controllers
{
    public class HomeController : Controller
    {
        private readonly IAuthService _authService;
        private readonly ICourseService _courseService;

        public HomeController(IAuthService authService, ICourseService courseService)
        {
            _authService = authService;
            _courseService = courseService;
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
        public async Task<IActionResult> Dashboard()
        {
            string userName = User.Identity?.Name ?? "Пользователь";

            // Получаем id курсов из сессии
            var stored = HttpContext.Session.GetString("myCourses");
            List<Guid> selectedIds = string.IsNullOrEmpty(stored)
                ? new List<Guid>()
                : System.Text.Json.JsonSerializer.Deserialize<List<Guid>>(stored);

            // Загружаем все курсы из БД
            var allCourses = await _courseService.GetActiveCoursesAsync();

            // Мои курсы
            var myCourses = allCourses
                .Where(c => selectedIds.Contains(c.Id))
                .ToList();

            // Доступные курсы
            var availableCourses = allCourses
                .Where(c => !selectedIds.Contains(c.Id))
                .ToList();

            ViewBag.MyCourses = myCourses;
            ViewBag.AllCourses = availableCourses;

            return View("Dashboard", userName);
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

            return RedirectToAction("Dashboard");
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

            return RedirectToAction("Dashboard");
        }

        // ================= ВЫХОД =================

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
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
