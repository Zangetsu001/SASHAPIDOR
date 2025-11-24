using System.Collections.Generic;

namespace EduMaster.Domain.Models
{
    public class Category
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty; // Например: "IT", "Marketing"

        // Связь: В одной категории много курсов
        public List<Course> Courses { get; set; } = new List<Course>();
    }
}