using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TVOnline.Models;
using System.Collections.Generic;
using TVOnline.Models.Error;


namespace TVOnline.Controllers
{
    [Route("[controller]")]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        [Route("[action]")]
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

            return View(posts);
        }

        [Authorize]
        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
