using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TVOnline.Data;
using TVOnline.Models;
using TVOnline.ViewModels.Employer;
using Microsoft.Extensions.Logging;

namespace TVOnline.Controllers.Employer {
    [Authorize]
    public class EmployerDashboardController : Controller {

        private readonly UserManager<Users> _userManager;
        private readonly AppDbContext _context;
        private readonly ILogger<EmployerDashboardController> _logger;

        public EmployerDashboardController(
            UserManager<Users> userManager,
            AppDbContext context,
            ILogger<EmployerDashboardController> logger)
        {
            _userManager = userManager;
            _context = context;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            _logger.LogInformation("Accessing EmployerDashboard Index");
            
            if (!User.Identity.IsAuthenticated)
            {
                _logger.LogWarning("User not authenticated");
                return RedirectToAction("Login", "Account");
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                _logger.LogWarning("User not found");
                return RedirectToAction("Login", "Account");
            }

            _logger.LogInformation($"User found: {user.Id}");

            // Kiểm tra quyền truy cập
            var isEmployer = await _userManager.IsInRoleAsync(user, "Employer");
            _logger.LogInformation($"User has Employer role: {isEmployer}");

            if (!isEmployer)
            {
                _logger.LogWarning($"User {user.Id} does not have Employer role");
                return RedirectToAction("Register", "EmployerRegistration");
            }

            var employer = await _context.Employers
                .Include(e => e.City)
                .ThenInclude(c => c.Zone)
                .FirstOrDefaultAsync(e => e.UserId == user.Id);

            _logger.LogInformation($"Employer record found: {employer != null}");

            if (employer == null)
            {
                _logger.LogWarning($"No employer record found for user {user.Id}");
                // Nếu có role nhưng không có trong bảng Employers, xóa role
                await _userManager.RemoveFromRoleAsync(user, "Employer");
                return RedirectToAction("Register", "EmployerRegistration");
            }

            try 
            {
                var viewModel = new EmployerDashboardViewModel
                {
                    CompanyName = employer.CompanyName,
                    Email = employer.Email,
                    City = employer.City?.CityName ?? "Unknown",
                    Zone = employer.City?.Zone?.ZoneName ?? "Unknown",
                    Field = employer.Field,
                    Description = employer.Description,
                    CreatedAt = employer.CreatedAt,
                    TotalInterviews = await _context.InterviewInvitations.CountAsync(i => i.EmployerId == employer.EmployerId),
                    TotalFeedbacks = await _context.Feedbacks.CountAsync(f => f.EmployerId == employer.EmployerId)
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error creating view model: {ex.Message}");
                throw;
            }
        }
    }
}
