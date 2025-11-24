using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EduMaster.Domain.ModelsDb
{
    [Table("Categories")]
    public class CategoryDb
    {
        [Key]
        public Guid Id { get; set; }

        public string name { get; set; } = string.Empty;
    }
}
