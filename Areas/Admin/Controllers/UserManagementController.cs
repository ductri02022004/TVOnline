using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TVOnline.Areas.Admin.Models;
using TVOnline.Data;
using TVOnline.Models;

namespace TVOnline.Areas.Admin.Controllers {
    [Area("Admin")]
    [Authorize(Roles = "Admin")]

    public class UserManagementController : Controller {
        private readonly UserManager<Users> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly AppDbContext _context;

        public UserManagementController(UserManager<Users> userManager,
                                        RoleManager<IdentityRole> roleManager,
                                        AppDbContext context) {
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
        }
        public async Task<IActionResult> ManageUsers() {
            var users = await _userManager.Users.ToListAsync();
            var userViewModels = new List<UserViewModel>();

            foreach (var user in users) {
                var roles = await _userManager.GetRolesAsync(user);
                userViewModels.Add(new UserViewModel {
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

        public async Task<IActionResult> UserDetails(string id) {
            if (string.IsNullOrEmpty(id)) {
                return NotFound();
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null) {
                return NotFound();
            }

            var roles = await _userManager.GetRolesAsync(user);
            var userViewModel = new UserViewModel {
                Id = user.Id,
                Email = user.Email,
                UserName = user.UserName,
                Roles = roles.ToList(),
                EmailConfirmed = user.EmailConfirmed,
                LockoutEnabled = user.LockoutEnabled,
                LockoutEnd = user.LockoutEnd
            };

            // Kiểm tra xem người dùng có phải là Employer không
            if (roles.Contains("Employer")) {
                var employer = await _context.Employers
                    .FirstOrDefaultAsync(e => e.UserId == user.Id);
                if (employer != null) {
                    userViewModel.EmployerDetails = new EmployerDetailsViewModel {
                        CompanyName = employer.CompanyName,
                        Field = employer.Field,
                        Description = employer.Description,
                        CreatedAt = employer.CreatedAt
                    };
                }
            }

            // Kiểm tra xem người dùng có phải là JobSeeker không
            if (roles.Contains("JobSeeker")) {
                // Lấy thông tin từ bảng UserCVs thay vì JobSeekers
                var userCV = await _context.UserCVs
                    .FirstOrDefaultAsync(j => j.UserId == user.Id);
                if (userCV != null) {
                    userViewModel.JobSeekerDetails = new JobSeekerDetailsViewModel {
                        FullName = user.UserName ?? "Chưa cập nhật",
                        Phone = user.PhoneNumber ?? "Chưa cập nhật",
                        Address = "Chưa cập nhật", // UserCV không có trường Address
                        CreatedAt = userCV.AppliedDate // Sử dụng AppliedDate thay vì CreatedAt
                    };
                } else {
                    userViewModel.JobSeekerDetails = new JobSeekerDetailsViewModel {
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
        public async Task<IActionResult> LockUser(string id) {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) {
                return NotFound();
            }

            // Kiểm tra xem người dùng có phải là admin không
            if (await _userManager.IsInRoleAsync(user, "Admin")) {
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
        public async Task<IActionResult> UnlockUser(string id) {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) {
                return NotFound();
            }

            // Mở khóa tài khoản
            await _userManager.SetLockoutEnabledAsync(user, false);
            await _userManager.SetLockoutEndDateAsync(user, null);

            TempData["SuccessMessage"] = "Đã mở khóa tài khoản người dùng thành công!";
            return RedirectToAction(nameof(ManageUsers));
        }

        [HttpPost]
        public async Task<IActionResult> ConfirmEmail(string id) {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) {
                return NotFound();
            }

            // Xác nhận email 
            user.EmailConfirmed = true;
            await _userManager.UpdateAsync(user);

            TempData["SuccessMessage"] = "Đã xác nhận email người dùng thành công!";
            return RedirectToAction(nameof(ManageUsers));
        }
        
        // Thêm action để hiển thị trang quản lý quyền
        public async Task<IActionResult> ManageRoles(string id)
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

            // Lấy tất cả các quyền trong hệ thống
            var roles = await _roleManager.Roles.ToListAsync();
            
            // Lấy các quyền hiện tại của người dùng
            var userRoles = await _userManager.GetRolesAsync(user);
            
            var model = new UserRolesViewModel
            {
                UserId = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                Roles = roles.Select(r => new RoleViewModel
                {
                    RoleId = r.Id,
                    RoleName = r.Name,
                    IsSelected = userRoles.Contains(r.Name)
                }).ToList()
            };
            
            return View(model);
        }
        
        // Thêm action để cập nhật quyền của người dùng
        [HttpPost]
        public async Task<IActionResult> UpdateRoles(UserRolesViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View("ManageRoles", model);
            }
            
            var user = await _userManager.FindByIdAsync(model.UserId);
            if (user == null)
            {
                return NotFound();
            }
            
            // Lấy các quyền hiện tại của người dùng
            var userRoles = await _userManager.GetRolesAsync(user);
            
            // Xóa tất cả các quyền hiện tại
            var removeResult = await _userManager.RemoveFromRolesAsync(user, userRoles);
            if (!removeResult.Succeeded)
            {
                ModelState.AddModelError("", "Không thể xóa các quyền hiện tại.");
                return View("ManageRoles", model);
            }
            
            // Thêm các quyền đã chọn
            var selectedRoles = model.Roles.Where(r => r.IsSelected).Select(r => r.RoleName);
            var addResult = await _userManager.AddToRolesAsync(user, selectedRoles);
            if (!addResult.Succeeded)
            {
                ModelState.AddModelError("", "Không thể thêm các quyền đã chọn.");
                return View("ManageRoles", model);
            }
            
            // Kiểm tra nếu người dùng được gán quyền Premium
            if (selectedRoles.Contains("Premium") && !userRoles.Contains("Premium"))
            {
                // Cập nhật thông tin Premium cho người dùng
                var userStatus = await _context.AccountStatuses.FirstOrDefaultAsync(a => a.UserId == user.Id);
                if (userStatus == null)
                {
                    // Tạo mới nếu chưa có
                    userStatus = new AccountStatus
                    {
                        UserId = user.Id,
                        IsPremium = true,
                        StartDate = DateTime.Now,
                        EndDate = DateTime.Now.AddMonths(1) // Mặc định 1 tháng
                    };
                    _context.AccountStatuses.Add(userStatus);
                }
                else
                {
                    // Cập nhật nếu đã có
                    userStatus.IsPremium = true;
                    userStatus.StartDate = DateTime.Now;
                    userStatus.EndDate = DateTime.Now.AddMonths(1);
                    _context.AccountStatuses.Update(userStatus);
                }
                await _context.SaveChangesAsync();
            }
            
            TempData["SuccessMessage"] = "Đã cập nhật quyền cho người dùng thành công!";
            return RedirectToAction(nameof(UserDetails), new { id = model.UserId });
        }
        
        // Thêm action để gán quyền Premium cho người dùng
        [HttpPost]
        public async Task<IActionResult> SetPremium(string id)
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
            
            // Thêm người dùng vào role Premium nếu chưa có
            if (!await _userManager.IsInRoleAsync(user, "Premium"))
            {
                await _userManager.AddToRoleAsync(user, "Premium");
            }
            
            // Cập nhật thông tin Premium trong bảng AccountStatus
            var userStatus = await _context.AccountStatuses.FirstOrDefaultAsync(a => a.UserId == user.Id);
            if (userStatus == null)
            {
                // Tạo mới nếu chưa có
                userStatus = new AccountStatus
                {
                    UserId = user.Id,
                    IsPremium = true,
                    StartDate = DateTime.Now,
                    EndDate = DateTime.Now.AddMonths(1) // Quyền Premium có hiệu lực 1 tháng
                };
                _context.AccountStatuses.Add(userStatus);
            }
            else
            {
                // Cập nhật nếu đã có
                userStatus.IsPremium = true;
                userStatus.StartDate = DateTime.Now;
                userStatus.EndDate = DateTime.Now.AddMonths(1); // Quyền Premium có hiệu lực 1 tháng
                _context.AccountStatuses.Update(userStatus);
            }
            
            await _context.SaveChangesAsync();
            
            TempData["SuccessMessage"] = "Đã gán quyền Premium cho người dùng trong 1 tháng!";
            return RedirectToAction(nameof(UserDetails), new { id });
        }
        
        // Thêm action để hủy quyền Premium của người dùng
        [HttpPost]
        public async Task<IActionResult> RemovePremium(string id)
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
            
            // Xóa người dùng khỏi role Premium
            if (await _userManager.IsInRoleAsync(user, "Premium"))
            {
                await _userManager.RemoveFromRoleAsync(user, "Premium");
            }
            
            // Cập nhật thông tin Premium trong bảng AccountStatus
            var userStatus = await _context.AccountStatuses.FirstOrDefaultAsync(a => a.UserId == user.Id);
            if (userStatus != null)
            {
                userStatus.IsPremium = false;
                userStatus.EndDate = DateTime.Now;
                _context.AccountStatuses.Update(userStatus);
                await _context.SaveChangesAsync();
            }
            
            TempData["SuccessMessage"] = "Đã hủy quyền Premium của người dùng!";
            return RedirectToAction(nameof(UserDetails), new { id });
        }
    }
}
