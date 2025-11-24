using System;
using System.Collections.Generic;

namespace EduMaster.Domain.Models
{
    public class Course
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Связь с категорией (Внешний ключ)
        public int CategoryId { get; set; }
        public Category? Category { get; set; }

        // Связь: У одного курса много картинок
        public List<CourseImage> Images { get; set; } = new List<CourseImage>();
    }
}