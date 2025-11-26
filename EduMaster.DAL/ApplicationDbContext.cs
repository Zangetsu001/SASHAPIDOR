using EduMaster.Domain.ModelsDb;
using Microsoft.EntityFrameworkCore;
// Не забудьте добавить using для папки, где лежат ваши классы сущностей (например, UserDb)
// using ProjectName.Domain.Entities; 

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    // === Добавьте этот блок кода в ваш класс ===

    // Таблица пользователей
    public DbSet<UserDb> UserDb { get; set; }

    // Таблица категорий
    public DbSet<CategoryDb> CategoryDb { get; set; }

    // Таблица курсов
    public DbSet<CourseDb> CourseDb { get; set; }

    // Таблица картинок курсов
    public DbSet<CourseImageDb> CourseImageDb { get; set; }
}