using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TVOnline.Data;
using TVOnline.Models;
using TVOnline.Areas.Admin.Models;

namespace TVOnline.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class CVManagementController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<Users> _userManager;

        public CVManagementController(AppDbContext context, UserManager<Users> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // Hiển thị danh sách người dùng
        public async Task<IActionResult> Index()
        {
            var users = await _userManager.Users
                .Select(u => new UserCVViewModel
                {
                    Id = u.Id,
                    UserName = u.UserName,
                    Email = u.Email,
                    CVCount = _context.UserCVs.Count(cv => cv.UserId == u.Id)
                })
                .ToListAsync();

            return View(users);
        }

        // Hiển thị danh sách CV của một người dùng
        public async Task<IActionResult> UserCVs(string userId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                return NotFound();
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound();
            }

            var userCVs = await _context.UserCVs
                .Include(cv => cv.Post)
                .Where(cv => cv.UserId == userId)
                .Select(cv => new UserCVDetailViewModel
                {
                    CvID = cv.CvID,
                    CVFileUrl = cv.CVFileUrl,
                    CVStatus = cv.CVStatus,
                    ApplicationDate = cv.ApplicationDate,
                    AppliedDate = cv.AppliedDate,
                    PostTitle = cv.Post != null ? cv.Post.Title : "N/A"
                })
                .ToListAsync();

            ViewBag.UserName = user.UserName;
            ViewBag.UserId = userId;

            return View(userCVs);
        }

        // Hiển thị chi tiết CV
        public async Task<IActionResult> CVDetails(string cvId)
        {
            if (string.IsNullOrEmpty(cvId))
            {
                return NotFound();
            }

            var cv = await _context.UserCVs
                .Include(cv => cv.Users)
                .Include(cv => cv.Post)
                .FirstOrDefaultAsync(cv => cv.CvID == cvId);

            if (cv == null)
            {
                return NotFound();
            }

            var cvViewModel = new CVDetailsViewModel
            {
                CvID = cv.CvID,
                UserName = cv.Users?.UserName,
                UserEmail = cv.Users?.Email,
                CVFileUrl = cv.CVFileUrl,
                CVStatus = cv.CVStatus,
                ApplicationDate = cv.ApplicationDate,
                AppliedDate = cv.AppliedDate,
                PostTitle = cv.Post?.Title,
                PostCompany = cv.Post?.Employer?.CompanyName,
                EmployerNotes = cv.EmployerNotes
            };

            return View(cvViewModel);
        }

        // Cập nhật trạng thái CV
        [HttpPost]
        public async Task<IActionResult> UpdateCVStatus(string cvId, string status, string notes)
        {
            if (string.IsNullOrEmpty(cvId))
            {
                return NotFound();
            }

            var cv = await _context.UserCVs.FindAsync(cvId);
            if (cv == null)
            {
                return NotFound();
            }

            cv.CVStatus = status;
            cv.EmployerNotes = notes;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Cập nhật trạng thái CV thành công!";
            return RedirectToAction(nameof(CVDetails), new { cvId = cvId });
        }
    }
}
