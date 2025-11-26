using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EduMaster.Domain.ModelsDb
{
    [Table("Course")]
    public class CourseDb
    {
        [Column("id")]
        public Guid Id { get; set; }
        [Column("title")]
        public string title { get; set; } 
        [Column("description")]
        public string description { get; set; }
        [Column("price")]
        public decimal price { get; set; }
        [Column("is_active")]
        public bool is_active { get; set; } 
        [Column("created_at")]
        public DateTime created_at { get; set; } 
        [Column("category_id")]
        public Guid category_id { get; set; }   // просто поле, без связей
    }
}
