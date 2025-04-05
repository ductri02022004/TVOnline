using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TVOnline.Data;
using TVOnline.Models;

namespace TVOnline.Areas.Admin.Controllers {
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class PostManagementController : Controller {
        private readonly UserManager<Users> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly AppDbContext _context;

        public PostManagementController(
            UserManager<Users> userManager,
            RoleManager<IdentityRole> roleManager,
            AppDbContext context) {
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
        }
        public async Task<IActionResult> ManagePosts() {
            var posts = await _context.Posts
                .Include(p => p.Employer)
                .Include(p => p.City)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            return View(posts);
        }

        public async Task<IActionResult> PostDetails(string id) {
            if (string.IsNullOrEmpty(id)) {
                return NotFound();
            }

            var post = await _context.Posts
                .Include(p => p.Employer)
                .ThenInclude(e => e.User)
                .Include(p => p.City)
                .FirstOrDefaultAsync(p => p.PostId == id);

            if (post == null) {
                return NotFound();
            }

            return View(post);
        }

        [HttpPost]
        public async Task<IActionResult> TogglePostStatus(string id) {
            var post = await _context.Posts.FindAsync(id);
            if (post == null) {
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
        public async Task<IActionResult> DeletePost(string id) {
            var post = await _context.Posts.FindAsync(id);
            if (post == null) {
                return NotFound();
            }

            try {
                // Tìm tất cả các UserCV liên quan đến bài đăng này
                var relatedUserCVs = await _context.UserCVs
                    .Where(cv => cv.PostId == id)
                    .ToListAsync();

                // Cập nhật PostId thành null cho tất cả các UserCV liên quan
                foreach (var userCV in relatedUserCVs) {
                    userCV.PostId = null;
                }

                // Lưu thay đổi trước khi xóa bài đăng
                await _context.SaveChangesAsync();

                // Sau đó xóa bài đăng
                _context.Posts.Remove(post);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Đã xóa bài đăng thành công!";
                return RedirectToAction(nameof(ManagePosts));
            }
            catch (Exception ex) {
                TempData["ErrorMessage"] = $"Không thể xóa bài đăng: {ex.Message}";
                return RedirectToAction(nameof(ManagePosts));
            }
        }
    }
}
