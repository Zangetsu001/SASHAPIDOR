using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EduMaster.DAL;
using EduMaster.Domain.Models.ViewModels;
using EduMaster.Domain.ModelsDb;
using EduMaster.Services;
using System.Security.Claims;

namespace EduMaster.Controllers
{
    public class CoursesController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly ICartService _cartService; // Сервис корзины

        // Добавляем ICartService в конструктор
        public CoursesController(ApplicationDbContext db, ICartService cartService)
        {
            _db = db;
            _cartService = cartService;
        }

        // === СТРАНИЦА КАТАЛОГА (Поиск + Фильтры) ===
        public IActionResult Catalog(string? search, Guid? categoryId)
        {
            var vm = new CoursesIndexViewModel
            {
                Search = search ?? string.Empty,
                SelectedCategoryId = categoryId
            };

            // Загружаем категории для фильтра
            vm.Categories = _db.CategoryDb.OrderBy(c => c.name).ToList();

            // Начальный запрос к курсам
            var query = _db.CourseDb.AsQueryable();

            // Фильтр по поиску
            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(c => c.title.Contains(search) || (c.description != null && c.description.Contains(search)));
            }

            // Фильтр по категории
            if (categoryId.HasValue)
            {
                query = query.Where(c => c.category_id == categoryId.Value);
                vm.SelectedCategory = vm.Categories.FirstOrDefault(c => c.Id == categoryId.Value);
            }

            // Выполняем запрос
            vm.Courses = query.ToList();

            return View(vm);
        }

        // === МЕТОД: ДОБАВИТЬ В КОРЗИНУ ===
        [HttpPost]
        public async Task<IActionResult> AddToCart(Guid courseId)
        {
            // 1. Проверяем, вошел ли пользователь
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdString))
            {
                // Если не вошел — отправляем на логин
                return RedirectToAction("Login", "Auth");
            }

            // 2. Парсим ID и добавляем в корзину через сервис
            if (Guid.TryParse(userIdString, out Guid userId))
            {
                await _cartService.AddToCartAsync(userId, courseId);
            }

            // 3. Возвращаем пользователя обратно в каталог
            return RedirectToAction("Catalog");
        }

        // === МЕТОД: ДЕТАЛИ КУРСА (для модального окна) ===
        [HttpGet]
        public IActionResult GetCourseDetails(Guid id)
        {
            var course = _db.CourseDb.FirstOrDefault(c => c.Id == id);
            if (course == null) return NotFound();

            var categoryName = _db.CategoryDb
                .Where(c => c.Id == course.category_id)
                .Select(c => c.name)
                .FirstOrDefault() ?? "Курс";

            return Json(new
            {
                title = course.title,
                description = course.description,
                price = course.price,
                category = categoryName
            });
        }

        // === МЕТОД: ПОЛУЧЕНИЕ КАРТИНКИ ===
        [HttpGet]
        public IActionResult GetImage(Guid courseId)
        {
            var image = _db.CourseImageDb.FirstOrDefault(i => i.CourseId == courseId);

            if (image != null && image.Data != null)
            {
                return File(image.Data, image.ContentType ?? "image/jpeg");
            }
            // Если картинки нет, можно вернуть заглушку или NotFound
            return NotFound();
        }
    }
}