using Microsoft.AspNetCore.Mvc;
using StudentGrading.Data;
using StudentGrading.Models;

namespace StudentGrading.Controllers
{
    public class HomeController : Controller
    {
        private readonly AppDb _context;

        public HomeController(AppDb context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            ViewBag.TotalExams = _context.Exams.Count();
            ViewBag.TotalResults = _context.ExamResults.Count();
            ViewBag.TotalUsers = _context.Users.Count();
            
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = System.Diagnostics.Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
