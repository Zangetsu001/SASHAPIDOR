using System;
using System.Collections.Generic;
using EduMaster.Domain.ModelsDb;

namespace EduMaster.Models.ViewModels
{
    public class CoursesIndexViewModel
    {
        public string Search { get; set; } = string.Empty;

        // Выбранная категория (ID из select'а)
        public Guid? SelectedCategoryId { get; set; }

        // Дополнительно — сама категория, если ты её где-то используешь (Model.SelectedCategory)
        public CategoryDb? SelectedCategory { get; set; }

        // Список всех категорий
        public List<CategoryDb> Categories { get; set; } = new();

        // Отфильтрованные курсы
        public List<CourseDb> Courses { get; set; } = new();
    }
}
