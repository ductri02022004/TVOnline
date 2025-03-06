using System;
using System.Net;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity.UI.Services;
using TVOnline.Data;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using TVOnline.ViewModels.Account;
using System.Text.Json;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.CSharp.RuntimeBinder;
using System.Collections;

namespace TVOnline.Controllers.Account
{
    public class AccountController : Controller
    {
        private readonly SignInManager<Users> signInManager;
        private readonly UserManager<Users> userManager;
        private readonly IEmailSender emailSender;
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly IMemoryCache _memoryCache;
        private readonly IUserCvService _userCvService;
        private readonly IPostService _postService;

        public AccountController(
            SignInManager<Users> signInManager,
            UserManager<Users> userManager,
            IEmailSender emailSender,
            IConfiguration configuration,
            IHttpContextAccessor _contextAccessor,
            IMemoryCache memoryCache, IUserCvService userCvService, IPostService postService)
        {
            this.signInManager = signInManager;
            this.userManager = userManager;
            this.emailSender = emailSender;
            _configuration = configuration;
            this._contextAccessor = _contextAccessor;
            _memoryCache = memoryCache;
            _userCvService = userCvService;
            _postService = postService;
        }

        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var result =
                    await signInManager.PasswordSignInAsync(model.UserName, model.Password, model.RememberMe, false);
                if (result.Succeeded)
                {
                    return RedirectToAction("Index", "Home");
                }

                if (result.IsNotAllowed)
                {
                    ModelState.AddModelError("",
                        "Tài khoản chưa được xác thực. Vui lòng kiểm tra email để xác nhận tài khoản.");
                    return View(model);
                }
                else
                {
                    ModelState.AddModelError("", "Tài khoản hoặc mật khẩu không đúng");
                    return View(model);
                }
            }

            return View(model);
        }

        [HttpPost]
        public IActionResult ExternalLogin(string provider, string? returnUrl = null)
        {
            // Tạo URL callback sau khi Google xác thực thành công
            var redirectUrl = Url.Action("ExternalLoginCallback", "Account", new { ReturnUrl = returnUrl });

            // Cấu hình properties cho việc xác thực với Google
            var properties = signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);

            // Chuyển hướng người dùng đến trang đăng nhập của Google
            return new ChallengeResult(provider, properties);
        }

        public async Task<IActionResult> ExternalLoginCallback(string? returnUrl, string? remoteError)
        {
            // Kiểm tra nếu có lỗi từ provider
            if (remoteError != null)
            {
                return RedirectToAction("Login", new { error = "Lỗi từ dịch vụ ngoài: " + remoteError });
            }

            // Lấy thông tin đăng nhập từ Google
            var info = await signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                return RedirectToAction("Login");
            }

            // Thử đăng nhập với thông tin từ Google
            var result = await signInManager.ExternalLoginSignInAsync(
                info.LoginProvider,
                info.ProviderKey,
                isPersistent: false);

            if (result.Succeeded)
            {
                // Đăng nhập thành công
                return RedirectToLocal(returnUrl);
            }

            // Lấy email từ thông tin Google
            var email = info.Principal.FindFirstValue(ClaimTypes.Email);

            // Kiểm tra xem email đã tồn tại chưa
            var existingUser = await userManager.FindByEmailAsync(email);

            if (existingUser != null)
            {
                // Email đã tồn tại, liên kết tài khoản Google với tài khoản hiện có
                var addLoginResult = await userManager.AddLoginAsync(existingUser, info);
                if (addLoginResult.Succeeded)
                {
                    // Đăng nhập người dùng
                    await signInManager.SignInAsync(existingUser, isPersistent: false);
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    TempData["ErrorMessage"] = "Không thể liên kết tài khoản Google với tài khoản hiện có.";
                    return RedirectToAction("Login");
                }
            }
            else
            {
                // Email chưa tồn tại, tạo tài khoản mới
                var givenName = info.Principal.FindFirstValue(ClaimTypes.GivenName);
                var surname = info.Principal.FindFirstValue(ClaimTypes.Surname);
                var fullName = $"{givenName} {surname}".Trim();
                if (string.IsNullOrEmpty(fullName))
                {
                    fullName = email.Split('@')[0];
                }

                var newUser = new Users
                {
                    UserName = email,
                    Email = email,
                    EmailConfirmed = true,
                    FullName = fullName
                };


                var createResult = await userManager.CreateAsync(newUser);
                if (createResult.Succeeded)
                {
                    // Liên kết tài khoản Google
                    await userManager.AddLoginAsync(newUser, info);
                    await signInManager.SignInAsync(newUser, isPersistent: false);
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    TempData["ErrorMessage"] = "Lỗi tạo tài khoản mới: " +
                                               string.Join(", ", createResult.Errors.Select(e => e.Description));
                    return RedirectToAction("Login");
                }
            }
        }

        private IActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction("Index", "Home");
        }

        public IActionResult Register()
        {
            return View();
        }

        // Key used to store pending registrations in memory cache
        private const string PENDING_REGISTRATION_PREFIX = "PendingRegistration_";

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Check if email is already registered
                var existingUser = await userManager.FindByEmailAsync(model.Email);
                if (existingUser != null)
                {
                    if (existingUser.EmailConfirmed)
                    {
                        ModelState.AddModelError("", "Email này đã được đăng ký trước đó.");
                        return View(model);
                    }
                    else
                    {
                        // If there's an unconfirmed user with this email, we'll overwrite the registration
                        await userManager.DeleteAsync(existingUser);
                    }
                }

                // Create a temporary registration ID
                var registrationId = Guid.NewGuid().ToString();

                // Store user registration data in memory cache
                var pendingUser = new
                {
                    FullName = model.Name,
                    Email = model.Email,
                    Password = model.Password,
                    PhoneNumber = string.IsNullOrEmpty(model.PhoneNumber) ? null : model.PhoneNumber,
                    City = string.IsNullOrEmpty(model.City) ? null : model.City,
                    RegistrationTime = DateTime.UtcNow
                };

                // Cache options - expired after 1 hours
                var cacheOptions = new MemoryCacheEntryOptions()
                    .SetAbsoluteExpiration(TimeSpan.FromHours(1));

                // Store pending registration in cache
                _memoryCache.Set(PENDING_REGISTRATION_PREFIX + registrationId, pendingUser, cacheOptions);

                // Store email-to-registrationId mapping in cache
                _memoryCache.Set("EmailMapping_" + model.Email, registrationId, cacheOptions);

                // Generate confirmation link with registration ID
                var confirmationLink = Url.Action("ConfirmEmail", "Account",
                    new { registrationId = registrationId },
                    protocol: Request.Scheme);

                var emailBody = $@"
                    <h2>Xác nhận đăng ký tài khoản</h2>
                    <p>Xin chào {pendingUser.FullName},</p>
                    <p>Vui lòng nhấn vào link bên dưới để xác nhận email của bạn:</p>
                    <p><a href='{confirmationLink}'>Xác nhận email</a></p>";

                await emailSender.SendEmailAsync(pendingUser.Email, "Xác nhận email đăng ký", emailBody);
                TempData["SuccessMessage"] = "Đăng ký thành công! Vui lòng kiểm tra email để xác nhận tài khoản.";
                return RedirectToAction("VerifyEmail");
            }

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> ConfirmEmail(string registrationId)
        {
            try
            {
                if (string.IsNullOrEmpty(registrationId))
                {
                    ViewBag.IsSuccess = false;
                    ViewBag.Message = "Link xác nhận không hợp lệ.";
                    return View();
                }

                // Retrieve pending registration from cache
                if (!_memoryCache.TryGetValue(PENDING_REGISTRATION_PREFIX + registrationId,
                        out object pendingRegistrationObj))
                {
                    ViewBag.IsSuccess = false;
                    ViewBag.Message = "Link xác nhận đã hết hạn hoặc không hợp lệ.";
                    return View();
                }

                // Convert cached object to dynamic
                dynamic pendingRegistration = pendingRegistrationObj;

                // Create the user now
                Users user = new Users
                {
                    FullName = pendingRegistration.FullName,
                    Email = pendingRegistration.Email,
                    UserName = pendingRegistration.Email,
                    PhoneNumber = pendingRegistration.PhoneNumber,
                    UserCity = pendingRegistration.City,
                    EmailConfirmed = true // Set email as confirmed
                };

                var result = await userManager.CreateAsync(user, pendingRegistration.Password);

                if (result.Succeeded)
                {
                    // Remove from cache after successful creation
                    _memoryCache.Remove(PENDING_REGISTRATION_PREFIX + registrationId);
                    _memoryCache.Remove("EmailMapping_" + pendingRegistration.Email);

                    ViewBag.IsSuccess = true;
                    ViewBag.Message =
                        "Email của bạn đã được xác nhận thành công. Bây giờ bạn có thể đăng nhập vào tài khoản của mình.";

                    // Auto login after confirmation
                    await signInManager.SignInAsync(user, isPersistent: false);
                    return View();
                }
                else
                {
                    ViewBag.IsSuccess = false;
                    // Fix for CS1977 - Avoid using lambda with dynamic result
                    string errorMessages = "";
                    foreach (var error in result.Errors)
                    {
                        errorMessages += error.Description + ", ";
                    }

                    if (errorMessages.EndsWith(", "))
                    {
                        errorMessages = errorMessages.Substring(0, errorMessages.Length - 2);
                    }

                    ViewBag.Message = "Xác nhận email không thành công: " + errorMessages;
                }
            }
            catch (Exception ex)
            {
                ViewBag.IsSuccess = false;
                ViewBag.Message = "Đã xảy ra lỗi trong quá trình xác nhận email. Vui lòng thử lại sau.";
                // Log the exception for debugging
                Console.WriteLine($"Exception during email confirmation: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
            }

            return View();
        }

        public IActionResult VerifyEmail()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ResendConfirmationEmail(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                TempData["ErrorMessage"] = "Vui lòng nhập địa chỉ email.";
                return RedirectToAction("VerifyEmail");
            }

            // Look for pending registration with this email
            string matchingKey = null;

            // Instead of trying to enumerate all keys, we'll store email-to-registrationId mapping
            // in a separate cache entry
            var emailMappingKey = "EmailMapping_" + email;
            if (_memoryCache.TryGetValue(emailMappingKey, out string registrationId))
            {
                // We found a mapping for this email, check if the registration still exists
                var fullKey = PENDING_REGISTRATION_PREFIX + registrationId;
                if (_memoryCache.TryGetValue(fullKey, out object pendingRegistration))
                {
                    matchingKey = fullKey;
                }
            }

            if (matchingKey != null)
            {
                // Found existing registration
                var pendingRegistration = _memoryCache.Get(matchingKey);

                // Generate new registration ID
                var newRegistrationId = Guid.NewGuid().ToString();

                // Cache options - expired after 1 hours
                var cacheOptions = new MemoryCacheEntryOptions()
                    .SetAbsoluteExpiration(TimeSpan.FromHours(1));

                // Store with new ID
                _memoryCache.Set(PENDING_REGISTRATION_PREFIX + newRegistrationId, pendingRegistration, cacheOptions);
                _memoryCache.Set("EmailMapping_" + email, newRegistrationId, cacheOptions);

                // Generate new confirmation link
                var confirmationLink = Url.Action("ConfirmEmail", "Account",
                    new { registrationId = newRegistrationId },
                    protocol: Request.Scheme);

                // Get user name safely
                string userName = "Người dùng";
                try
                {
                    dynamic dynPendingReg = pendingRegistration;
                    userName = dynPendingReg.FullName ?? "Người dùng";
                }
                catch (RuntimeBinderException)
                {
                    // Use default name if property access fails
                }

                var emailBody = $@"
                    <h2>Xác nhận đăng ký tài khoản</h2>
                    <p>Xin chào {userName},</p>
                    <p>Vui lòng nhấn vào link bên dưới để xác nhận email của bạn:</p>
                    <p><a href='{confirmationLink}'>Xác nhận email</a></p>";

                await emailSender.SendEmailAsync(email, "Xác nhận email đăng ký", emailBody);
                TempData["SuccessMessage"] = "Email xác nhận đã được gửi lại. Vui lòng kiểm tra hộp thư của bạn.";
                return RedirectToAction("VerifyEmail");
            }
            else
            {
                // Check if this email is already registered in the database
                var existingUser = await userManager.FindByEmailAsync(email);
                if (existingUser != null)
                {
                    TempData["ErrorMessage"] = "Email này đã được đăng ký và xác nhận. Vui lòng đăng nhập.";
                    return RedirectToAction("Login");
                }

                // Email not found in pending registrations and not in database
                TempData["ErrorMessage"] = "Email này chưa được đăng ký. Vui lòng đăng ký tài khoản mới.";
                return RedirectToAction("Register");
            }
        }

        public IActionResult CheckEmail()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CheckEmail(CheckEmailViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await userManager.FindByEmailAsync(model.Email);
                if (user == null)
                {
                    ModelState.AddModelError("", "Không tìm thấy tài khoản với email này");
                    return View(model);
                }

                return RedirectToAction("ChangePassword", new { email = model.Email });
            }

            return View(model);
        }

        public IActionResult ChangePassword(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                return RedirectToAction("CheckEmail");
            }

            var model = new ChangePasswordViewModel { Email = email };
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await userManager.FindByEmailAsync(model.Email);
                if (user != null)
                {
                    var result = await userManager.RemovePasswordAsync(user);
                    if (result.Succeeded)
                    {
                        result = await userManager.AddPasswordAsync(user, model.NewPassword);
                        if (result.Succeeded)
                        {
                            TempData["SuccessMessage"] = "Đổi mật khẩu thành công! Vui lòng đăng nhập lại.";
                            return RedirectToAction("Login");
                        }
                        else
                        {
                            foreach (var error in result.Errors)
                            {
                                ModelState.AddModelError("", error.Description);
                            }
                        }
                    }
                    else
                    {
                        ModelState.AddModelError("", "Không thể đặt lại mật khẩu. Vui lòng thử lại sau.");
                    }
                }
                else
                {
                    ModelState.AddModelError("", "Không tìm thấy tài khoản với email này");
                }
            }

            return View(model);
        }

        public async Task<IActionResult> Logout()
        {
            await signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        public IActionResult AccessDenied(string? returnUrl = null)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> UpgradeAccount()
        {
            string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return View("Error");
            var userCur = await userManager.FindByIdAsync(userId);
            return userCur == null ? View("Error") : View("Upgrade");
        }

        [Route("/AppliedJob")]
        public async Task<IActionResult> AppliedJob()
        {
            string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var appliedJobs = await _userCvService.GetAppliedJobsByUserIdAsync(userId);
            return View(appliedJobs);
        }

        [HttpPost]
        public async Task<ActionResult> CancelApplication(string postId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier); // Lấy ID của user đang đăng nhập

            if (userId == null)
            {
                TempData["ErrorMessage"] = "Bạn cần đăng nhập để thực hiện thao tác này.";
                return RedirectToAction("AppliedJob");
            }

            bool isDeleted = await _userCvService.CancelAppliedJobAsync(userId, postId);

            return RedirectToAction("AppliedJob");
        }

        [HttpGet]
        [Route("/SavedJobs")]
        public async Task<IActionResult> GetSavedJobs(int page = 1)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var savedJobPostsResponse = await _postService.GetSavedPostsForJobSeeker(userId);

            int pageSize = 5;
            int totalSavedPosts = savedJobPostsResponse.Count();
            int totalPages = (int)Math.Ceiling((double)totalSavedPosts / pageSize);

            var pagedSavedPosts = savedJobPostsResponse.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalPosts = totalSavedPosts;

            return View("SavedJobsView", pagedSavedPosts);
        }

        [HttpPost]
        [Route("[controller]/[action]/{postId}")]
        public async Task<IActionResult> UnsaveJob(string postId)
        {
            if (string.IsNullOrEmpty(postId))
            {
                return BadRequest("Post ID is required.");
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            bool isUnsaved = await _postService.DeleteSavedJobForJobSeeker(postId, userId);

            return isUnsaved
                ? RedirectToAction("GetSavedJobs")
                : BadRequest(new { message = "Failed to unsave job." }); // Indicate failure
        }
    }
}