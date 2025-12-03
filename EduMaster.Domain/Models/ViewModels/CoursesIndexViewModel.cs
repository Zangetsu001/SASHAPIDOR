using System;
using System.Collections.Generic;
using EduMaster.Domain.ModelsDb;

namespace EduMaster.Domain.Models.ViewModels // <--- ИСПРАВИЛ ТУТ
{
    public class CoursesIndexViewModel
    {
        public string Search { get; set; } = string.Empty;

        // Выбранная категория (ID из фильтра)
        public Guid? SelectedCategoryId { get; set; }

        // Сама категория (для заголовка)
        public CategoryDb? SelectedCategory { get; set; }

        // Список всех категорий
        public List<CategoryDb> Categories { get; set; } = new();

        // Список курсов
        public List<CourseDb> Courses { get; set; } = new();
    }
}