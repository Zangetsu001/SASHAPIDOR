using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EduMaster.DAL;
using EduMaster.Domain.Models.ViewModels;
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

        // Каталог с фильтрами
        public IActionResult Catalog(string? search, Guid? categoryId)
        {
            var vm = new CoursesIndexViewModel
            {
                Search = search ?? string.Empty,
                SelectedCategoryId = categoryId
            };

            vm.Categories = _db.CategoryDb.OrderBy(c => c.name).ToList();
            var query = _db.CourseDb.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(c => c.title.Contains(search) || (c.description != null && c.description.Contains(search)));
            }

            if (categoryId.HasValue)
            {
                query = query.Where(c => c.category_id == categoryId.Value);
                vm.SelectedCategory = vm.Categories.FirstOrDefault(c => c.Id == categoryId.Value);
            }

            vm.Courses = query.ToList();
            return View(vm);
        }

        // Детали для модального окна
        [HttpGet]
        public IActionResult GetCourseDetails(Guid id)
        {
            var course = _db.CourseDb.FirstOrDefault(c => c.Id == id);
            if (course == null) return NotFound();

            var categoryName = _db.CategoryDb.Where(c => c.Id == course.category_id).Select(c => c.name).FirstOrDefault() ?? "Курс";

            return Json(new { title = course.title, description = course.description, price = course.price, category = categoryName });
        }

        // === МЕТОД: ПОЛУЧЕНИЕ КАРТИНКИ ИЗ БД ===
        [HttpGet]
        public IActionResult GetImage(Guid courseId)
        {
            var image = _db.CourseImageDb.FirstOrDefault(i => i.CourseId == courseId);

            if (image != null && image.Data != null)
            {
                return File(image.Data, image.ContentType ?? "image/jpeg");
            }
            return NotFound();
        }
    }
}