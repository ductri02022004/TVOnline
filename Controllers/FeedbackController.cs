using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TVOnline.Data;
using TVOnline.Models;
using System;
using System.Threading.Tasks;
using System.Security.Claims;

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

            if (string.IsNullOrEmpty(employerId))
            {
                TempData["ErrorMessage"] = "Không tìm thấy thông tin nhà tuyển dụng.";
                return RedirectToAction("AppliedJob", "Account");
            }

            ViewBag.EmployerId = employerId;
            return View(new Feedbacks());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Submit(Feedbacks feedback)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    feedback.Date = DateTime.Now;
                    feedback.UserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                    _context.Feedbacks.Add(feedback);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "Cảm ơn bạn đã gửi phản hồi!";
                    return RedirectToAction("AppliedJob", "Account");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Có lỗi xảy ra khi lưu phản hồi. Vui lòng thử lại sau.");
                }
            }

            ViewBag.EmployerId = feedback.EmployerId;
            return View("Index", feedback);
        }
    }
} 