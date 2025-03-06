using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TVOnline.ViewModels.UserProfile;

namespace TVOnline.Controllers.EditUserProfile {
    [Authorize]
    public class EditUserProfileController : Controller {
        private readonly UserManager<Users> _userManager;
        private readonly SignInManager<Users> _signInManager;

        public EditUserProfileController(
            UserManager<Users> userManager,
            SignInManager<Users> signInManager) {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        public async Task<IActionResult> Index() {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) {
                return RedirectToAction("Login", "Account");
            }

            // Kiểm tra xem người dùng có mật khẩu hay không
            var hasPassword = await _userManager.HasPasswordAsync(user);
            ViewBag.HasPassword = hasPassword;

            var model = new EditUserProfileViewModel {
                Id = user.Id,
                Name = user.FullName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                City = user.UserCity,
                Job = user.UserJob,
                Dob = user.Dob
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateProfile(EditUserProfileViewModel model) {
            if (!ModelState.IsValid) {
                ViewBag.HasPassword = await _userManager.HasPasswordAsync(await _userManager.GetUserAsync(User));
                return View("Index", model);
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null) {
                return RedirectToAction("Login", "Account");
            }

            user.FullName = model.Name;
            user.PhoneNumber = model.PhoneNumber;
            user.UserCity = model.City;
            user.UserJob = model.Job;
            user.Dob = model.Dob;

            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded) {
                return RedirectToAction("Index", new { success = "Cập nhật thông tin thành công" });
            }

            foreach (var error in result.Errors) {
                ModelState.AddModelError("", error.Description);
            }
            ViewBag.HasPassword = await _userManager.HasPasswordAsync(user);
            return View("Index", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ProfilePasswordChangeViewModel model) {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) {
                return RedirectToAction("Login", "Account");
            }

            var hasPassword = await _userManager.HasPasswordAsync(user);

            // Kiểm tra mật khẩu mới và xác nhận mật khẩu
            if (model.NewPassword != model.ConfirmNewPassword) {
                return RedirectToAction("Index", new { error = "Mật khẩu xác nhận không khớp" });
            }

            if (model.NewPassword.Length < 6) {
                return RedirectToAction("Index", new { error = "Mật khẩu phải có ít nhất 6 ký tự" });
            }

            if (!hasPassword) {
                // Nếu chưa có mật khẩu, tạo mật khẩu mới
                var addPasswordResult = await _userManager.AddPasswordAsync(user, model.NewPassword);
                if (addPasswordResult.Succeeded) {
                    await _signInManager.SignInAsync(user, isPersistent: false);
                    return RedirectToAction("Index", new { success = "Tạo mật khẩu thành công" });
                }

                var errorMessage = addPasswordResult.Errors.FirstOrDefault()?.Description ?? "Không thể tạo mật khẩu. Vui lòng thử lại.";
                return RedirectToAction("Index", new { error = errorMessage });
            }

            // Nếu đã có mật khẩu, kiểm tra mật khẩu hiện tại
            if (string.IsNullOrEmpty(model.CurrentPassword)) {
                return RedirectToAction("Index", new { error = "Vui lòng nhập mật khẩu hiện tại" });
            }

            var changePasswordResult = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);
            if (changePasswordResult.Succeeded) {
                await _signInManager.SignInAsync(user, isPersistent: false);
                return RedirectToAction("Index", new { success = "Đổi mật khẩu thành công" });
            }

            var changePasswordError = changePasswordResult.Errors.FirstOrDefault()?.Description ?? "Không thể đổi mật khẩu. Vui lòng thử lại.";
            return RedirectToAction("Index", new { error = changePasswordError });
        }
    }
}
