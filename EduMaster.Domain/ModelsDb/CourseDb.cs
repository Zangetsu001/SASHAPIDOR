using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EduMaster.Domain.ModelsDb
{
    [Table("Courses")]
    public class CourseDb
    {
        [Key]
        public Guid Id { get; set; }

        public string title { get; set; } = string.Empty;

        public string description { get; set; } = string.Empty;

        public decimal price { get; set; }

        public bool is_active { get; set; } = true;

        public DateTime created_at { get; set; } = DateTime.UtcNow;

        public Guid category_id { get; set; }   // просто поле, без связей
    }
}
