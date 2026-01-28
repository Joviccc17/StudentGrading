using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using StudentGrading.Data;
using System;

namespace StudentGrading.Controllers
{
    public class ResultController : Controller
    {
        private readonly AppDb _context;

        public ResultController(AppDb context)
        {
            _context = context;
        }

        // GET: /Result/Statistics
        public IActionResult Statistics()
        {
            var results = _context.ExamResults
                .OrderByDescending(r => r.Score)
                .ToList();

            return View(results);
        }
    }
}

