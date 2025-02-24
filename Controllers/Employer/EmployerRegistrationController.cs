using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TVOnline.Data;
using TVOnline.Models;
using TVOnline.ViewModels.Employer;
using static TVOnline.Models.Location;
using Microsoft.Extensions.Logging;

namespace TVOnline.Controllers.Employer {
    [Authorize]
    public class EmployerRegistrationController : Controller {
        private readonly UserManager<Users> _userManager;
        private readonly AppDbContext _context;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ILogger<EmployerRegistrationController> _logger;

        public EmployerRegistrationController(
            UserManager<Users> userManager,
            AppDbContext context,
            RoleManager<IdentityRole> roleManager,
            ILogger<EmployerRegistrationController> logger) {
            _userManager = userManager;
            _context = context;
            _roleManager = roleManager;
            _logger = logger;
        }

        private async Task<List<SelectListItem>> GetCitiesListAsync() {
            var cities = await _context.Cities
                .Include(c => c.Zone)
                .OrderBy(c => c.Zone.ZoneName)
                .ThenBy(c => c.CityName)
                .Select(c => new SelectListItem {
                    Value = c.CityId.ToString(),
                    Text = $"{c.CityName} ({c.Zone.ZoneName})"
                })
                .ToListAsync();

            cities.Insert(0, new SelectListItem {
                Value = "",
                Text = "-- Chọn thành phố --",
                Selected = true
            });

            return cities;
        }

        [HttpGet]
        public async Task<IActionResult> Register() {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) {
                return RedirectToAction("Login", "Account");
            }

            // Kiểm tra cả trong bảng Employers và role của user
            var isEmployer = await _userManager.IsInRoleAsync(user, "Employer");
            var existingEmployer = await _context.Employers.FirstOrDefaultAsync(e => e.UserId == user.Id);
            
            if (isEmployer || existingEmployer != null) {
                return RedirectToAction("Index", "EmployerDashboard");
            }

            var viewModel = new RegisterEmployerViewModel {
                Cities = await GetCitiesListAsync()
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RegisterEmployer(RegisterEmployerViewModel model) {
            _logger.LogInformation("RegisterEmployer called with model: {@Model}", model);

            if (!ModelState.IsValid) {
                _logger.LogWarning("ModelState is invalid. Errors: {@Errors}", 
                    ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)));
                model.Cities = await GetCitiesListAsync();
                return View("Register", model);
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null) {
                _logger.LogWarning("User not found");
                ModelState.AddModelError("", "Không tìm thấy thông tin người dùng. Vui lòng đăng nhập lại.");
                model.Cities = await GetCitiesListAsync();
                return View("Register", model);
            }

            // Kiểm tra xem người dùng đã là nhà tuyển dụng chưa
            var isEmployer = await _userManager.IsInRoleAsync(user, "Employer");
            var existingEmployer = await _context.Employers.FirstOrDefaultAsync(e => e.UserId == user.Id);
            
            if (isEmployer || existingEmployer != null) {
                _logger.LogWarning("User is already an employer");
                return RedirectToAction("Index", "EmployerDashboard");
            }

            // Tạo mới nhà tuyển dụng
            var employer = new Employers {
                EmployerId = Guid.NewGuid().ToString(),
                UserId = user.Id,
                CompanyName = model.CompanyName,
                Email = model.Email,
                CityId = model.CityId,
                Description = model.Description,
                Field = model.Field,
                CreatedAt = DateTime.Now
            };

            try {
                // Thêm employer trước
                _context.Employers.Add(employer);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Employer added successfully");

                // Sau đó thêm role
                var roleResult = await _userManager.AddToRoleAsync(user, "Employer");
                if (!roleResult.Succeeded) {
                    // Nếu thêm role thất bại, xóa employer đã thêm
                    _context.Employers.Remove(employer);
                    await _context.SaveChangesAsync();

                    var errors = string.Join(", ", roleResult.Errors.Select(e => e.Description));
                    _logger.LogError("Failed to add employer role: {Errors}", errors);
                    ModelState.AddModelError("", $"Không thể gán quyền nhà tuyển dụng: {errors}");
                    model.Cities = await GetCitiesListAsync();
                    return View("Register", model);
                }

                _logger.LogInformation("Employer role added successfully");
                TempData["SuccessMessage"] = "Đăng ký trở thành nhà tuyển dụng thành công!";
                return RedirectToAction("Index", "EmployerDashboard");
            } catch (Exception ex) {
                // Nếu có lỗi, xóa cả employer và role
                _logger.LogError(ex, "Error occurred while registering employer");
                if (await _userManager.IsInRoleAsync(user, "Employer")) {
                    await _userManager.RemoveFromRoleAsync(user, "Employer");
                }
                if (existingEmployer != null) {
                    _context.Employers.Remove(employer);
                    await _context.SaveChangesAsync();
                }

                ModelState.AddModelError("", $"Có lỗi xảy ra: {ex.Message}");
                model.Cities = await GetCitiesListAsync();
                return View("Register", model);
            }
        }
    }
}
