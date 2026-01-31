using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudentGrading.Controllers;
using StudentGrading.Models;
using StudentGrading.Data;
using StudentGrading.Services;
using Microsoft.AspNetCore.Http;
using Moq;
using System.Text;

namespace StudentGradingUnitTest
{
    [TestClass]
    public class ExamControllerTests
    {
        private AppDb _context = null!;
        private ExamController _controller = null!;
        private GradingService _gradingService = null!;
        private Mock<ISession> _mockSession = null!;

        [TestInitialize]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<AppDb>()
                .UseInMemoryDatabase(databaseName: "TestDatabase_" + Guid.NewGuid())
                .Options;

            _context = new AppDb(options);
            _gradingService = new GradingService();

            var mockHttpContext = new Mock<HttpContext>();
            _mockSession = new Mock<ISession>();

            var sessionData = new Dictionary<string, byte[]>();

            _mockSession.Setup(s => s.Set(It.IsAny<string>(), It.IsAny<byte[]>()))
                .Callback<string, byte[]>((key, value) => sessionData[key] = value);

            _mockSession.Setup(s => s.TryGetValue(It.IsAny<string>(), out It.Ref<byte[]>.IsAny))
                .Returns((string key, out byte[] value) =>
                {
                    if (sessionData.ContainsKey(key))
                    {
                        value = sessionData[key];
                        return true;
                    }
                    value = null!;
                    return false;
                });

            mockHttpContext.Setup(c => c.Session).Returns(_mockSession.Object);

            _controller = new ExamController(_context, _gradingService)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = mockHttpContext.Object
                }
            };
        }

        private void SetSessionString(string key, string value)
        {
            _mockSession.Setup(s => s.TryGetValue(key, out It.Ref<byte[]>.IsAny))
                .Returns((string k, out byte[] v) =>
                {
                    v = Encoding.UTF8.GetBytes(value);
                    return true;
                });
        }

        private void SetSessionInt(string key, int value)
        {
            _mockSession.Setup(s => s.TryGetValue(key, out It.Ref<byte[]>.IsAny))
                .Returns((string k, out byte[] v) =>
                {
                    v = BitConverter.GetBytes(value);
                    return true;
                });
        }

        [TestCleanup]
        public void Cleanup()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        [TestMethod]
        public void List_ReturnsViewWithExams()
        {
            _context.Exams.AddRange(
                new Exam { Title = "Math Exam", DurationMinutes = 60, MaxPoints = 100 },
                new Exam { Title = "English Exam", DurationMinutes = 45, MaxPoints = 80 }
            );
            _context.SaveChanges();

            var result = _controller.List() as ViewResult;

            Assert.IsNotNull(result);
            var exams = result.Model as List<Exam>;
            Assert.IsNotNull(exams);
            Assert.AreEqual(2, exams.Count);
        }

        [TestMethod]
        public void List_WhenNoExams_ReturnsEmptyList()
        {
            var result = _controller.List() as ViewResult;

            Assert.IsNotNull(result);
            var exams = result.Model as List<Exam>;
            Assert.IsNotNull(exams);
            Assert.AreEqual(0, exams.Count);
        }

        [TestMethod]
        public void Create_GET_ReturnsViewResult()
        {
            var result = _controller.Create();

            Assert.IsInstanceOfType(result, typeof(ViewResult));
        }

        [TestMethod]
        public void Create_POST_WithValidExam_AddsExamToDatabase()
        {
            var exam = new Exam
            {
                Title = "Physics Exam",
                DurationMinutes = 90,
                MaxPoints = 100,
                Description = "Midterm exam"
            };

            var result = _controller.Create(exam);

            var examInDb = _context.Exams.FirstOrDefault(e => e.Title == "Physics Exam");
            Assert.IsNotNull(examInDb);
            Assert.AreEqual("Physics Exam", examInDb.Title);
            Assert.AreEqual(90, examInDb.DurationMinutes);
            Assert.AreEqual(100, examInDb.MaxPoints);
        }

        [TestMethod]
        public void Create_POST_WithValidExam_RedirectsToList()
        {
            var exam = new Exam
            {
                Title = "Chemistry Exam",
                DurationMinutes = 60,
                MaxPoints = 80
            };

            var result = _controller.Create(exam) as RedirectToActionResult;

            Assert.IsNotNull(result);
            Assert.AreEqual("List", result.ActionName);
        }

        [TestMethod]
        public void Create_POST_WithInvalidModel_ReturnsViewWithExam()
        {
            _controller.ModelState.AddModelError("Title", "Title is required");

            var exam = new Exam
            {
                DurationMinutes = 60,
                MaxPoints = 80
            };

            var result = _controller.Create(exam) as ViewResult;

            Assert.IsNotNull(result);
            Assert.AreEqual(exam, result.Model);
        }

        [TestMethod]
        public void Take_WithValidId_ReturnsViewWithExam()
        {
            var exam = new Exam
            {
                Title = "Biology Exam",
                DurationMinutes = 60,
                MaxPoints = 100,
                Questions = new List<Question>
                {
                    new Question { Text = "Question 1", CorrectAnswer = "Answer 1", Points = 10 },
                    new Question { Text = "Question 2", CorrectAnswer = "Answer 2", Points = 10 }
                }
            };
            _context.Exams.Add(exam);
            _context.SaveChanges();

            SetSessionString("UserRole", "Student");

            var result = _controller.Take(exam.Id) as ViewResult;

            Assert.IsNotNull(result);
            var returnedExam = result.Model as Exam;
            Assert.IsNotNull(returnedExam);
            Assert.AreEqual("Biology Exam", returnedExam.Title);
            Assert.AreEqual(2, returnedExam.Questions.Count);
        }

        [TestMethod]
        public void Take_WithInvalidId_ReturnsNotFound()
        {
            var result = _controller.Take(999);

            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }

        [TestMethod]
        public void Take_WhenUserIsTeacher_RedirectsToPreview()
        {
            var exam = new Exam
            {
                Title = "History Exam",
                DurationMinutes = 60,
                MaxPoints = 100
            };
            _context.Exams.Add(exam);
            _context.SaveChanges();

            SetSessionString("UserRole", "Teacher");

            var result = _controller.Take(exam.Id) as RedirectToActionResult;

            Assert.IsNotNull(result);
            Assert.AreEqual("Preview", result.ActionName);
        }

        [TestMethod]
        public void Preview_WithValidId_ReturnsViewWithExam()
        {
            var exam = new Exam
            {
                Title = "Geography Exam",
                DurationMinutes = 45,
                MaxPoints = 50,
                Questions = new List<Question>
                {
                    new Question { Text = "Capital of France?", CorrectAnswer = "Paris", Points = 5 }
                }
            };
            _context.Exams.Add(exam);
            _context.SaveChanges();

            SetSessionString("UserRole", "Teacher");

            var result = _controller.Preview(exam.Id) as ViewResult;

            Assert.IsNotNull(result);
            var returnedExam = result.Model as Exam;
            Assert.IsNotNull(returnedExam);
            Assert.AreEqual("Geography Exam", returnedExam.Title);
        }

        [TestMethod]
        public void Preview_WhenUserIsStudent_RedirectsToTake()
        {
            var exam = new Exam
            {
                Title = "Literature Exam",
                DurationMinutes = 60,
                MaxPoints = 100
            };
            _context.Exams.Add(exam);
            _context.SaveChanges();

            SetSessionString("UserRole", "Student");

            var result = _controller.Preview(exam.Id) as RedirectToActionResult;

            Assert.IsNotNull(result);
            Assert.AreEqual("Take", result.ActionName);
        }

        [TestMethod]
        public void Submit_WithValidAnswers_CreatesExamResult()
        {
            var exam = new Exam
            {
                Title = "Math Test",
                DurationMinutes = 60,
                MaxPoints = 10,
                Questions = new List<Question>
                {
                    new Question { Id = 1, Text = "2+2?", CorrectAnswer = "4", Points = 5 },
                    new Question { Id = 2, Text = "3+3?", CorrectAnswer = "6", Points = 5 }
                }
            };
            _context.Exams.Add(exam);
            _context.SaveChanges();

            SetSessionInt("UserId", 1);

            var answers = new Dictionary<int, string>
            {
                { 1, "4" },
                { 2, "6" }
            };

            var result = _controller.Submit(exam.Id, answers);

            var examResult = _context.ExamResults.FirstOrDefault(er => er.ExamId == exam.Id);
            Assert.IsNotNull(examResult);
            Assert.AreEqual(10, examResult.Score);
            Assert.AreEqual(100.0, examResult.Percentage);
            Assert.AreEqual(5, examResult.Grade);
        }

        [TestMethod]
        public void Submit_WithPartialCorrectAnswers_CalculatesCorrectScore()
        {
            var exam = new Exam
            {
                Title = "Science Quiz",
                DurationMinutes = 30,
                MaxPoints = 20,
                Questions = new List<Question>
                {
                    new Question { Id = 1, Text = "Water formula?", CorrectAnswer = "H2O", Points = 10 },
                    new Question { Id = 2, Text = "Gold symbol?", CorrectAnswer = "Au", Points = 10 }
                }
            };
            _context.Exams.Add(exam);
            _context.SaveChanges();

            SetSessionInt("UserId", 2);

            var answers = new Dictionary<int, string>
            {
                { 1, "H2O" },
                { 2, "Ag" }
            };

            var result = _controller.Submit(exam.Id, answers);

            var examResult = _context.ExamResults.FirstOrDefault(er => er.ExamId == exam.Id);
            Assert.IsNotNull(examResult);
            Assert.AreEqual(10, examResult.Score);
            Assert.AreEqual(50.0, examResult.Percentage);
            Assert.AreEqual(2, examResult.Grade);
        }

        [TestMethod]
        public void Submit_WithAllWrongAnswers_CreatesResultWithZeroScore()
        {
            var exam = new Exam
            {
                Title = "English Test",
                DurationMinutes = 45,
                MaxPoints = 15,
                Questions = new List<Question>
                {
                    new Question { Id = 1, Text = "Verb to be?", CorrectAnswer = "is", Points = 5 },
                    new Question { Id = 2, Text = "Past tense of go?", CorrectAnswer = "went", Points = 10 }
                }
            };
            _context.Exams.Add(exam);
            _context.SaveChanges();

            SetSessionInt("UserId", 3);

            var answers = new Dictionary<int, string>
            {
                { 1, "are" },
                { 2, "goed" }
            };

            var result = _controller.Submit(exam.Id, answers);

            var examResult = _context.ExamResults.FirstOrDefault(er => er.ExamId == exam.Id);
            Assert.IsNotNull(examResult);
            Assert.AreEqual(0, examResult.Score);
            Assert.AreEqual(0.0, examResult.Percentage);
            Assert.AreEqual(1, examResult.Grade);
        }

        [TestMethod]
        public void Submit_WithNoUserId_RedirectsToLogin()
        {
            var exam = new Exam
            {
                Title = "Test Exam",
                DurationMinutes = 60,
                MaxPoints = 100
            };
            _context.Exams.Add(exam);
            _context.SaveChanges();

            var answers = new Dictionary<int, string>();

            var result = _controller.Submit(exam.Id, answers) as RedirectToActionResult;

            Assert.IsNotNull(result);
            Assert.AreEqual("Login", result.ActionName);
            Assert.AreEqual("Account", result.ControllerName);
        }

        [TestMethod]
        public void Submit_WithInvalidExamId_ReturnsNotFound()
        {
            SetSessionInt("UserId", 1);

            var answers = new Dictionary<int, string>();

            var result = _controller.Submit(999, answers);

            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }

        [TestMethod]
        public void Submit_AfterSuccessfulSubmit_RedirectsToStatistics()
        {
            var exam = new Exam
            {
                Title = "Final Exam",
                DurationMinutes = 120,
                MaxPoints = 100,
                Questions = new List<Question>
                {
                    new Question { Id = 1, Text = "Question", CorrectAnswer = "Answer", Points = 100 }
                }
            };
            _context.Exams.Add(exam);
            _context.SaveChanges();

            SetSessionInt("UserId", 1);

            var answers = new Dictionary<int, string> { { 1, "Answer" } };

            var result = _controller.Submit(exam.Id, answers) as RedirectToActionResult;

            Assert.IsNotNull(result);
            Assert.AreEqual("Statistics", result.ActionName);
            Assert.AreEqual("Result", result.ControllerName);
        }

        [TestMethod]
        public void Create_POST_WithExamContainingQuestions_SavesExamAndQuestions()
        {
            var exam = new Exam
            {
                Title = "Complete Exam",
                DurationMinutes = 90,
                MaxPoints = 30,
                Questions = new List<Question>
                {
                    new Question { Text = "Q1", CorrectAnswer = "A1", Points = 10 },
                    new Question { Text = "Q2", CorrectAnswer = "A2", Points = 10 },
                    new Question { Text = "Q3", CorrectAnswer = "A3", Points = 10 }
                }
            };

            var result = _controller.Create(exam);

            var examInDb = _context.Exams
                .Include(e => e.Questions)
                .FirstOrDefault(e => e.Title == "Complete Exam");

            Assert.IsNotNull(examInDb);
            Assert.AreEqual(3, examInDb.Questions.Count);
            Assert.AreEqual(30, examInDb.MaxPoints);
        }

        [TestMethod]
        public void Submit_CalculatesPercentageCorrectly()
        {
            var exam = new Exam
            {
                Title = "Percentage Test",
                DurationMinutes = 60,
                MaxPoints = 40,
                Questions = new List<Question>
                {
                    new Question { Id = 1, Text = "Q1", CorrectAnswer = "A1", Points = 10 },
                    new Question { Id = 2, Text = "Q2", CorrectAnswer = "A2", Points = 10 },
                    new Question { Id = 3, Text = "Q3", CorrectAnswer = "A3", Points = 10 },
                    new Question { Id = 4, Text = "Q4", CorrectAnswer = "A4", Points = 10 }
                }
            };
            _context.Exams.Add(exam);
            _context.SaveChanges();

            SetSessionInt("UserId", 1);

            var answers = new Dictionary<int, string>
            {
                { 1, "A1" },
                { 2, "Wrong" },
                { 3, "A3" },
                { 4, "Wrong" }
            };

            var result = _controller.Submit(exam.Id, answers);

            var examResult = _context.ExamResults.FirstOrDefault(er => er.ExamId == exam.Id);
            Assert.IsNotNull(examResult);
            Assert.AreEqual(20, examResult.Score);
            Assert.AreEqual(50.0, examResult.Percentage);
            Assert.AreEqual(2, examResult.Grade);
        }
    }
}
