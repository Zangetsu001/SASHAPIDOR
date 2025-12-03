using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EduMaster.Domain.ModelsDb
{
    [Table("CourseImage")]
    public class CourseImageDb
    {
        [Column("id")]
        [Key]
        public Guid Id { get; set; }

        [Column("course_id")]
        public Guid CourseId { get; set; }

        [ForeignKey("CourseId")]
        public virtual CourseDb? Course { get; set; }

        // === ПОЛЯ ДЛЯ ХРАНЕНИЯ ФАЙЛА ===
        [Column("data")]
        public byte[]? Data { get; set; }

        [Column("content_type")]
        public string? ContentType { get; set; }
    }
}