using System.ComponentModel.DataAnnotations;

namespace StudentGrading.Models
{
    public class User
    {
        public int Id { get; set; }

        [Required]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;

        // "Student" ili "Teacher"
        [Required]
        public string Role { get; set; } = string.Empty;
    }
}

