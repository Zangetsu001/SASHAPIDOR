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
    public DbSet<UserDb> UsersDb { get; set; }

    // Таблица категорий
    public DbSet<CategoryDb> CategoriesDb { get; set; }

    // Таблица курсов
    public DbSet<CourseDb> CoursesDb { get; set; }

    // Таблица картинок курсов
    public DbSet<CourseImageDb> CourseImagesDb { get; set; }
}