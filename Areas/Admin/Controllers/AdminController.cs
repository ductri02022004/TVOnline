using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using TVOnline.Areas.Admin.Models;
using TVOnline.Data;
using TVOnline.Models;
using Microsoft.Extensions.Logging;

namespace TVOnline.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly UserManager<Users> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly AppDbContext _context;
        private readonly ILogger<AdminController> _logger;

        public AdminController(
            UserManager<Users> userManager,
            RoleManager<IdentityRole> roleManager,
            AppDbContext context,
            ILogger<AdminController> logger)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
            _logger = logger;
        }

        public IActionResult Index()
        {
            // Trang dashboard cho admin
            var dashboardViewModel = new AdminDashboardViewModel
            {
                TotalUsers = _userManager.Users.Count(),
                TotalEmployers = _context.Employers.Count(),
                TotalJobSeekers = _userManager.GetUsersInRoleAsync("JobSeeker").Result.Count,
                TotalPosts = _context.Posts.Count(),
                TotalApplications = _context.InterviewInvitations.Count()
            };

            return View(dashboardViewModel);
        }

        public async Task<IActionResult> ManageUsers()
        {
            var users = await _userManager.Users.ToListAsync();
            var userViewModels = new List<UserViewModel>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                userViewModels.Add(new UserViewModel
                {
                    Id = user.Id,
                    Email = user.Email,
                    UserName = user.UserName,
                    Roles = roles.ToList(),
                    EmailConfirmed = user.EmailConfirmed,
                    LockoutEnabled = user.LockoutEnabled,
                    LockoutEnd = user.LockoutEnd
                });
            }

            return View(userViewModels);
        }

        public async Task<IActionResult> UserDetails(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var roles = await _userManager.GetRolesAsync(user);
            var userViewModel = new UserViewModel
            {
                Id = user.Id,
                Email = user.Email,
                UserName = user.UserName,
                Roles = roles.ToList(),
                EmailConfirmed = user.EmailConfirmed,
                LockoutEnabled = user.LockoutEnabled,
                LockoutEnd = user.LockoutEnd
            };

            // Kiểm tra xem người dùng có phải là Employer không
            if (roles.Contains("Employer"))
            {
                var employer = await _context.Employers
                    .FirstOrDefaultAsync(e => e.UserId == user.Id);
                if (employer != null)
                {
                    userViewModel.EmployerDetails = new EmployerDetailsViewModel
                    {
                        CompanyName = employer.CompanyName,
                        Field = employer.Field,
                        Description = employer.Description,
                        CreatedAt = employer.CreatedAt
                    };
                }
            }

            // Kiểm tra xem người dùng có phải là JobSeeker không
            if (roles.Contains("JobSeeker"))
            {
                // Lấy thông tin từ bảng UserCVs thay vì JobSeekers
                var userCV = await _context.UserCVs
                    .FirstOrDefaultAsync(j => j.UserId == user.Id);
                if (userCV != null)
                {
                    userViewModel.JobSeekerDetails = new JobSeekerDetailsViewModel
                    {
                        FullName = user.UserName ?? "Chưa cập nhật",
                        Phone = user.PhoneNumber ?? "Chưa cập nhật",
                        Address = "Chưa cập nhật", // UserCV không có trường Address
                        CreatedAt = userCV.AppliedDate // Sử dụng AppliedDate thay vì CreatedAt
                    };
                }
                else
                {
                    userViewModel.JobSeekerDetails = new JobSeekerDetailsViewModel
                    {
                        FullName = user.UserName ?? "Chưa cập nhật",
                        Phone = user.PhoneNumber ?? "Chưa cập nhật",
                        Address = "Chưa cập nhật",
                        CreatedAt = DateTime.Now
                    };
                }
            }

            return View(userViewModel);
        }

        [HttpPost]
        public async Task<IActionResult> LockUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            // Kiểm tra xem người dùng có phải là admin không
            if (await _userManager.IsInRoleAsync(user, "Admin"))
            {
                TempData["ErrorMessage"] = "Không thể khóa tài khoản admin!";
                return RedirectToAction(nameof(ManageUsers));
            }

            // Khóa tài khoản trong 30 ngày
            var lockoutEndDate = DateTime.Now.AddDays(30);
            await _userManager.SetLockoutEnabledAsync(user, true);
            await _userManager.SetLockoutEndDateAsync(user, lockoutEndDate);

            TempData["SuccessMessage"] = "Đã khóa tài khoản người dùng thành công!";
            return RedirectToAction(nameof(ManageUsers));
        }

        [HttpPost]
        public async Task<IActionResult> UnlockUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            // Mở khóa tài khoản
            await _userManager.SetLockoutEnabledAsync(user, false);
            await _userManager.SetLockoutEndDateAsync(user, null);

            TempData["SuccessMessage"] = "Đã mở khóa tài khoản người dùng thành công!";
            return RedirectToAction(nameof(ManageUsers));
        }

        [HttpPost]
        public async Task<IActionResult> ConfirmEmail(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            // Xác nhận email
            user.EmailConfirmed = true;
            await _userManager.UpdateAsync(user);

            TempData["SuccessMessage"] = "Đã xác nhận email người dùng thành công!";
            return RedirectToAction(nameof(ManageUsers));
        }

        public async Task<IActionResult> ManagePosts()
        {
            var posts = await _context.Posts
                .Include(p => p.Employer)
                .Include(p => p.City)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            return View(posts);
        }

        [HttpPost]
        public async Task<IActionResult> TogglePostStatus(string id)
        {
            var post = await _context.Posts.FindAsync(id);
            if (post == null)
            {
                return NotFound();
            }

            // Đảo ngược trạng thái
            post.IsActive = !post.IsActive;
            post.UpdatedAt = DateTime.Now;

            _context.Update(post);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Đã {(post.IsActive ? "kích hoạt" : "vô hiệu hóa")} bài đăng thành công!";
            return RedirectToAction(nameof(ManagePosts));
        }

        [HttpPost]
        public async Task<IActionResult> DeletePost(string id)
        {
            try
            {
                var post = await _context.Posts.FindAsync(id);
                if (post == null)
                {
                    return NotFound();
                }

                // Xóa tất cả UserCV liên quan đến post này
                var userCVs = await _context.UserCVs.Where(ucv => ucv.PostId == id).ToListAsync();
                _context.UserCVs.RemoveRange(userCVs);

                // Sau đó xóa post
                _context.Posts.Remove(post);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                // Log lỗi
                _logger.LogError(ex, "Lỗi khi xóa post {PostId}", id);
                TempData["Error"] = "Không thể xóa bài đăng. Vui lòng thử lại sau.";
                return RedirectToAction(nameof(Index));
            }
        }
    }
}
