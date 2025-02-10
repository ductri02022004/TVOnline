using System.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.IdentityModel.Tokens;
using TVOnline.Models;
using TVOnline.ViewModels;

namespace TVOnline.Controllers {
    public class AccountController : Controller {
        private readonly SignInManager<Users> signInManager;
        private readonly UserManager<Users> userManager;
        private readonly IEmailSender emailSender;
        private readonly IConfiguration _configuration;

        public AccountController(
            SignInManager<Users> signInManager,
            UserManager<Users> userManager,
            IEmailSender emailSender,
            IConfiguration configuration) {
            this.signInManager = signInManager;
            this.userManager = userManager;
            this.emailSender = emailSender;
            _configuration = configuration;
        }

        public IActionResult Login() {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model) {
            if (ModelState.IsValid) {
                var result = await signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, false);
                if (result.Succeeded) {
                    return RedirectToAction("Index", "Home");
                }
                if (result.IsNotAllowed) {
                    ModelState.AddModelError("", "Email chưa được xác thực. Vui lòng kiểm tra email để xác thực tài khoản.");
                    return View(model);
                } else {
                    ModelState.AddModelError("", "Email hoặc mật khẩu không đúng");
                    return View(model);
                }
            }
            return View(model);
        }

        public IActionResult Register() {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model) {
            if (ModelState.IsValid) {
                Users users = new Users {
                    FullName = model.Name,
                    Email = model.Email,
                    UserName = model.Email,
                    PhoneNumber = string.IsNullOrEmpty(model.PhoneNumber) ? null : model.PhoneNumber,
                    City = string.IsNullOrEmpty(model.City) ? null : model.City
                };
                var result = await userManager.CreateAsync(users, model.Password);
                if (result.Succeeded) {
                    var token = await userManager.GenerateEmailConfirmationTokenAsync(users);
                    var encodedToken = WebUtility.UrlEncode(token);

                    var appDomain = _configuration["Application:AppDomain"];
                    var confirmationLink = $"{appDomain}/Account/ConfirmEmail?uid={users.Id}&token={encodedToken}";

                    var emailBody = $@"
                        <h2>Xác nhận đăng ký tài khoản</h2>
                        <p>Xin chào {users.FullName},</p>
                        <p>Vui lòng nhấn vào link bên dưới để xác nhận email của bạn:</p>
                        <p><a href='{confirmationLink}'>Xác nhận email</a></p>
                        <p>Hoặc copy đường link sau vào trình duyệt:</p>
                        <p>{confirmationLink}</p>
                        <p>Nếu bạn không thực hiện đăng ký tài khoản, vui lòng bỏ qua email này.</p>";

                    await emailSender.SendEmailAsync(users.Email, "Xác nhận email đăng ký", emailBody);
                    TempData["SuccessMessage"] = "Đăng ký thành công! Vui lòng kiểm tra email để xác nhận tài khoản.";
                    return RedirectToAction("VerifyEmail");
                } else {
                    foreach (var error in result.Errors) {
                        ModelState.AddModelError("", error.Description);
                    }
                    return View(model);
                }
            }
            return View(model);
        }


        public async Task<IdentityResult> ConfirmEmailAsync(string uid, string token) {
            return await userManager.ConfirmEmailAsync(await userManager.FindByIdAsync(uid), token);
        }


        [HttpGet("confirm-email")]
        public async Task<IActionResult> ConfirmEmail(string uid, string token)
        {
            if (string.IsNullOrEmpty(uid) || string.IsNullOrEmpty(token))
            {
                ViewBag.IsSuccess = false;
                ViewBag.Message = "Link xác nhận không hợp lệ";
                return View();
            }

            var user = await userManager.FindByIdAsync(uid);
            if (user == null)
            {
                ViewBag.IsSuccess = false;
                ViewBag.Message = "Không tìm thấy tài khoản";
                return View();
            }

            if (await userManager.IsEmailConfirmedAsync(user))
            {
                ViewBag.IsSuccess = true;
                ViewBag.Message = "Email của bạn đã được xác nhận trước đó";
                return View();
            }

            var decodedToken = WebUtility.UrlDecode(token);
            var result = await userManager.ConfirmEmailAsync(user, decodedToken);

            if (result.Succeeded)
            {
                ViewBag.IsSuccess = true;
                ViewBag.Message = "Xác nhận email thành công!";
            }
            else
            {
                ViewBag.IsSuccess = false;
                ViewBag.Message = "Không thể xác nhận email. Vui lòng thử lại sau.";
            }

            return View();
        }

        public IActionResult VerifyEmail() {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ResendConfirmationEmail(string email) {
            if (string.IsNullOrEmpty(email)) {
                TempData["ErrorMessage"] = "Vui lòng nhập địa chỉ email.";
                return RedirectToAction("VerifyEmail");
            }

            var user = await userManager.FindByEmailAsync(email);
            if (user == null) {
                TempData["ErrorMessage"] = "Không tìm thấy tài khoản với email này.";
                return RedirectToAction("VerifyEmail");
            }

            if (await userManager.IsEmailConfirmedAsync(user)) {
                TempData["SuccessMessage"] = "Email của bạn đã được xác nhận trước đó. Vui lòng đăng nhập.";
                return RedirectToAction("Login");
            }

            var token = await userManager.GenerateEmailConfirmationTokenAsync(user);
            var encodedToken = WebUtility.UrlEncode(token);

            var appDomain = _configuration["Application:AppDomain"];
            var confirmationLink = $"{appDomain}/Account/ConfirmEmail?uid={user.Id}&token={encodedToken}";

            var emailBody = $@"
                <h2>Xác nhận đăng ký tài khoản</h2>
                <p>Xin chào {user.FullName},</p>
                <p>Vui lòng nhấn vào link bên dưới để xác nhận email của bạn:</p>
                <p><a href='{confirmationLink}'>Xác nhận email</a></p>
                <p>Hoặc copy đường link sau vào trình duyệt:</p>
                <p>{confirmationLink}</p>
                <p>Nếu bạn không thực hiện đăng ký tài khoản, vui lòng bỏ qua email này.</p>";

            await emailSender.SendEmailAsync(user.Email, "Xác nhận email đăng ký", emailBody);
            TempData["SuccessMessage"] = "Email xác nhận đã được gửi lại. Vui lòng kiểm tra hộp thư của bạn.";
            return RedirectToAction("VerifyEmail");
        }

        public IActionResult CheckEmail() {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CheckEmail(CheckEmailViewModel model) {
            if (ModelState.IsValid) {
                var userEmail = await userManager.FindByNameAsync(model.Email);
                if (User == null) {
                    ModelState.AddModelError("", "Có lỗi xảy ra");
                    return View(model);
                } else {
                    return RedirectToAction("ChangePassword", "Account", new { username = userEmail.UserName });
                }
            }
            return View(model);
        }

        public IActionResult ChangePassword(string userName) {
            if (string.IsNullOrEmpty(userName)) {
                return RedirectToAction("CheckEmail", "Account");
            }
            return View(new ChangePasswordViewModel { Email = userName });
        }

        [HttpPost]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model) {
            if (ModelState.IsValid) {
                var user = await userManager.FindByEmailAsync(model.Email);
                if (user != null) {
                    var result = await userManager.RemovePasswordAsync(user);
                    if (result.Succeeded) {
                        result = await userManager.AddPasswordAsync(user, model.NewPassword);
                        return RedirectToAction("Login", "Account");
                    } else {
                        foreach (var error in result.Errors) {
                            ModelState.AddModelError("", error.Description);
                        }
                        return View(model);
                    }
                } else {
                    ModelState.AddModelError("", "Không tìm thấy email. Hãy thử lại");
                    return View(model);
                }
            } else {
                ModelState.AddModelError("", "Lỗi. Hãy thử lại");
                return View(model);
            }
        }

        public async Task<IActionResult> Logout() {
            await signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }
    }
}
