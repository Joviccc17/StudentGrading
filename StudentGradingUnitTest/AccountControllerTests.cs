using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudentGrading.Controllers;
using StudentGrading.Models;
using StudentGrading.Data;
using Microsoft.AspNetCore.Http;
using Moq;

namespace StudentGradingUnitTest
{
    [TestClass]
    public class AccountControllerTests
    {
        private AppDb _context = null!;
        private AccountController _controller = null!;

        [TestInitialize]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<AppDb>()
                .UseInMemoryDatabase(databaseName: "TestDatabase_" + Guid.NewGuid())
                .Options;

            _context = new AppDb(options);

            var mockHttpContext = new Mock<HttpContext>();
            var mockSession = new Mock<ISession>();

            byte[] sessionValue = new byte[0];
            mockSession.Setup(s => s.Set(It.IsAny<string>(), It.IsAny<byte[]>()))
                .Callback<string, byte[]>((key, value) => sessionValue = value);

            mockHttpContext.Setup(c => c.Session).Returns(mockSession.Object);

            _controller = new AccountController(_context)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = mockHttpContext.Object
                }
            };
        }

        [TestCleanup]
        public void Cleanup()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        [TestMethod]
        public void Register_GET_ReturnsViewResult()
        {
            var result = _controller.Register();

            Assert.IsInstanceOfType(result, typeof(ViewResult));
        }

        [TestMethod]
        public void Register_POST_WithValidUser_AddsUserToDatabase()
        {
            var user = new User
            {
                Email = "test@example.com",
                Password = "password123",
                Role = "Student"
            };

            var result = _controller.Register(user);

            var userInDb = _context.Users.FirstOrDefault(u => u.Email == "test@example.com");
            Assert.IsNotNull(userInDb);
            Assert.AreEqual("test@example.com", userInDb.Email);
            Assert.AreEqual("Student", userInDb.Role);
        }

        [TestMethod]
        public void Register_POST_WithValidUser_RedirectsToLogin()
        {
            var user = new User
            {
                Email = "test@example.com",
                Password = "password123",
                Role = "Student"
            };

            var result = _controller.Register(user) as RedirectToActionResult;

            Assert.IsNotNull(result);
            Assert.AreEqual("Login", result.ActionName);
        }

        [TestMethod]
        public void Register_POST_WithInvalidModel_ReturnsViewWithUser()
        {
            _controller.ModelState.AddModelError("Email", "Email is required");

            var user = new User
            {
                Password = "password123",
                Role = "Student"
            };

            var result = _controller.Register(user) as ViewResult;

            Assert.IsNotNull(result);
            Assert.AreEqual(user, result.Model);
        }

        [TestMethod]
        public void Login_GET_ReturnsViewResult()
        {
            var result = _controller.Login();

            Assert.IsInstanceOfType(result, typeof(ViewResult));
        }

        [TestMethod]
        public void Login_POST_WithValidCredentials_RedirectsToHome()
        {
            _context.Users.Add(new User
            {
                Email = "test@example.com",
                Password = "password123",
                Role = "Student"
            });
            _context.SaveChanges();

            var result = _controller.Login("test@example.com", "password123") as RedirectToActionResult;

            Assert.IsNotNull(result);
            Assert.AreEqual("Index", result.ActionName);
            Assert.AreEqual("Home", result.ControllerName);
        }

        [TestMethod]
        public void Login_POST_WithInvalidCredentials_ReturnsViewWithError()
        {
            _context.Users.Add(new User
            {
                Email = "test@example.com",
                Password = "password123",
                Role = "Student"
            });
            _context.SaveChanges();

            var result = _controller.Login("test@example.com", "wrongpassword") as ViewResult;

            Assert.IsNotNull(result);
            Assert.AreEqual("Neispravni podaci", _controller.ViewBag.Error);
        }

        [TestMethod]
        public void Login_POST_WithNonExistentUser_ReturnsViewWithError()
        {
            var result = _controller.Login("nonexistent@example.com", "password123") as ViewResult;

            Assert.IsNotNull(result);
            Assert.AreEqual("Neispravni podaci", _controller.ViewBag.Error);
        }

        [TestMethod]
        public void Login_POST_WithTeacherRole_RedirectsToHome()
        {
            _context.Users.Add(new User
            {
                Email = "teacher@example.com",
                Password = "password123",
                Role = "Teacher"
            });
            _context.SaveChanges();

            var result = _controller.Login("teacher@example.com", "password123") as RedirectToActionResult;

            Assert.IsNotNull(result);
            Assert.AreEqual("Index", result.ActionName);
            Assert.AreEqual("Home", result.ControllerName);
        }

        [TestMethod]
        public void Logout_ClearsSessionAndRedirectsToLogin()
        {
            var result = _controller.Logout() as RedirectToActionResult;

            Assert.IsNotNull(result);
            Assert.AreEqual("Login", result.ActionName);
        }

        [TestMethod]
        public void Register_POST_WithEmptyEmail_ReturnsViewWithUser()
        {
            _controller.ModelState.AddModelError("Email", "Email is required");

            var user = new User
            {
                Email = "",
                Password = "password123",
                Role = "Student"
            };

            var result = _controller.Register(user) as ViewResult;

            Assert.IsNotNull(result);
            Assert.IsFalse(_controller.ModelState.IsValid);
        }

        [TestMethod]
        public void Login_POST_WithEmptyEmail_ReturnsViewWithError()
        {
            var result = _controller.Login("", "password123") as ViewResult;

            Assert.IsNotNull(result);
            Assert.AreEqual("Neispravni podaci", _controller.ViewBag.Error);
        }

        [TestMethod]
        public void Login_POST_WithEmptyPassword_ReturnsViewWithError()
        {
            _context.Users.Add(new User
            {
                Email = "test@example.com",
                Password = "password123",
                Role = "Student"
            });
            _context.SaveChanges();

            var result = _controller.Login("test@example.com", "") as ViewResult;

            Assert.IsNotNull(result);
            Assert.AreEqual("Neispravni podaci", _controller.ViewBag.Error);
        }
    }
}
