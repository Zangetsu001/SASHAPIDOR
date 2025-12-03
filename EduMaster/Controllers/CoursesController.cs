using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using EduMaster.Domain.Models.ViewModels; // <--- ТЕПЕРЬ ЭТО БУДЕТ РАБОТАТЬ
using EduMaster.Domain.ModelsDb;

namespace EduMaster.Controllers
{
    public class CoursesController : Controller
    {
        private readonly ApplicationDbContext _db;

        public CoursesController(ApplicationDbContext db)
        {
            _db = db;
        }

        // ================= СТРАНИЦА КУРСОВ (КАТАЛОГ) =================
        // Можно назвать Catalog или Index, главное чтобы View называлась так же
        public IActionResult Catalog(string? search, Guid? categoryId)
        {
            var vm = new CoursesIndexViewModel
            {
                Search = search ?? string.Empty,
                SelectedCategoryId = categoryId
            };

            // 1. Загружаем категории
            vm.Categories = _db.CategoryDb
                               .OrderBy(c => c.name)
                               .ToList();

            // 2. Загружаем курсы
            var query = _db.CourseDb.AsQueryable();

            // ВАЖНО: Я убрал .Include(c => c.category_id), так как это вызывает ошибку.
            // Если вы добавили навигационное свойство 'Category' в CourseDb.cs, 
            // то раскомментируйте строку ниже:
            // query = query.Include(c => c.Category);

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(c =>
                    c.title.Contains(search) ||
                    (c.description != null && c.description.Contains(search)));
            }

            if (categoryId.HasValue)
            {
                query = query.Where(c => c.category_id == categoryId.Value);
                vm.SelectedCategory = vm.Categories.FirstOrDefault(c => c.Id == categoryId.Value);
            }

            vm.Courses = query.ToList();

            return View(vm);
        }

        // ================= ДЕТАЛИ КУРСА (API для модального окна) =================
        [HttpGet]
        public IActionResult GetCourseDetails(Guid id)
        {
            var course = _db.CourseDb.FirstOrDefault(c => c.Id == id);
            if (course == null) return NotFound();

            // Ищем имя категории вручную (так как Include может не работать без настройки связей)
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
        [HttpGet]
        public IActionResult GetImage(Guid courseId)
        {
            // Ищем картинку, привязанную к курсу
            var image = _db.CourseImageDb.FirstOrDefault(i => i.CourseId == courseId);

            if (image != null && image.Data != null)
            {
                // Возвращаем файл (картинку) браузеру
                return File(image.Data, image.ContentType ?? "image/jpeg");
            }

            // Если картинки нет — возвращаем 404
            return NotFound();
        }
    }
}