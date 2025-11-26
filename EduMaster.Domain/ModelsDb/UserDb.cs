using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EduMaster.Domain.ModelsDb
{
    [Table("User")]
    public class UserDb
    {
        [Column("id")]
        public Guid Id { get; set; }
        [Column("login")]
        public string Login { get; set; } 
        [Column("email")]
        public string Email { get; set; } 
        [Column("password_hash")]
        public string PasswordHash { get; set; } 
        [Column("role")]
        public string Role { get; set; } 
        [Column("created_at")]
        public DateTime CreatedAt { get; set; } 
    }
}
