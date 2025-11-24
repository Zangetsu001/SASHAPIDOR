namespace EduMaster.Domain.Models
{
    public class CourseImage
    {
        public Guid Id { get; set; }
        public string ImagePath { get; set; } = string.Empty; // Ссылка на файл, например "/img/course1.jpg"
        public bool IsCover { get; set; } = false; // Является ли картинка главной обложкой?

        // Связь с курсом
        public int CourseId { get; set; }
        public Course? Course { get; set; }
    }
}