using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;
using TVOnline.Data;
using TVOnline.Models;
using TVOnline.ViewModels.Post;

namespace TVOnline.Controllers.Employer
{
    [Authorize]
    public class PostController : Controller
    {
        private readonly UserManager<Users> _userManager;
        private readonly AppDbContext _context;
        private readonly ILogger<PostController> _logger;

        public PostController(
            UserManager<Users> userManager,
            AppDbContext context,
            ILogger<PostController> logger)
        {
            _userManager = userManager;
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var employer = await _context.Employers.FirstOrDefaultAsync(e => e.UserId == user.Id);
            if (employer == null)
            {
                return RedirectToAction("Register", "EmployerRegistration");
            }

            var viewModel = new CreatePostViewModel
            {
                Cities = await _context.Cities
                    .Select(c => new SelectListItem
                    {
                        Value = c.CityId.ToString(),
                        Text = c.CityName
                    })
                    .ToListAsync(),
                JobTypes = new List<SelectListItem>
                {
                    new SelectListItem { Value = "Full-time", Text = "Toàn thời gian" },
                    new SelectListItem { Value = "Part-time", Text = "Bán thời gian" },
                    new SelectListItem { Value = "Freelance", Text = "Freelance" },
                    new SelectListItem { Value = "Intern", Text = "Thực tập sinh" }
                },
                ExperienceLevels = new List<SelectListItem>
                {
                    new SelectListItem { Value = "Fresher", Text = "Mới tốt nghiệp" },
                    new SelectListItem { Value = "Junior", Text = "1-2 năm kinh nghiệm" },
                    new SelectListItem { Value = "Middle", Text = "3-5 năm kinh nghiệm" },
                    new SelectListItem { Value = "Senior", Text = "Trên 5 năm kinh nghiệm" }
                }
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreatePostViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Check if email is confirmed
            if (!user.EmailConfirmed)
            {
                TempData["ErrorMessage"] = "Bạn cần xác thực email trước khi thêm dữ liệu vào hệ thống.";
                return RedirectToAction("VerifyEmail", "Account");
            }

            if (ModelState.IsValid)
            {
                var employer = await _context.Employers.FirstOrDefaultAsync(e => e.UserId == user.Id);
                if (employer == null)
                {
                    return RedirectToAction("Register", "EmployerRegistration");
                }
                
                var post = new Post
                {
                    PostId = Guid.NewGuid().ToString(),
                    Title = model.Title,
                    Description = model.Description,
                    Requirements = model.Requirements,
                    Benefits = model.Benefits,
                    Salary = model.Salary,
                    JobType = model.JobType,
                    Experience = model.Experience,
                    CityId = model.CityId,
                    EmployerId = employer.EmployerId,
                    CreatedAt = DateTime.Now,
                };

                try
                {
                    _context.Posts.Add(post);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Đăng bài tuyển dụng thành công!";
                    return RedirectToAction("Index", "EmployerDashboard");
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error creating post: {ex.Message}");
                    ModelState.AddModelError("", "Có lỗi xảy ra khi đăng bài. Vui lòng thử lại sau.");
                }
            }

            // Nếu có lỗi, load lại các danh sách
            model.Cities = await _context.Cities
                .Select(c => new SelectListItem
                {
                    Value = c.CityId.ToString(),
                    Text = c.CityName
                })
                .ToListAsync();
            model.JobTypes = new List<SelectListItem>
            {
                new SelectListItem { Value = "Full-time", Text = "Toàn thời gian" },
                new SelectListItem { Value = "Part-time", Text = "Bán thời gian" },
                new SelectListItem { Value = "Freelance", Text = "Freelance" },
                new SelectListItem { Value = "Intern", Text = "Thực tập sinh" }
            };
            model.ExperienceLevels = new List<SelectListItem>
            {
                new SelectListItem { Value = "Fresher", Text = "Mới tốt nghiệp" },
                new SelectListItem { Value = "Junior", Text = "1-2 năm kinh nghiệm" },
                new SelectListItem { Value = "Middle", Text = "3-5 năm kinh nghiệm" },
                new SelectListItem { Value = "Senior", Text = "Trên 5 năm kinh nghiệm" }
            };

            return View(model);
        }
    }
}
