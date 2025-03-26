using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;
using TVOnline.Data;
using TVOnline.ViewModels.Post;
using TVOnline.Models;

namespace TVOnline.Controllers.Employer
{
    [Authorize]
    [Route("Employer/Posts")]
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
        [Route("Create")]
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
        [Route("Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreatePostViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
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
                    Position = model.Position,
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

        [HttpGet]
        [Route("Edit/{id}")]
        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

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

            var post = await _context.Posts
                .Include(p => p.City)
                .FirstOrDefaultAsync(p => p.PostId == id && p.EmployerId == employer.EmployerId);

            if (post == null)
            {
                return NotFound();
            }

            var viewModel = new EditPostViewModel
            {
                PostId = post.PostId,
                Title = post.Title,
                Description = post.Description,
                Requirements = post.Requirements,
                Benefits = post.Benefits,
                Salary = post.Salary,
                Position = post.Position,
                JobType = post.JobType,
                Experience = post.Experience,
                CityId = post.CityId,
                IsActive = post.IsActive,
                Cities = await _context.Cities
                    .Select(c => new SelectListItem
                    {
                        Value = c.CityId.ToString(),
                        Text = c.CityName,
                        Selected = c.CityId == post.CityId
                    })
                    .ToListAsync(),
                JobTypes = new List<SelectListItem>
                {
                    new SelectListItem { Value = "Full-time", Text = "Toàn thời gian", Selected = post.JobType == "Full-time" },
                    new SelectListItem { Value = "Part-time", Text = "Bán thời gian", Selected = post.JobType == "Part-time" },
                    new SelectListItem { Value = "Freelance", Text = "Freelance", Selected = post.JobType == "Freelance" },
                    new SelectListItem { Value = "Intern", Text = "Thực tập sinh", Selected = post.JobType == "Intern" }
                },
                ExperienceLevels = new List<SelectListItem>
                {
                    new SelectListItem { Value = "Fresher", Text = "Mới tốt nghiệp", Selected = post.Experience == "Fresher" },
                    new SelectListItem { Value = "Junior", Text = "1-2 năm kinh nghiệm", Selected = post.Experience == "Junior" },
                    new SelectListItem { Value = "Middle", Text = "3-5 năm kinh nghiệm", Selected = post.Experience == "Middle" },
                    new SelectListItem { Value = "Senior", Text = "Trên 5 năm kinh nghiệm", Selected = post.Experience == "Senior" }
                }
            };

            return View(viewModel);
        }

        [HttpPost]
        [Route("Edit/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, EditPostViewModel model)
        {
            if (id != model.PostId)
            {
                return NotFound();
            }

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

            if (ModelState.IsValid)
            {
                try
                {
                    var post = await _context.Posts.FirstOrDefaultAsync(p => p.PostId == id && p.EmployerId == employer.EmployerId);
                    if (post == null)
                    {
                        return NotFound();
                    }

                    post.Title = model.Title;
                    post.Description = model.Description;
                    post.Requirements = model.Requirements;
                    post.Benefits = model.Benefits;
                    post.Salary = model.Salary;
                    post.Position = model.Position;
                    post.JobType = model.JobType;
                    post.Experience = model.Experience;
                    post.CityId = model.CityId;
                    post.IsActive = model.IsActive;
                    post.UpdatedAt = DateTime.Now;

                    _context.Update(post);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Cập nhật tin tuyển dụng thành công!";
                    return RedirectToAction("ManagePosts", "EmployerDashboard");
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    _logger.LogError($"Concurrency error updating post: {ex.Message}");
                    ModelState.AddModelError("", "Có lỗi xảy ra khi cập nhật. Vui lòng thử lại sau.");
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error updating post: {ex.Message}");
                    ModelState.AddModelError("", "Có lỗi xảy ra khi cập nhật. Vui lòng thử lại sau.");
                }
            }

            // Nếu có lỗi, load lại các danh sách
            model.Cities = await _context.Cities
                .Select(c => new SelectListItem
                {
                    Value = c.CityId.ToString(),
                    Text = c.CityName,
                    Selected = c.CityId == model.CityId
                })
                .ToListAsync();
            model.JobTypes = new List<SelectListItem>
            {
                new SelectListItem { Value = "Full-time", Text = "Toàn thời gian", Selected = model.JobType == "Full-time" },
                new SelectListItem { Value = "Part-time", Text = "Bán thời gian", Selected = model.JobType == "Part-time" },
                new SelectListItem { Value = "Freelance", Text = "Freelance", Selected = model.JobType == "Freelance" },
                new SelectListItem { Value = "Intern", Text = "Thực tập sinh", Selected = model.JobType == "Intern" }
            };
            model.ExperienceLevels = new List<SelectListItem>
            {
                new SelectListItem { Value = "Fresher", Text = "Mới tốt nghiệp", Selected = model.Experience == "Fresher" },
                new SelectListItem { Value = "Junior", Text = "1-2 năm kinh nghiệm", Selected = model.Experience == "Junior" },
                new SelectListItem { Value = "Middle", Text = "3-5 năm kinh nghiệm", Selected = model.Experience == "Middle" },
                new SelectListItem { Value = "Senior", Text = "Trên 5 năm kinh nghiệm", Selected = model.Experience == "Senior" }
            };

            return View(model);
        }

        [HttpGet]
        [Route("Details/{id}")]
        public async Task<IActionResult> Details(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

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

            var post = await _context.Posts
                .Include(p => p.City)
                .Include(p => p.Employer)
                .FirstOrDefaultAsync(p => p.PostId == id && p.EmployerId == employer.EmployerId);

            if (post == null)
            {
                return NotFound();
            }

            return View(post);
        }
    }
}
