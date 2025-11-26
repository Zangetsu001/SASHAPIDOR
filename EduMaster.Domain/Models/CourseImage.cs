namespace EduMaster.Domain.Models
{
    public class CourseImage
    {
        public Guid Id { get; set; }
        public string ImagePath { get; set; } 
        public bool IsCover { get; set; } 

        // Связь с курсом
        public int CourseId { get; set; }
        public Course? Course { get; set; }
    }
}