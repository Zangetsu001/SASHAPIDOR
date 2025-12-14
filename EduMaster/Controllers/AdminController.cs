using EduMaster.DAL;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace EduMaster.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _db;

        public AdminController(ApplicationDbContext db)
        {
            _db = db;
        }

        // Главная страница админки (переименована в AdminDashboard)
        public IActionResult AdminDashboard()
        {
            return View(_db.CourseDb.ToList());
        }

        [HttpPost]
        public IActionResult Delete(System.Guid id)
        {
            var course = _db.CourseDb.Find(id);
            if (course != null)
            {
                _db.CourseDb.Remove(course);
                _db.SaveChanges();
            }
            return RedirectToAction("AdminDashboard");
        }
    }
}