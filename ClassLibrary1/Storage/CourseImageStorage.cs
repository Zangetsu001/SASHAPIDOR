using Microsoft.EntityFrameworkCore; // <--- Этот using исправляет ошибку
using System.Threading.Tasks;
using System.Linq;
using System;
using EduMaster.DAL;
using EduMaster.Domain.ModelsDb;
using EduMaster.DAL.Interfaces;

public class CourseImageStorage : IBaseStorage<CourseImageDb>
{
    public readonly ApplicationDbContext _db;

    public CourseImageStorage(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task Add(CourseImageDb item)
    {
        await _db.CourseImagesDb.AddAsync(item);
        await _db.SaveChangesAsync();
    }

    public async Task Delete(CourseImageDb item)
    {
        _db.CourseImagesDb.Remove(item);
        await _db.SaveChangesAsync();
    }

    public async Task<CourseImageDb> Get(Guid id)
    {
        return await _db.CourseImagesDb.FirstOrDefaultAsync(x => x.Id == id);
    }

    public IQueryable<CourseImageDb> GetAll()
    {
        return _db.CourseImagesDb;
    }

    public async Task<CourseImageDb> Update(CourseImageDb item)
    {
        _db.CourseImagesDb.Update(item);
        await _db.SaveChangesAsync();
        return item;
    }
}