using System.Collections.Generic;

namespace EduMaster.Domain.Models
{
    public class Category
    {
        public Guid Id { get; set; }
        public string Name { get; set; } 

        // Связь: В одной категории много курсов
        public List<Course> Course { get; set; } 
    }
}