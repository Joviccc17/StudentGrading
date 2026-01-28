using System;

namespace StudentGrading.Models
{
    public class ExamSubmission
    {
        public int Id { get; set; }
        public int ExamId { get; set; }
        public Exam? Exam { get; set; }

        public int StudentId { get; set; }
        public Student? Student { get; set; }
        public DateTime SubmittedAt { get; set; } = DateTime.Now;
    }
}

