using Microsoft.EntityFrameworkCore; // <--- Этот using исправляет ошибку
using System.Threading.Tasks;
using System.Linq;
using System;
using EduMaster.DAL; // Убедитесь, что это namespace вашего ApplicationDbContext
using EduMaster.DAL.Interfaces;
using EduMaster.Domain.ModelsDb; // Namespace ваших моделей

public class UserStorage : IBaseStorage<UserDb>
{
    public readonly ApplicationDbContext _db;

    public UserStorage(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task Add(UserDb item)
    {
        await _db.UsersDb.AddAsync(item);
        await _db.SaveChangesAsync();
    }

    public async Task Delete(UserDb item)
    {
        _db.UsersDb.Remove(item);
        await _db.SaveChangesAsync();
    }

    public async Task<UserDb> Get(Guid id)
    {
        return await _db.UsersDb.FirstOrDefaultAsync(x => x.Id == id);
    }

    public IQueryable<UserDb> GetAll()
    {
        return _db.UsersDb;
    }

    public async Task<UserDb> Update(UserDb item)
    {
        _db.UsersDb.Update(item);
        await _db.SaveChangesAsync();
        return item;
    }
}