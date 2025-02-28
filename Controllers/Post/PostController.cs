using Microsoft.AspNetCore.Mvc;
using TVOnline.Models;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using System.IO;
using System;
using Microsoft.AspNetCore.Identity;
using TVOnline.Repository.UserCVs;
using TVOnline.Service.Post;

namespace TVOnline.Controllers
{
    [Route("[controller]")]
    public class PostController(IUserCvService userCvService, IPostService postService, UserManager<Users> userManager) : Controller
    {
        private readonly IUserCvService _userCvService = userCvService;
        private readonly IPostService _postService = postService;
        private readonly UserManager<Users> _userManager = userManager;

        List<Post> posts = [
            new()
    {
        PostId = 1,
        Title = "Lập trình viên Backend",
        Description = "Chúng tôi đang tìm kiếm một lập trình viên Backend có kinh nghiệm làm việc với C# và .NET Core để phát triển hệ thống quản lý doanh nghiệp.",
        Benefits = """
        - Lương thưởng hấp dẫn.
        - Môi trường làm việc chuyên nghiệp, năng động.
        - Cơ hội thăng tiến và phát triển bản thân.
        """,
        Salary = 15000000m,
        Position = "Backend Developer",
        Experience = "2 năm kinh nghiệm",
        CityId = 1,
        EmployerId = "101",
        CreatedAt = DateTime.Now.AddDays(-1),
        IsActive = true,
        Requirements = """
        - Thành thạo C# và .NET Core.
        - Có kinh nghiệm làm việc với SQL Server hoặc MySQL.
        - Hiểu về kiến trúc microservices và RESTful API.
        """,
        JobType = "Full-time"
    },

    new()
    {
        PostId = 2,
        Title = "Nhân viên hỗ trợ IT",
        Description = "Công ty cần tuyển nhân viên hỗ trợ IT có trách nhiệm cài đặt, bảo trì hệ thống máy tính, phần mềm và hỗ trợ nhân viên trong công ty.",
        Benefits = """
        - Bảo hiểm đầy đủ theo quy định nhà nước.
        - Phụ cấp ăn trưa và xăng xe.
        - Du lịch hàng năm cùng công ty.
        """,
        Salary = 12000000m,
        Position = "IT Support",
        Experience = "1 năm kinh nghiệm",
        CityId = 2,
        EmployerId = "102",
        CreatedAt = DateTime.Now.AddDays(-2),
        IsActive = true,
        Requirements = """
        - Biết về phần cứng và hệ điều hành Windows.
        - Có khả năng xử lý các sự cố phần mềm và mạng máy tính.
        - Giao tiếp tốt và có tinh thần hỗ trợ khách hàng.
        """,
        JobType = "Full-time"
    },

    new()
    {
        PostId = 3,
        Title = "Lập trình viên Frontend",
        Description = "Chúng tôi cần tuyển lập trình viên Frontend làm việc từ xa, yêu cầu có kinh nghiệm với ReactJS để xây dựng các giao diện web tương tác.",
        Benefits = """
        - Làm việc từ xa, thời gian linh hoạt.
        - Lương thưởng cạnh tranh.
        - Cơ hội làm việc với đội ngũ kỹ sư quốc tế.
        """,
        Salary = 18000000m,
        Position = "Frontend Developer",
        Experience = "3 năm kinh nghiệm",
        CityId = 3,
        EmployerId = "103",
        CreatedAt = DateTime.Now.AddDays(-3),
        IsActive = true,
        Requirements = """
        - Thành thạo ReactJS, HTML, CSS.
        - Có kinh nghiệm với Redux hoặc các thư viện state management khác.
        - Hiểu về responsive design và tối ưu hiệu suất frontend.
        """,
        JobType = "Remote"
    },

    new()
    {
        PostId = 4,
        Title = "Chuyên viên QA/QC",
        Description = "Tuyển chuyên viên QA/QC để kiểm thử phần mềm, đảm bảo chất lượng sản phẩm trước khi đưa vào sử dụng.",
        Benefits = """
        - Môi trường làm việc chuyên nghiệp, năng động.
        - Được đào tạo chuyên sâu về testing và automation.
        - Lương thưởng hấp dẫn, chế độ bảo hiểm đầy đủ.
        """,
        Salary = 14000000m,
        Position = "QA Engineer",
        Experience = "2 năm kinh nghiệm",
        CityId = 1,
        EmployerId = "104",
        CreatedAt = DateTime.Now.AddDays(-4),
        IsActive = true,
        Requirements = """
        - Có kinh nghiệm kiểm thử manual và automation.
        - Thành thạo các công cụ như Selenium, JMeter.
        - Hiểu về quy trình phát triển phần mềm và Agile/Scrum.
        """,
        JobType = "Full-time"
    },

    new()
    {
        PostId = 5,
        Title = "Lập trình viên Mobile",
        Description = "Chúng tôi cần một lập trình viên mobile chuyên về React Native hoặc Flutter để phát triển ứng dụng di động trên iOS và Android.",
        Benefits = """
        - Làm việc tại văn phòng hoặc từ xa tùy chọn.
        - Cơ hội làm việc với dự án startup sáng tạo.
        - Thưởng theo hiệu suất và đóng góp cho dự án.
        """,
        Salary = 20000000m,
        Position = "Mobile Developer",
        Experience = "2 năm kinh nghiệm",
        CityId = 2,
        EmployerId = "105",
        CreatedAt = DateTime.Now.AddDays(-5),
        IsActive = true,
        Requirements = """
        - Thành thạo React Native hoặc Flutter.
        - Có kinh nghiệm làm việc với Firebase hoặc GraphQL.
        - Khả năng tối ưu hiệu suất ứng dụng trên thiết bị di động.
        """,
        JobType = "Remote"
    },

    new()
    {
        PostId = 6,
        Title = "Data Analyst",
        Description = "Công ty đang tìm kiếm một chuyên gia phân tích dữ liệu để xử lý và trực quan hóa dữ liệu, hỗ trợ ra quyết định kinh doanh.",
        Benefits = """
        - Lương thưởng hấp dẫn.
        - Làm việc với hệ thống dữ liệu lớn, cơ hội học hỏi cao.
        - Hỗ trợ chi phí đào tạo và chứng chỉ chuyên môn.
        """,
        Salary = 22000000m,
        Position = "Data Analyst",
        Experience = "3 năm kinh nghiệm",
        CityId = 3,
        EmployerId = "106",
        CreatedAt = DateTime.Now.AddDays(-6),
        IsActive = true,
        Requirements = """
        - Có kinh nghiệm với SQL, Python, hoặc R.
        - Hiểu biết về Power BI hoặc Tableau.
        - Khả năng phân tích và trực quan hóa dữ liệu tốt.
        """,
        JobType = "Full-time"
    },

    new()
    {
        PostId = 7,
        Title = "DevOps Engineer",
        Description = "Công ty đang tìm kiếm DevOps Engineer để tối ưu hóa quy trình CI/CD và quản lý hạ tầng cloud.",
        Benefits = """
        - Làm việc với công nghệ cloud tiên tiến.
        - Thời gian làm việc linh hoạt.
        - Hỗ trợ thiết bị làm việc và các chi phí liên quan.
        """,
        Salary = 25000000m,
        Position = "DevOps Engineer",
        Experience = "3-5 năm kinh nghiệm",
        CityId = 1,
        EmployerId = "107",
        CreatedAt = DateTime.Now.AddDays(-7),
        IsActive = true,
        Requirements = """
        - Thành thạo Docker, Kubernetes, và CI/CD pipeline.
        - Có kinh nghiệm với AWS, Azure, hoặc GCP.
        - Hiểu biết về bảo mật hệ thống và quản lý hạ tầng.
        """,
        JobType = "Full-time"
    },

    new()
    {
        PostId = 8,
        Title = "Business Analyst",
        Description = "Công ty cần tuyển Business Analyst để thu thập và phân tích yêu cầu từ khách hàng, hỗ trợ phát triển phần mềm.",
        Benefits = """
        - Được làm việc với đội ngũ chuyên gia công nghệ.
        - Cơ hội thăng tiến lên vị trí quản lý.
        - Lương thưởng theo dự án và KPI.
        """,
        Salary = 18000000m,
        Position = "Business Analyst",
        Experience = "2-4 năm kinh nghiệm",
        CityId = 2,
        EmployerId = "108",
        CreatedAt = DateTime.Now.AddDays(-8),
        IsActive = true,
        Requirements = """
        - Có kinh nghiệm làm việc với khách hàng để thu thập yêu cầu.
        - Thành thạo kỹ năng phân tích hệ thống.
        - Có kiến thức về Agile/Scrum.
        """,
        JobType = "Full-time"
    },
    new()
    {
        PostId = 9,
        Title = "System Administrator",
        Description = "Công ty cần tuyển System Administrator để quản lý và duy trì hệ thống máy chủ, đảm bảo hoạt động ổn định của hệ thống IT.",
        Benefits = """
                   - Lương thưởng hấp dẫn, thưởng theo dự án.
                   - Được đào tạo và cập nhật công nghệ mới thường xuyên.
                   - Bảo hiểm sức khỏe toàn diện và các chế độ phúc lợi khác.
                   """,
        Salary = 20000000m,
        Position = "System Administrator",
        Experience = "3 năm kinh nghiệm",
        CityId = 3,
        EmployerId = "109",
        CreatedAt = DateTime.Now.AddDays(-9),
        IsActive = true,
        Requirements = """
                       - Có kinh nghiệm quản trị hệ thống Linux và Windows Server.
                       - Thành thạo các công cụ giám sát hệ thống như Zabbix, Prometheus.
                       - Hiểu biết về bảo mật hệ thống và mạng.
                       """,
        JobType = "Full-time"
    },
    new()
    {
        PostId = 10,
        Title = "DevOps Engineer",
        Description = "Chúng tôi đang tìm kiếm DevOps Engineer có kinh nghiệm để tối ưu hóa quy trình CI/CD và đảm bảo hệ thống vận hành ổn định.",
        Benefits = """
                   - Môi trường làm việc linh hoạt, có thể làm việc remote.
                   - Chế độ lương thưởng cạnh tranh, thưởng hiệu suất hàng quý.
                   - Cơ hội tiếp cận các công nghệ mới như Kubernetes, Docker, Terraform.
                   """,
        Salary = 25000000m,
        Position = "DevOps Engineer",
        Experience = "3-5 năm kinh nghiệm",
        CityId = 1,
        EmployerId = "110",
        CreatedAt = DateTime.Now.AddDays(-10),
        IsActive = true,
        Requirements = """
                       - Có kinh nghiệm với Docker, Kubernetes, CI/CD Pipelines.
                       - Thành thạo các công cụ quản lý cấu hình như Ansible, Terraform.
                       - Kiến thức vững về hệ thống Linux và bảo mật cloud.
                       """,
        JobType = "Remote"
    }


];

        [Route("[action]")]
        public IActionResult Index(int page = 1)
        {
            int pageSize = 5;
            int totalPosts = posts.Count();
            int totalPages = (int)Math.Ceiling((double)totalPosts / pageSize);

            var pagedPosts = posts.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalPosts = totalPosts;

            return View("Details", pagedPosts);
        }

        [Route("[action]/{postID}")]
        public IActionResult Details(string postID)
        {
            var post = posts.FirstOrDefault(p => p.PostId.ToString() == postID);
            if (post == null)
            {
                return NotFound();
            }
            return View("JobDetails", post);
        }

        [HttpPost]
        [Route("[action]/{postID}")]
        public async Task<IActionResult> Apply(IFormFile cvFile, string postId)
        {
            if (cvFile is { Length: > 0 })
            {
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads", cvFile.FileName);

                await using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await cvFile.CopyToAsync(stream);
                }

                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    TempData["ErrorMessage"] = "Bạn cần đăng nhập để ứng tuyển.";
                    return RedirectToAction("Details", new { postId = postId });
                }

                // Lưu thông tin ứng tuyển vào cơ sở dữ liệu hoặc xử lý thêm tại đây
                // Ví dụ: Lưu thông tin ứng viên và đường dẫn CV vào cơ sở dữ liệu
                var userCv = new UserCV
                {
                    CvFile = cvFile,
                    CVFileUrl = cvFile.FileName,
                    CVStatus = "Applied",
                    Users = user,
                    UserId = user.Id
                };

                await _userCvService.SaveCv(userCv);

                TempData["SuccessMessage"] = "Ứng tuyển thành công!";
                return RedirectToAction("Details", new { id = postId });
            }

            TempData["ErrorMessage"] = "Vui lòng tải lên CV của bạn.";
            return RedirectToAction("Details", new { id = postId });
        }
    }
}