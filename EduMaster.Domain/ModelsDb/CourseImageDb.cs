using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EduMaster.Domain.ModelsDb
{
    [Table("CourseImage")]
    public class CourseImageDb
    {
        [Column("id")]
        public Guid Id { get; set; }
        [Column("image_path")]
        public string image_path { get; set; } 
        [Column("is_cover")]
        public bool is_cover { get; set; } 
        [Column("course_id")]
        public Guid course_id { get; set; }   // просто поле
    }
}
