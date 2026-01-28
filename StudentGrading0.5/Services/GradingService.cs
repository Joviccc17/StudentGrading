using StudentGrading.Models;

namespace StudentGrading.Services
{
    public class GradingService
    {
        public int CalculateScore(List<Question> questions, Dictionary<int, string> answers)
        {
            int score = 0;

            foreach (var q in questions)
            {
                if (answers.TryGetValue(q.Id, out var givenAnswer))
                {                    
                    if (string.Equals(givenAnswer?.Trim(), q.CorrectAnswer.Trim(), StringComparison.OrdinalIgnoreCase))
                    {
                        score += q.Points;
                    }
                }
            }

            return score;
        }

        public int CalculateGrade(double percentage)
        {            
            if (percentage >= 90) return 5;
            if (percentage >= 75) return 4;
            if (percentage >= 60) return 3;
            if (percentage >= 50) return 2;
            return 1;
        }
    }
}

