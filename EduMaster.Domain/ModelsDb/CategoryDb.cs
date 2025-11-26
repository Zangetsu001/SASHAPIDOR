using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EduMaster.Domain.ModelsDb
{
    [Table("Category")]
    public class CategoryDb
    {
        [Column("id")]
        public Guid Id { get; set; }
        [Column("name")]
        public string name { get; set; } 
    }
}
