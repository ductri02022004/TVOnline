using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using TVOnline.Models;
using TVOnline.Services;
using TVOnline.ViewModels;

namespace TVOnline.Controllers
{
    [Authorize]
    public class PremiumCVController : Controller
    {
        private readonly ICVTemplateService _cvTemplateService;
        private readonly IPremiumUserService _premiumUserService;
        private readonly UserManager<Users> _userManager;

        public PremiumCVController(
            ICVTemplateService cvTemplateService,
            IPremiumUserService premiumUserService,
            UserManager<Users> userManager)
        {
            _cvTemplateService = cvTemplateService;
            _premiumUserService = premiumUserService;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            // Kiểm tra xem người dùng có phải là Premium User không
            var isPremium = await _premiumUserService.IsUserPremium(user.Id);
            if (!isPremium)
            {
                return RedirectToAction("UpgradeAccount", "Account");
            }

            // Lấy danh sách các mẫu CV đang hoạt động
            var templates = await _cvTemplateService.GetActiveTemplatesAsync();
            
            return View(templates);
        }

        public async Task<IActionResult> Preview(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            // Kiểm tra xem người dùng có phải là Premium User không
            var isPremium = await _premiumUserService.IsUserPremium(user.Id);
            if (!isPremium)
            {
                return RedirectToAction("UpgradeAccount", "Account");
            }

            var template = await _cvTemplateService.GetTemplateByIdAsync(id);
            if (template == null)
            {
                return NotFound();
            }

            // Tạo view model chứa thông tin người dùng và mẫu CV
            var viewModel = new CVPreviewViewModel
            {
                Template = template,
                User = user
            };

            return View(viewModel);
        }

        public async Task<IActionResult> Create(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            // Kiểm tra xem người dùng có phải là Premium User không
            var isPremium = await _premiumUserService.IsUserPremium(user.Id);
            if (!isPremium)
            {
                return RedirectToAction("UpgradeAccount", "Account");
            }

            var template = await _cvTemplateService.GetTemplateByIdAsync(id);
            if (template == null)
            {
                return NotFound();
            }

            // Tạo view model cho form tạo CV
            var viewModel = new CVEditorViewModel
            {
                TemplateId = template.TemplateId,
                TemplateName = template.Name,
                HtmlContent = template.HtmlContent,
                CssContent = template.CssContent,
                FullName = user.FullName,
                Email = user.Email,
                Phone = user.PhoneNumber ?? "",
                Address = "",
                JobTitle = "",
                Summary = "",
                Education = "",
                Experience = "",
                Skills = "",
                Languages = "",
                Certificates = "",
                Projects = "",
                Interests = ""
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CVEditorViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return NotFound();
                }

                // Kiểm tra xem người dùng có phải là Premium User không
                var isPremium = await _premiumUserService.IsUserPremium(user.Id);
                if (!isPremium)
                {
                    return RedirectToAction("UpgradeAccount", "Account");
                }

                // Tạo CV mới và lưu vào cơ sở dữ liệu (có thể mở rộng chức năng này sau)
                
                // Hiển thị CV đã tạo
                return RedirectToAction(nameof(Preview), new { id = model.TemplateId });
            }

            return View(model);
        }

        public async Task<IActionResult> Download(string id)
        {
            // Chức năng tải xuống CV dưới dạng PDF (có thể triển khai sau)
            return RedirectToAction(nameof(Index));
        }
    }
}
