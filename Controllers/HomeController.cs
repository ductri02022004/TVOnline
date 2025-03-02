using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using TVOnline.Models;
using System.Collections.Generic;
using TVOnline.Models.Error;
using Microsoft.AspNetCore.Identity;
using TVOnline.Data;
using Microsoft.EntityFrameworkCore;
using TVOnline.ViewModels.Post;


namespace TVOnline.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly UserManager<Users> _userManager;
        private readonly AppDbContext _context;

        public HomeController(
            ILogger<HomeController> logger, 
            UserManager<Users> userManager, 
            AppDbContext context)
        {
            _logger = logger;
            _userManager = userManager;
            _context = context;
        }
        
        public async Task<IActionResult> Index()
        {
            var posts = await _context.Posts
                .Include(p => p.Employer)
                .Include(p => p.City)
                .OrderByDescending(p => p.CreatedAt)
                .Select(p => new PostListViewModel
                {
                    PostId = p.PostId.ToString(),
                    Title = p.Title,
                    CompanyName = p.Employer.CompanyName,
                    Location = p.City.CityName,
                    Salary = p.Salary,
                    JobType = p.JobType,
                    Experience = p.Experience,
                    CreatedAt = p.CreatedAt
                }).Take(6)
                .ToListAsync();
            return View(posts);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public async Task<IActionResult> BecomeEmployer()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account");
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Kiểm tra xem người dùng đã là nhà tuyển dụng chưa
            var existingEmployer = await _context.Employers.FirstOrDefaultAsync(e => e.UserId == user.Id);
            
            if (existingEmployer != null)
            {
                // Nếu đã là employer, chuyển thẳng đến dashboard
                return RedirectToAction("Index", "EmployerDashboard");
            }

            // Nếu chưa là employer, chuyển đến trang đăng ký
            return RedirectToAction("Register", "EmployerRegistration");
        }
    }
}
