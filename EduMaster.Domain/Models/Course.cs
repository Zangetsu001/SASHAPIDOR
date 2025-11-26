using System;
using System.Collections.Generic;

namespace EduMaster.Domain.Models
{
    public class Course
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; } 
        public decimal Price { get; set; }
        public bool IsActive { get; set; } 
        public DateTime CreatedAt { get; set; } 

        // Связь с категорией (Внешний ключ)
        public int CategoryId { get; set; }
        public Category? Category { get; set; }

        // Связь: У одного курса много картинок
        public List<CourseImage> Images { get; set; } 
    }
}