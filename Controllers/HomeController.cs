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
    [Route("[controller]")]
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

        [Route("/")]
        public IActionResult Index() {
        
            // Tạo danh sách tạm thời các bài đăng
            var posts = new List<Post>
            {
                new()
                {
                    
                    PostId = 1,
                    Title = "Lập trình viên Backend",
                    Description = "Đây là bài đăng 1",
                    Benefits = "Lương thưởng hấp dẫn, môi trường làm việc chuyên nghiệp",
                    Salary = 15000000m,
                    Position = "Backend Developer",
                    Experience = "2 năm kinh nghiệm",
                    CityId = 1,
                    EmployerId = "101",
                    CreatedAt = DateTime.Now.AddDays(-1),
                    IsActive = true,
                    Requirements = "Thành thạo C# và .NET Core",
                    JobType = "Full-time"
                },
                new()
                {
                    PostId = 2,
                    Title = "Nhân viên hỗ trợ IT",
                    Description = "Mô tả bài đăng 2",
                    Benefits = "Bảo hiểm, phụ cấp, du lịch hàng năm",
                    Salary = 12000000m,
                    Position = "IT Support",
                    Experience = "1 năm kinh nghiệm",
                    CityId = 2,
                    EmployerId = "102",
                    CreatedAt = DateTime.Now.AddDays(-2),
                    IsActive = true,
                    Requirements = "Biết về phần cứng và hệ điều hành Windows",
                    JobType = "Full-time"
                },
                new()
                {
                    PostId = 3,
                    Title = "Lập trình viên Frontend",
                    Description = "Mô tả bài đăng 3",
                    Benefits = "Làm việc từ xa, thời gian linh hoạt",
                    Salary = 18000000m,
                    Position = "Frontend Developer",
                    Experience = "3 năm kinh nghiệm",
                    CityId = 3,
                    EmployerId = "103",
                    CreatedAt = DateTime.Now.AddDays(-3),
                    IsActive = true,
                    Requirements = "Thành thạo ReactJS, HTML, CSS",
                    JobType = "Remote"
                }

            };

//             var posts = await _context.Posts
//                 .Include(p => p.Employer)
//                 .Include(p => p.City)
//                 .OrderByDescending(p => p.CreatedAt)
//                 .Select(p => new PostListViewModel
//                 {
//                     PostId = p.PostId.ToString(),
//                     Title = p.Title,
//                     CompanyName = p.Employer.CompanyName,
//                     Location = p.City.CityName,
//                     Salary = p.Salary,
//                     JobType = p.JobType,
//                     Experience = p.Experience,
//                     CreatedAt = p.CreatedAt
//                 })
//                 .ToListAsync();
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
