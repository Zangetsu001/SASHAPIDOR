using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace EduMaster.Domain.ModelsDb
{
    public class EnrollmentDb
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid UserId { get; set; }
        [ForeignKey("UserId")]
        public UserDb User { get; set; }

        public Guid CourseId { get; set; }
        [ForeignKey("CourseId")]
        public CourseDb Course { get; set; }

        public string Status { get; set; } // "InCart", "Purchased", "Archived"
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}