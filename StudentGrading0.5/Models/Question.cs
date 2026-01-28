using System.ComponentModel.DataAnnotations;

namespace StudentGrading.Models
{
    public class Question
    {
        public int Id { get; set; }

        [Required]
        public string Text { get; set; } = string.Empty;

        [Required]
        public string CorrectAnswer { get; set; } = string.Empty;

        public int Points { get; set; } 
        public int ExamId { get; set; }
        public Exam? Exam { get; set; }
    }
}

