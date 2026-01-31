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

        [TestMethod]
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

        [TestMethod]
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

        [TestMethod]
        public void CalculateGrade_WhenPercentageIs90_Returns5()
        {
            double percentage = 90;

            int grade = _service.CalculateGrade(percentage);

            Assert.AreEqual(5, grade);
        }

        [TestMethod]
        public void CalculateGrade_WhenPercentageIs75_Returns4()
        {
            double percentage = 75;

            int grade = _service.CalculateGrade(percentage);

            Assert.AreEqual(4, grade);
        }

        [TestMethod]
        public void CalculateGrade_WhenPercentageIs60_Returns3()
        {
            double percentage = 60;

            int grade = _service.CalculateGrade(percentage);

            Assert.AreEqual(3, grade);
        }

        [TestMethod]
        public void CalculateGrade_WhenPercentageIs50_Returns2()
        {
            double percentage = 50;

            int grade = _service.CalculateGrade(percentage);

            Assert.AreEqual(2, grade);
        }

        [TestMethod]
        public void CalculateGrade_WhenPercentageIsBelow50_Returns1()
        {
            double percentage = 49;

            int grade = _service.CalculateGrade(percentage);

            Assert.AreEqual(1, grade);
        }

        [TestMethod]
        public void CalculateScore_WithMultipleQuestions_AddsAllPoints()
        {
            var questions = new List<Question>
            {
                new Question { Id = 1, CorrectAnswer = "4", Points = 2 },
                new Question { Id = 2, CorrectAnswer = "Paris", Points = 3 },
                new Question { Id = 3, CorrectAnswer = "Blue", Points = 1 }
            };

            var answers = new Dictionary<int, string>
            {
                { 1, "4" },
                { 2, "Paris" },
                { 3, "Blue" }
            };

            int score = _service.CalculateScore(questions, answers);

            Assert.AreEqual(6, score);
        }

        [TestMethod]
        public void CalculateScore_WithMixedAnswers_AddsOnlyCorrectPoints()
        {
            var questions = new List<Question>
            {
                new Question { Id = 1, CorrectAnswer = "4", Points = 2 },
                new Question { Id = 2, CorrectAnswer = "Paris", Points = 3 },
                new Question { Id = 3, CorrectAnswer = "Blue", Points = 1 }
            };

            var answers = new Dictionary<int, string>
            {
                { 1, "4" },
                { 2, "London" },
                { 3, "Blue" }
            };

            int score = _service.CalculateScore(questions, answers);

            Assert.AreEqual(3, score);
        }

        [TestMethod]
        public void CalculateScore_WhenAnswerHasWhitespace_TrimsAndCompares()
        {
            var questions = new List<Question>
            {
                new Question { Id = 1, CorrectAnswer = "4", Points = 2 }
            };

            var answers = new Dictionary<int, string>
            {
                { 1, "  4  " }
            };

            int score = _service.CalculateScore(questions, answers);

            Assert.AreEqual(2, score);
        }

        [TestMethod]
        public void CalculateScore_WhenAnswerHasDifferentCase_ComparesIgnoringCase()
        {
            var questions = new List<Question>
            {
                new Question { Id = 1, CorrectAnswer = "Paris", Points = 3 }
            };

            var answers = new Dictionary<int, string>
            {
                { 1, "PARIS" }
            };

            int score = _service.CalculateScore(questions, answers);

            Assert.AreEqual(3, score);
        }

        [TestMethod]
        public void CalculateScore_WhenNoAnswersProvided_ReturnsZero()
        {
            var questions = new List<Question>
            {
                new Question { Id = 1, CorrectAnswer = "4", Points = 2 }
            };

            var answers = new Dictionary<int, string>();

            int score = _service.CalculateScore(questions, answers);

            Assert.AreEqual(0, score);
        }
    }
}


