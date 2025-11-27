using EduMaster.Domain.Models;

namespace EduMaster.Services
{
    public interface ICourseService
    {
        Task<List<Course>> GetActiveCoursesAsync();
        Task<List<Course>> GetCoursesByIdsAsync(List<Guid> ids);
        Task<Course?> GetCourseAsync(Guid id);
    }
}
