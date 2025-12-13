using EduMaster.Domain.ModelsDb;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EduMaster.Services
{
    public interface ICartService
    {
        Task AddToCartAsync(Guid userId, Guid courseId);
        Task RemoveFromCartAsync(Guid userId, Guid enrollmentId);
        Task<List<EnrollmentDb>> GetUserCartAsync(Guid userId);
        Task PurchaseCartAsync(Guid userId);
    }
}