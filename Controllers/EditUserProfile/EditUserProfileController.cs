using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TVOnline.Models;
using TVOnline.ViewModels.UserProfile;

namespace TVOnline.Controllers.EditUserProfile {
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
        public async Task<IActionResult> UpdateProfile(EditUserProfileViewModel model) {
            if (!ModelState.IsValid) {
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
            return View("Index", model);
        }

        [HttpPost]
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

            // Thực hiện đổi mật khẩu
            var result = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);
            if (result.Succeeded) {
                await _signInManager.SignInAsync(user, isPersistent: false);
                return RedirectToAction("Index", new { success = "Đổi mật khẩu thành công" });
            }

            // Xử lý các lỗi cụ thể
            var error = result.Errors.FirstOrDefault();
            if (error != null && error.Code == "PasswordMismatch") {
                return RedirectToAction("Index", new { error = "Mật khẩu hiện tại không đúng" });
            }

            return RedirectToAction("Index", new { error = "Không thể đổi mật khẩu. Vui lòng thử lại." });
        }
    }
}
