using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using TVOnline.Data;
using TVOnline.Models;
using TVOnline.Services;

namespace TVOnline.Areas.Premium.Controllers
{
    [Area("Premium")]
    [Authorize(Policy = "PremiumUser")]
    public class CVController : Controller
    {
        private readonly ICVTemplateService _cvTemplateService;
        private readonly AppDbContext _context;

        public CVController(ICVTemplateService cvTemplateService, AppDbContext context)
        {
            _cvTemplateService = cvTemplateService;
            _context = context;
        }

        // Hiển thị danh sách tất cả các mẫu CV có sẵn cho người dùng Premium
        public async Task<IActionResult> Index()
        {
            // Kiểm tra người dùng có quyền Premium hay không
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var isPremium = await IsPremiumUser(userId);

            if (!isPremium)
            {
                return RedirectToAction("Index", "Home", new { area = "" });
            }

            // Lấy danh sách các mẫu CV đang hoạt động
            var templates = await _cvTemplateService.GetActiveTemplatesAsync();
            return View(templates);
        }

        // Xem trước mẫu CV
        public async Task<IActionResult> Preview(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            // Kiểm tra người dùng có quyền Premium hay không
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var isPremium = await IsPremiumUser(userId);

            if (!isPremium)
            {
                return RedirectToAction("Index", "Home", new { area = "" });
            }

            var template = await _cvTemplateService.GetTemplateByIdAsync(id);

            if (template == null || !template.IsActive)
            {
                return NotFound();
            }

            return View(template);
        }

        // Tạo CV từ mẫu
        public async Task<IActionResult> Create(string templateId)
        {
            if (string.IsNullOrEmpty(templateId))
            {
                return NotFound();
            }

            // Kiểm tra người dùng có quyền Premium hay không
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var isPremium = await IsPremiumUser(userId);

            if (!isPremium)
            {
                return RedirectToAction("Index", "Home", new { area = "" });
            }

            var template = await _cvTemplateService.GetTemplateByIdAsync(templateId);

            if (template == null || !template.IsActive)
            {
                return NotFound();
            }

            // Tạo model cho view
            var model = new CVViewModel
            {
                TemplateId = template.TemplateId,
                TemplateName = template.Name,
                HtmlContent = template.HtmlContent,
                CssContent = template.CssContent
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CVViewModel model)
        {
            // Kiểm tra người dùng có quyền Premium hay không
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var isPremium = await IsPremiumUser(userId);

            if (!isPremium)
            {
                return RedirectToAction("Index", "Home", new { area = "" });
            }

            if (ModelState.IsValid)
            {
                // Lấy thông tin mẫu CV
                var template = await _cvTemplateService.GetTemplateByIdAsync(model.TemplateId);
                if (template == null || !template.IsActive)
                {
                    return NotFound();
                }

                // Tạo CV mới
                var cv = new PremiumUserCV
                {
                    UserId = userId,
                    TemplateId = model.TemplateId,
                    Title = model.Title,
                    Content = model.Content,
                    HtmlContent = template.HtmlContent,
                    CssContent = template.CssContent,
                    CreatedAt = DateTime.Now
                };

                // Lưu CV vào database
                _context.PremiumUserCVs.Add(cv);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "CV của bạn đã được tạo thành công!";
                return RedirectToAction(nameof(MyCV));
            }

            return View(model);
        }

        // Danh sách CV của người dùng
        public async Task<IActionResult> MyCV()
        {
            // Kiểm tra người dùng có quyền Premium hay không
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var isPremium = await IsPremiumUser(userId);

            if (!isPremium)
            {
                return RedirectToAction("Index", "Home", new { area = "" });
            }

            // Lấy danh sách CV của người dùng
            var userCVs = await _context.PremiumUserCVs
                .Where(cv => cv.UserId == userId)
                .OrderByDescending(cv => cv.CreatedAt)
                .ToListAsync();

            return View(userCVs);
        }

        // Xem CV
        public async Task<IActionResult> View(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            // Kiểm tra người dùng có quyền Premium hay không
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var isPremium = await IsPremiumUser(userId);

            if (!isPremium)
            {
                return RedirectToAction("Index", "Home", new { area = "" });
            }

            // Lấy thông tin CV
            var cv = await _context.PremiumUserCVs.FindAsync(id);

            if (cv == null || cv.UserId != userId)
            {
                return NotFound();
            }

            return View(cv);
        }

        // Xóa CV
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            // Kiểm tra người dùng có quyền Premium hay không
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var isPremium = await IsPremiumUser(userId);

            if (!isPremium)
            {
                return RedirectToAction("Index", "Home", new { area = "" });
            }

            // Lấy thông tin CV
            var cv = await _context.PremiumUserCVs.FindAsync(id);

            if (cv == null || cv.UserId != userId)
            {
                return NotFound();
            }

            // Xóa CV
            _context.PremiumUserCVs.Remove(cv);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "CV đã được xóa thành công!";
            return RedirectToAction(nameof(MyCV));
        }

        // Kiểm tra người dùng có quyền Premium hay không
        private async Task<bool> IsPremiumUser(string userId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                return false;
            }

            // Kiểm tra trong bảng AccountStatus
            var accountStatus = await _context.AccountStatuses
                .Where(a => a.UserId == userId && a.IsPremium && a.EndDate > DateTime.Now)
                .FirstOrDefaultAsync();

            return accountStatus != null;
        }
    }

    // ViewModel cho CV
    public class CVViewModel
    {
        public string TemplateId { get; set; }
        public string TemplateName { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public string HtmlContent { get; set; }
        public string CssContent { get; set; }
    }
}
