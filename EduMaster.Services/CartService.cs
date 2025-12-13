using EduMaster.DAL;
using EduMaster.Domain.ModelsDb;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EduMaster.Services
{
    public class CartService : ICartService
    {
        private readonly ApplicationDbContext _db;

        public CartService(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task AddToCartAsync(Guid userId, Guid courseId)
        {
            // Проверяем, не добавлен ли курс уже
            var existing = await _db.EnrollmentDb
                .FirstOrDefaultAsync(e => e.UserId == userId && e.CourseId == courseId);

            if (existing == null)
            {
                var item = new EnrollmentDb
                {
                    UserId = userId,
                    CourseId = courseId,
                    Status = "InCart"
                };
                _db.EnrollmentDb.Add(item);
                await _db.SaveChangesAsync();
            }
        }

        public async Task RemoveFromCartAsync(Guid userId, Guid enrollmentId)
        {
            var item = await _db.EnrollmentDb
                .FirstOrDefaultAsync(e => e.Id == enrollmentId && e.UserId == userId);

            if (item != null)
            {
                _db.EnrollmentDb.Remove(item);
                await _db.SaveChangesAsync();
            }
        }

        public async Task<List<EnrollmentDb>> GetUserCartAsync(Guid userId)
        {
            return await _db.EnrollmentDb
                .Include(e => e.Course)
                .Where(e => e.UserId == userId && e.Status == "InCart")
                .ToListAsync();
        }

        public async Task PurchaseCartAsync(Guid userId)
        {
            var items = await _db.EnrollmentDb
                .Where(e => e.UserId == userId && e.Status == "InCart")
                .ToListAsync();

            foreach (var item in items)
            {
                item.Status = "Purchased";
            }
            await _db.SaveChangesAsync();
        }
    }
}