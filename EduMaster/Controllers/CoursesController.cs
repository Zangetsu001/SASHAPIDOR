using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EduMaster.DAL;
using EduMaster.Models.ViewModels;

namespace EduMaster.Controllers
{
    public class CoursesController : Controller
    {
        private readonly ApplicationDbContext _db;

        public CoursesController(ApplicationDbContext db)
        {
            _db = db;
        }

        // /Courses
        public IActionResult Index(string? search, Guid? categoryId)
        {
            var vm = new CoursesIndexViewModel
            {
                Search = search ?? string.Empty,
                SelectedCategoryId = categoryId
            };

            // ------------------- КАТЕГОРИИ -------------------
            vm.Categories = _db.CategoryDb
                               .OrderBy(c => c.name)
                               .ToList();   // <-- ВАЖНО: List<CategoryDb>, НИ КАКОГО Dictionary!

            // ------------------- КУРСЫ -----------------------
            var query = _db.CourseDb
                           .Include(c => c.category_id) // если есть навигационное свойство
                           .AsQueryable();

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
    }
}
