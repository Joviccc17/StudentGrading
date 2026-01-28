using System.ComponentModel.DataAnnotations;

namespace StudentGrading.Models
{
    public class Exam
    {
        public int Id { get; set; }

        [Required]
        public string Title { get; set; } = string.Empty;

        public int DurationMinutes { get; set; }
        public int MaxPoints { get; set; }

        public List<Question> Questions { get; set; } = new();
    }
}
