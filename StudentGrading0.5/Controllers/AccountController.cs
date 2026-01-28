using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using StudentGrading.Models;
using StudentGrading.Data;
using System;

namespace StudentGrading.Controllers
{
    public class AccountController : Controller
    {
        private readonly AppDb _context;

        public AccountController(AppDb context)
        {
            _context = context;
        }        
        public IActionResult Register()
        {
            return View();
        }
        
        [HttpPost]
        public IActionResult Register(User user)
        {
            if (!ModelState.IsValid)
                return View(user);

            
            _context.Users.Add(user);
            _context.SaveChanges();

            return RedirectToAction("Login");
        }
               
        public IActionResult Login()
        {
            return View();
        }
                
        [HttpPost]
        public IActionResult Login(string email, string password)
        {
            var user = _context.Users
                .FirstOrDefault(u => u.Email == email && u.Password == password);

            if (user == null)
            {
                ViewBag.Error = "Neispravni podaci";
                return View();
            }
            
            HttpContext.Session.SetInt32("UserId", user.Id);
            HttpContext.Session.SetString("UserRole", user.Role);

            return RedirectToAction("Index", "Home");
        }
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }
    }
}
