using EduMaster.Domain.ModelsDb;
using Microsoft.EntityFrameworkCore;

namespace EduMaster.DAL  // <--- ВОТ ЭТОЙ СТРОКИ НЕ ХВАТАЛО
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // Таблица пользователей
        public DbSet<UserDb> UserDb { get; set; }

        // Таблица категорий
        public DbSet<CategoryDb> CategoryDb { get; set; }

        // Таблица курсов
        public DbSet<CourseDb> CourseDb { get; set; }

        // Таблица картинок курсов
        public DbSet<CourseImageDb> CourseImageDb { get; set; }

        // Таблица корзины/записей (ОБЯЗАТЕЛЬНО для работы корзины!)
        public DbSet<EnrollmentDb> EnrollmentDb { get; set; }
    }
}