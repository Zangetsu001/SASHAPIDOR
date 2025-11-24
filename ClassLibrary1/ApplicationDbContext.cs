using EduMaster.Domain.ModelsDb;
using Microsoft.EntityFrameworkCore;

namespace EduMaster.DAL
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<UserDb> Users { get; set; } = null!;
    }
}
