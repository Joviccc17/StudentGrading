using Microsoft.VisualStudio.TestTools.UnitTesting;
using StudentGrading.Services;
using StudentGrading.Models;
using System.Collections.Generic;


namespace StudentGradingUnitTest
{
    [TestClass]
    public class GradingServiceTests
    {
        private GradingService _service = null!;

        [TestInitialize]
        public void Setup()
        {
            _service = new GradingService();
        }

        public void CalculateScore_WhenAnswerIsCorrect_AddsPoints()
        {
            var questions = new List<Question>
            {
                new Question { Id = 1, CorrectAnswer = "4", Points = 2 }
            };

            var answers = new Dictionary<int, string>
            {
                { 1, "4" }
            };

            int score = _service.CalculateScore(questions, answers);

            Assert.AreEqual(2, score);
        }

        public void CalculateScore_WhenAnswerIsWrong_AddsZeroPoints()
        {
            var questions = new List<Question>
            {
                new Question { Id = 1, CorrectAnswer = "4", Points = 2 }
            };

            var answers = new Dictionary<int, string>
            {
                { 1, "5" }
            };

            int score = _service.CalculateScore(questions, answers);

            Assert.AreEqual(0, score);

        }

        public void CalculateGrade_WhenPercentageIs90_Returns5()
        {
            double percentage = 90;

            int grade = _service.CalculateGrade(percentage);

            Assert.AreEqual(5, grade);
        }




    }

}


