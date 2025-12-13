using Microsoft.AspNetCore.Mvc;
using EduMaster.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Threading.Tasks;

namespace EduMaster.Controllers
{
    public class AuthController : Controller
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        // === СТРАНИЦА ВХОДА ===
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        // === ОБРАБОТКА ВХОДА ===
        [HttpPost]
        public async Task<IActionResult> Login(string login, string password)
        {
            if (await _authService.LoginAsync(login, password))
            {
                return RedirectToAction("Index", "Home");
            }

            ViewBag.Error = "Неверный логин или пароль";
            return View();
        }

        // === СТРАНИЦА РЕГИСТРАЦИИ ===
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        // === ОБРАБОТКА РЕГИСТРАЦИИ ===
        [HttpPost]
        public async Task<IActionResult> Register(string email, string login, string password, string confirmPassword)
        {
            if (password != confirmPassword)
            {
                ViewBag.Error = "Пароли не совпадают";
                return View();
            }

            if (await _authService.RegisterAsync(email, login, password))
            {
                return RedirectToAction("Index", "Home");
            }

            ViewBag.Error = "Пользователь с таким Email или Login уже существует";
            return View();
        }

        // === ВЫХОД ===
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }
    }
}