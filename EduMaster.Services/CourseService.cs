using EduMaster.Domain.ModelsDb;
using EduMaster.Domain.Models;
using Microsoft.EntityFrameworkCore;
using System;
using EduMaster.DAL;

namespace EduMaster.Services
{
    public class CourseService : ICourseService
    {
        private readonly ApplicationDbContext _context;

        public CourseService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Course>> GetActiveCoursesAsync()
        {
            var coursesDb = await _context.CourseDb
                .Where(c => c.is_active)
                .ToListAsync();

            return coursesDb.Select(MapToCourse).ToList();
        }

        public async Task<List<Course>> GetCoursesByIdsAsync(List<Guid> ids)
        {
            var coursesDb = await _context.CourseDb
                .Where(c => ids.Contains(c.Id))
                .ToListAsync();

            return coursesDb.Select(MapToCourse).ToList();
        }

        public async Task<Course?> GetCourseAsync(Guid id)
        {
            var courseDb = await _context.CourseDb
                .FirstOrDefaultAsync(c => c.Id == id);

            return courseDb != null ? MapToCourse(courseDb) : null;
        }

        private Course MapToCourse(CourseDb courseDb)
        {
            return new Course
            {
                Id = courseDb.Id,
                Title = courseDb.title ?? string.Empty,
                Description = courseDb.description ?? string.Empty,
                Price = courseDb.price,
                IsActive = courseDb.is_active,
                CreatedAt = courseDb.created_at,
                CategoryId = courseDb.category_id,
                Category = null,
                Images = new List<CourseImage>()
            };
        }
    }
}
