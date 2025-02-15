using System;
using System.Net;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TVOnline.Models;
using Microsoft.AspNetCore.Identity.UI.Services;
using TVOnline.Data;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using TVOnline.ViewModels.Account;

namespace TVOnline.Controllers.Account {
    public class AccountController : Controller {
        private readonly SignInManager<Users> signInManager;
        private readonly UserManager<Users> userManager;
        private readonly IEmailSender emailSender;
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly AppDbContext _context;

        public AccountController(
            SignInManager<Users> signInManager,
            UserManager<Users> userManager,
            IEmailSender emailSender,
            IConfiguration configuration,
            IHttpContextAccessor _contextAccessor) {
            this.signInManager = signInManager;
            this.userManager = userManager;
            this.emailSender = emailSender;
            _configuration = configuration;
            this._contextAccessor = _contextAccessor;
        }

        public IActionResult Login() {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model) {
            if (ModelState.IsValid) {
                var result = await signInManager.PasswordSignInAsync(model.UserName, model.Password, model.RememberMe, false);
                if (result.Succeeded) {
                    return RedirectToAction("Index", "Home");
                }
                if (result.IsNotAllowed) {
                    ModelState.AddModelError("", "Tài khoản chưa được xác thực. Vui lòng kiểm tra email để xác thực tài khoản.");
                    return View(model);
                } else {
                    ModelState.AddModelError("", "Tài khoản hoặc mật khẩu không đúng");
                    return View(model);
                }
            }
            return View(model);
        }

        [HttpPost]
        public IActionResult ExternalLogin(string provider, string returnUrl = null) {
            // Tạo URL callback sau khi Google xác thực thành công
            var redirectUrl = Url.Action("ExternalLoginCallback", "Account", new { ReturnUrl = returnUrl });

            // Cấu hình properties cho việc xác thực với Google
            var properties = signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);

            // Chuyển hướng người dùng đến trang đăng nhập của Google
            return new ChallengeResult(provider, properties);
        }

        public async Task<IActionResult> ExternalLoginCallback(string? returnUrl , string? remoteError) {
            // Kiểm tra nếu có lỗi từ provider
            if (remoteError != null) {
                return RedirectToAction("Login", new { error = "Lỗi từ dịch vụ ngoài: " + remoteError });
            }

            // Lấy thông tin đăng nhập từ Google
            var info = await signInManager.GetExternalLoginInfoAsync();
            if (info == null) {
                return RedirectToAction("Login");
            }

            // Thử đăng nhập với thông tin từ Google
            var result = await signInManager.ExternalLoginSignInAsync(
                info.LoginProvider,
                info.ProviderKey,
                isPersistent: false);

            if (result.Succeeded) {
                // Đăng nhập thành công
                return RedirectToLocal(returnUrl);
            }

            // Lấy email từ thông tin Google
            var email = info.Principal.FindFirstValue(ClaimTypes.Email);

            // Kiểm tra xem email đã tồn tại chưa
            var existingUser = await userManager.FindByEmailAsync(email);

            if (existingUser != null) {
                // Email đã tồn tại, liên kết tài khoản Google với tài khoản hiện có
                var addLoginResult = await userManager.AddLoginAsync(existingUser, info);
                if (addLoginResult.Succeeded) {
                    // Đăng nhập người dùng
                    await signInManager.SignInAsync(existingUser, isPersistent: false);
                    return RedirectToAction("Index", "Home");
                } else {
                    TempData["ErrorMessage"] = "Không thể liên kết tài khoản Google với tài khoản hiện có.";
                    return RedirectToAction("Login");
                }
            } else {
                // Email chưa tồn tại, tạo tài khoản mới
                var givenName = info.Principal.FindFirstValue(ClaimTypes.GivenName);
                var surname = info.Principal.FindFirstValue(ClaimTypes.Surname);
                var fullName = $"{givenName} {surname}".Trim();
                if (string.IsNullOrEmpty(fullName)) {
                    fullName = email.Split('@')[0];
                }

                var newUser = new Users {
                    UserName = email,
                    Email = email,
                    EmailConfirmed = true,
                    FullName = fullName
                };


                var createResult = await userManager.CreateAsync(newUser);
                if (createResult.Succeeded) {
                    // Liên kết tài khoản Google
                    await userManager.AddLoginAsync(newUser, info);
                    await signInManager.SignInAsync(newUser, isPersistent: false);
                    return RedirectToAction("Index", "Home");
                } else {
                    TempData["ErrorMessage"] = "Lỗi tạo tài khoản mới: " + string.Join(", ", createResult.Errors.Select(e => e.Description));
                    return RedirectToAction("Login");
                }
            }
        }

        private IActionResult RedirectToLocal(string returnUrl) {
            if (Url.IsLocalUrl(returnUrl)) {
                return Redirect(returnUrl);
            }
            return RedirectToAction("Index", "Home");
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
                        <p><a href='{confirmationLink}'>Xác nhận email</a></p>";

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
        public async Task<IActionResult> ConfirmEmail(string uid, string token) {
            if (!string.IsNullOrEmpty(uid) && !string.IsNullOrEmpty(token)) {
                token = token.Replace(" ", "");
                string decodedToken = WebUtility.UrlDecode(token);

                var result = await ConfirmEmailAsync(uid, decodedToken);
                if (result.Succeeded) {
                    ViewBag.IsSuccess = true;
                }
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
                <p><a href='{confirmationLink}'>Xác nhận email</a></p>";

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
                var user = await userManager.FindByEmailAsync(model.Email);
                if (user == null) {
                    ModelState.AddModelError("", "Không tìm thấy tài khoản với email này");
                    return View(model);
                }

                return RedirectToAction("ChangePassword", new { email = model.Email });
            }
            return View(model);
        }

        public IActionResult ChangePassword(string email) {
            if (string.IsNullOrEmpty(email)) {
                return RedirectToAction("CheckEmail");
            }

            var model = new ChangePasswordViewModel { Email = email };
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model) {
            if (ModelState.IsValid) {
                var user = await userManager.FindByEmailAsync(model.Email);
                if (user != null) {
                    var result = await userManager.RemovePasswordAsync(user);
                    if (result.Succeeded) {
                        result = await userManager.AddPasswordAsync(user, model.NewPassword);
                        if (result.Succeeded) {
                            TempData["SuccessMessage"] = "Đổi mật khẩu thành công! Vui lòng đăng nhập lại.";
                            return RedirectToAction("Login");
                        } else {
                            foreach (var error in result.Errors) {
                                ModelState.AddModelError("", error.Description);
                            }
                        }
                    } else {
                        ModelState.AddModelError("", "Không thể đặt lại mật khẩu. Vui lòng thử lại sau.");
                    }
                } else {
                    ModelState.AddModelError("", "Không tìm thấy tài khoản với email này");
                }
            }
            return View(model);
        }
        public async Task<IActionResult> Logout() {
            await signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> EditProfile() {
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userCur = await userManager.FindByIdAsync(userId);

            if (userCur == null) {
                return View("Error");
            }

            var editProfileViewModel = new EditProfileViewModel() {
                Id = userId,
                Name = userCur.FullName,
                Age = userCur.Age,
                Email = userCur.Email,
                PhoneNumber = userCur.PhoneNumber,
                City = userCur.City,
                CvFileUrl = userCur.CvFileUrl,
                JobIndustry = userCur.JobIndustry
            };
            return View(editProfileViewModel);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> EditProfile(EditProfileViewModel model) {
            if (!ModelState.IsValid) {
                ModelState.AddModelError("", "Vui lòng kiểm tra lại thông tin");
                return View(model);
            }

            // Lấy user hiện tại
            var user = await userManager.FindByIdAsync(model.Id);
            if (user == null) {
                ModelState.AddModelError("", "Không tìm thấy người dùng");
                return View(model);
            }

            // Cập nhật thông tin
            user.FullName = model.Name;
            user.PhoneNumber = model.PhoneNumber;
            user.Age = model.Age;
            user.City = model.City;
            user.JobIndustry = model.JobIndustry;
            if (model.CvFileUrl != null) {
                user.CvFileUrl = model.CvFileUrl;
            }

            // Lưu thay đổi sử dụng UserManager
            var result = await userManager.UpdateAsync(user);
            if (result.Succeeded) {
                TempData["SuccessMessage"] = "Cập nhật thông tin thành công!";
                return RedirectToAction("Index", "Home");
            }

            foreach (var error in result.Errors) {
                ModelState.AddModelError("", error.Description);
            }

            return View(model);
        }

    }
}
