using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TVOnline.Data;
using TVOnline.Models;
using System;
using System.Threading.Tasks;

namespace TVOnline.Controllers
{
    public class FeedbackController : Controller
    {
        private readonly AppDbContext _context;

        public FeedbackController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Index(string employerId)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account");
            }

            ViewBag.EmployerId = employerId;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Submit(Feedbacks feedback)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account");
            }

            if (ModelState.IsValid)
            {
                feedback.Date = DateTime.Now;
                _context.Feedbacks.Add(feedback);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Cảm ơn bạn đã gửi phản hồi!";
                return RedirectToAction("AppliedJob", "Account");
            }

            return View("Index", feedback);
        }
    }
} 