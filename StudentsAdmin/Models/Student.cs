using System.ComponentModel.DataAnnotations;

namespace StudentsAdmin.Models
{
    public class Student
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        [Required, EmailAddress]
        public string Email { get; set; }

        public string? PhotoPath { get; set; }
    }
}
