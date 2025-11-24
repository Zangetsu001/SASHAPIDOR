using Microsoft.AspNetCore.Mvc;
using EduMaster.Services;

namespace EduMaster.Controllers
{
    public class HomeController : Controller
    {
        private readonly IAuthService _authService;

        public HomeController(IAuthService authService)
        {
            _authService = authService;
        }

        // ================= РЕГИСТРАЦИЯ =================

        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> Register([FromBody] RegisterViewModel model)
        {
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(model.Email))
                errors.Add("Email обязателен");
            if (string.IsNullOrWhiteSpace(model.Login))
                errors.Add("Логин обязателен");
            if (string.IsNullOrWhiteSpace(model.Password))
                errors.Add("Пароль обязателен");
            if (model.Password != model.PasswordConfirm)
                errors.Add("Пароли не совпадают");

            if (errors.Any())
                return Json(new { isSuccess = false, errors });

            try
            {
                var user = await _authService.RegisterAsync(
                    model.Login,
                    model.Email,
                    model.Password
                );

                if (user == null)
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
            if (string.IsNullOrWhiteSpace(model.LoginOrEmail) ||
                string.IsNullOrWhiteSpace(model.Password))
            {
                return Json(new
                {
                    isSuccess = false,
                    errors = new[] { "Введите логин или email и пароль" }
                });
            }

            var user = await _authService.LoginAsync(model.LoginOrEmail, model.Password);

            if (user == null)
                return Json(new { isSuccess = false, errors = new[] { "Неверный логин/email или пароль" } });

            return Json(new { isSuccess = true });
        }

        // ================= VIEW =================

        public IActionResult Index() => View();
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
