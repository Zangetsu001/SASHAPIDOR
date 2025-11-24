using Microsoft.EntityFrameworkCore; // <--- Этот using исправляет ошибку
using System.Threading.Tasks;
using System.Linq;
using System;
using EduMaster.DAL;
using EduMaster.Domain.ModelsDb;
using EduMaster.DAL.Interfaces;

public class CourseStorage : IBaseStorage<CourseDb>
{
    public readonly ApplicationDbContext _db;

    public CourseStorage(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task Add(CourseDb item)
    {
        await _db.CoursesDb.AddAsync(item);
        await _db.SaveChangesAsync();
    }

    public async Task Delete(CourseDb item)
    {
        _db.CoursesDb.Remove(item);
        await _db.SaveChangesAsync();
    }

    public async Task<CourseDb> Get(Guid id)
    {
        return await _db.CoursesDb.FirstOrDefaultAsync(x => x.Id == id);
    }

    public IQueryable<CourseDb> GetAll()
    {
        return _db.CoursesDb;
    }

    public async Task<CourseDb> Update(CourseDb item)
    {
        _db.CoursesDb.Update(item);
        await _db.SaveChangesAsync();
        return item;
    }
}