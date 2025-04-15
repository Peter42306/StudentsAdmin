using System.ComponentModel.DataAnnotations;

namespace StudentsAdmin.Models
{
    public class Student
    {
        public int Id { get; set; }
        
        public string Name { get; set; }

        [Required, EmailAddress]
        public string Email { get; set; }

        [Display(Name = "Photo")]
        public string? PhotoPath { get; set; }
    }
}
