using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using TVOnline.Areas.Admin.Models;
using TVOnline.Data;
using TVOnline.Models;
using Microsoft.AspNetCore.Authentication;

namespace TVOnline.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class AdminDashboardController : Controller {
        private readonly UserManager<Users> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly AppDbContext _context;

        public AdminDashboardController(
            UserManager<Users> userManager,
            RoleManager<IdentityRole> roleManager,
            AppDbContext context) {
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
        }

        public IActionResult Index() {
            // Trang dashboard cho admin
            var dashboardViewModel = new AdminDashboardViewModel {
                TotalUsers = _userManager.Users.Count(),
                TotalEmployers = _context.Employers.Count(),
                TotalJobSeekers = _userManager.GetUsersInRoleAsync("JobSeeker").Result.Count,
                TotalPosts = _context.Posts.Count(),
                TotalApplications = _context.InterviewInvitations.Count()
            };

            return View(dashboardViewModel);
        }




        public async Task<IActionResult> Logout() {
            await HttpContext.SignOutAsync(IdentityConstants.ApplicationScheme);
            return RedirectToAction("Login", "Account", new { area = "" });
        }
    }
        
}
