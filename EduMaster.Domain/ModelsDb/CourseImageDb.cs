using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EduMaster.Domain.ModelsDb
{
    [Table("CourseImages")]
    public class CourseImageDb
    {
        [Key]
        public Guid Id { get; set; }

        public string image_path { get; set; } = string.Empty;

        public bool is_cover { get; set; } = false;

        public Guid course_id { get; set; }   // просто поле
    }
}
