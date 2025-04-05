using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TVOnline.Data;
using TVOnline.Models;
using TVOnline.Service.Employers;
using TVOnline.ViewModels.Employer;

namespace TVOnline.Controllers.Employer
{
    [Authorize]
    public class EmployerDashboardController : Controller
    {
        private readonly UserManager<Users> _userManager;
        private readonly AppDbContext _context;
        private readonly ILogger<EmployerDashboardController> _logger;
        private readonly IEmployersService _employersService;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public EmployerDashboardController(
            UserManager<Users> userManager,
            AppDbContext context,
            ILogger<EmployerDashboardController> logger,
            IEmployersService employersService,
            IWebHostEnvironment webHostEnvironment)
        {
            _userManager = userManager;
            _context = context;
            _logger = logger;
            _employersService = employersService;
            _webHostEnvironment = webHostEnvironment;
        }

        public async Task<IActionResult> Index()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account");
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Kiểm tra quyền truy cập
            var isEmployer = await _userManager.IsInRoleAsync(user, "Employer");
            if (!isEmployer)
            {
                return RedirectToAction("Register", "EmployerRegistration");
            }

            var employer = await _context.Employers
                .FirstOrDefaultAsync(e => e.UserId == user.Id);

            if (employer == null)
            {
                return RedirectToAction("Register", "EmployerRegistration");
            }

            // Lấy thông tin công ty
            var companyInfo = await _employersService.GetEmployerById(employer.EmployerId);
            var companyViewModel = companyInfo.ToCompanyViewModel();

            // Lấy thông tin thành phố
            var city = await _context.Cities.FirstOrDefaultAsync(c => c.CityId == employer.CityId);
            var cityName = city?.CityName ?? "Chưa cập nhật";

            // Tạo CompanyInfoViewModel
            var companyInfoViewModel = new CompanyInfoViewModel
            {
                EmployerId = employer.EmployerId,
                CompanyName = employer.CompanyName,
                Field = employer.Field,
                Email = employer.Email,
                PhoneNumber = user.PhoneNumber,
                Description = employer.Description,
                LogoURL = employer.LogoURL,
                CityId = employer.CityId, // Sửa lỗi chuyển đổi kiểu dữ liệu
                CityName = cityName
            };

            // Đếm số lượng tin tuyển dụng
            var totalPosts = await _context.Posts
                .Where(p => p.EmployerId == employer.EmployerId)
                .CountAsync();

            // Đếm số lượng ứng viên đã ứng tuyển
            var totalApplications = await _context.UserCVs
                .Include(cv => cv.Post)
                .Where(cv => cv.Post.EmployerId == employer.EmployerId)
                .CountAsync();

            // Lấy danh sách các CV gần đây
            var recentApplications = await _context.UserCVs
                .Include(cv => cv.Users)
                .Include(cv => cv.Post)
                .Where(cv => cv.Post.EmployerId == employer.EmployerId)
                .OrderByDescending(cv => cv.AppliedDate)
                .Take(5)
                .Select(cv => new RecentApplicationViewModel
                {
                    UserName = cv.Users.FullName,
                    PostTitle = cv.Post.Title,
                    AppliedDate = cv.AppliedDate,
                    Status = cv.CVStatus,
                    CvId = cv.CvID
                })
                .ToListAsync();

            // Thống kê số lượng CV theo trạng thái
            var allApplications = await _context.UserCVs
                .Include(cv => cv.Post)
                .Where(cv => cv.Post.EmployerId == employer.EmployerId)
                .ToListAsync();

            var applicationStats = new ApplicationStatisticsViewModel
            {
                TotalApplications = allApplications.Count,
                AppliedCount = allApplications.Count(cv => cv.CVStatus == "Applied"),
                ReviewingCount = allApplications.Count(cv => cv.CVStatus == "Reviewing"),
                ShortlistedCount = allApplications.Count(cv => cv.CVStatus == "Shortlisted"),
                RejectedCount = allApplications.Count(cv => cv.CVStatus == "Rejected"),
                InterviewedCount = allApplications.Count(cv => cv.CVStatus == "Interviewed")
            };

            var viewModel = new EmployerDashboardViewModel
            {
                Company = companyViewModel,
                CompanyInfo = companyInfoViewModel,
                TotalPosts = totalPosts,
                TotalApplications = totalApplications,
                RecentApplications = recentApplications,
                ApplicationStatistics = applicationStats
            };

            return View(viewModel);
        }

        public async Task<IActionResult> ManageApplications(string status = null)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account");
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Kiểm tra quyền truy cập
            var isEmployer = await _userManager.IsInRoleAsync(user, "Employer");
            if (!isEmployer)
            {
                return RedirectToAction("Register", "EmployerRegistration");
            }

            var employer = await _context.Employers
                .FirstOrDefaultAsync(e => e.UserId == user.Id);

            if (employer == null)
            {
                return RedirectToAction("Register", "EmployerRegistration");
            }

            // Lấy danh sách các CV đã nộp cho các bài đăng của nhà tuyển dụng này
            var query = _context.UserCVs
                .Include(cv => cv.Users)
                .Include(cv => cv.Post)
                .Where(cv => cv.Post.EmployerId == employer.EmployerId);

            // Lọc theo trạng thái nếu có
            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(cv => cv.CVStatus == status);
            }

            var applications = await query.OrderByDescending(cv => cv.Post.CreatedAt).ToListAsync();

            ViewBag.StatusFilter = status;
            return View(applications);
        }

        [Route("[action]/{cvId?}")]
        public async Task<IActionResult> ApplicationDetails(string? cvId, [FromQuery] string? id)
        {
            var cvIdToUse = cvId ?? id;
            if (string.IsNullOrEmpty(cvIdToUse))
            {
                return NotFound();
            }

            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account");
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Kiểm tra quyền truy cập
            var isEmployer = await _userManager.IsInRoleAsync(user, "Employer");
            if (!isEmployer)
            {
                return RedirectToAction("Register", "EmployerRegistration");
            }

            var employer = await _context.Employers
                .FirstOrDefaultAsync(e => e.UserId == user.Id);

            if (employer == null)
            {
                return RedirectToAction("Register", "EmployerRegistration");
            }

            // Lấy chi tiết CV
            var application = await _context.UserCVs
                .Include(cv => cv.Users)
                .Include(cv => cv.Post)
                .FirstOrDefaultAsync(cv => cv.CvID == cvIdToUse && cv.Post.EmployerId == employer.EmployerId);

            return application == null ? NotFound() : View(application);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateApplicationStatus(string cvId, string status)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account");
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Kiểm tra quyền truy cập
            var isEmployer = await _userManager.IsInRoleAsync(user, "Employer");
            if (!isEmployer)
            {
                return RedirectToAction("Register", "EmployerRegistration");
            }

            var employer = await _context.Employers
                .FirstOrDefaultAsync(e => e.UserId == user.Id);

            if (employer == null)
            {
                return RedirectToAction("Register", "EmployerRegistration");
            }

            // Cập nhật trạng thái CV
            var application = await _context.UserCVs
                .Include(cv => cv.Post)
                .FirstOrDefaultAsync(cv => cv.CvID == cvId && cv.Post.EmployerId == employer.EmployerId);

            if (application == null)
            {
                return NotFound();
            }

            application.CVStatus = status;
            await _context.SaveChangesAsync();

            return RedirectToAction("ManageApplications");
        }

        [HttpPost]
        public async Task<IActionResult> UpdateEmployerNotes(string cvId, string notes)
        {
            if (string.IsNullOrEmpty(cvId))
            {
                return BadRequest("CV ID is required");
            }

            var userCv = await _context.UserCVs.FindAsync(cvId);
            if (userCv == null)
            {
                return NotFound("CV not found");
            }

            userCv.EmployerNotes = notes;
            await _context.SaveChangesAsync();

            return RedirectToAction("ApplicationDetails", new { cvId });
        }

        [HttpGet]
        public async Task<IActionResult> EditProfile()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            // Kiểm tra xem người dùng có phải là employer không
            if (!await _userManager.IsInRoleAsync(user, "Employer"))
            {
                return Forbid();
            }

            var employer = await _employersService.GetEmployerByUserId(user.Id);
            if (employer == null)
            {
                return NotFound("Employer profile not found");
            }

            await PrepareViewBagCities(employer.CityId);

            var viewModel = new EditCompanyProfileViewModel
            {
                EmployerId = employer.EmployerId,
                CompanyName = employer.CompanyName,
                Industry = employer.Field,
                Email = employer.Email,
                Phone = user.PhoneNumber,
                Website = employer.Website,
                Description = employer.Description,
                CurrentLogoUrl = employer.LogoURL,
                CityId = employer.CityId
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProfile(EditCompanyProfileViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            // Kiểm tra quyền truy cập
            if (!await _userManager.IsInRoleAsync(user, "Employer"))
            {
                return Forbid();
            }

            var employer = await _employersService.GetEmployerByUserId(user.Id);
            if (employer == null)
            {
                return NotFound("Employer profile not found");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Xử lý upload logo nếu có
                    if (model.LogoFile != null && model.LogoFile.Length > 0)
                    {
                        _logger.LogInformation("Attempting to upload logo file: {FileName}, Size: {FileSize}",
                            model.LogoFile.FileName, model.LogoFile.Length);

                        // Kiểm tra kích thước file (tối đa 2MB)
                        if (model.LogoFile.Length > 2 * 1024 * 1024)
                        {
                            ModelState.AddModelError("LogoFile", "Kích thước file không được vượt quá 2MB");
                            await PrepareViewBagCities(model.CityId);
                            return View(model);
                        }

                        // Kiểm tra định dạng file
                        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                        var fileExtension = Path.GetExtension(model.LogoFile.FileName).ToLowerInvariant();
                        if (!allowedExtensions.Contains(fileExtension))
                        {
                            ModelState.AddModelError("LogoFile", "Chỉ chấp nhận các định dạng: JPG, JPEG, PNG, GIF");
                            await PrepareViewBagCities(model.CityId);
                            return View(model);
                        }

                        var logoFileName = await UploadFile(model.LogoFile, "images/companies-logo");
                        if (!string.IsNullOrEmpty(logoFileName))
                        {
                            employer.LogoURL = logoFileName;
                            _logger.LogInformation("Logo URL updated for employer {EmployerId}: {LogoURL}",
                                employer.EmployerId, logoFileName);
                        }
                        else
                        {
                            _logger.LogWarning("Failed to upload logo for employer {EmployerId}", employer.EmployerId);
                        }
                    }

                    // Cập nhật thông tin công ty
                    employer.CompanyName = model.CompanyName;
                    employer.Field = model.Industry;
                    employer.Email = model.Email;
                    employer.Website = model.Website;
                    user.PhoneNumber = model.Phone;
                    employer.Description = model.Description;
                    employer.CityId = model.CityId;

                    // Cập nhật thông tin user
                    await _userManager.UpdateAsync(user);

                    // Cập nhật thông tin employer
                    var result = await _employersService.UpdateEmployer(employer);
                    _logger.LogInformation("Employer update result: {Result} for EmployerId: {EmployerId}, Website: {Website}, LogoURL: {LogoURL}",
                        result, employer.EmployerId, employer.Website, employer.LogoURL);

                    TempData["SuccessMessage"] = "Cập nhật thông tin công ty thành công!";
                    return RedirectToAction(nameof(EditProfile));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error updating employer profile");
                    ModelState.AddModelError("", "Có lỗi xảy ra khi cập nhật thông tin. Vui lòng thử lại.");
                    TempData["ErrorMessage"] = "Có lỗi xảy ra khi cập nhật thông tin. Vui lòng thử lại sau.";
                }
            }
            else
            {
                // Nếu ModelState không hợp lệ, log lỗi để debug
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                _logger.LogWarning("ModelState errors: {Errors}", string.Join(", ", errors));
            }

            await PrepareViewBagCities(model.CityId);

            return View(model);
        }

        private async Task PrepareViewBagCities(int cityId)
        {
            var cities = await _context.Cities.ToListAsync();
            ViewBag.Cities = new SelectList(cities, "CityId", "CityName", cityId);
        }

        private async Task<string> UploadFile(IFormFile file, string folderPath)
        {
            if (file == null || file.Length == 0)
                return null;

            try
            {
                // Đảm bảo thư mục tồn tại
                var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, folderPath);
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                // Tạo tên file duy nhất
                var fileExtension = Path.GetExtension(file.FileName);
                var uniqueFileName = Guid.NewGuid().ToString() + fileExtension;
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                // Lưu file
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(fileStream);
                }

                // Log thông tin để debug
                _logger.LogInformation("File uploaded successfully: {FilePath}", filePath);

                // Trả về đường dẫn tương đối
                return "/" + folderPath.Replace("\\", "/") + "/" + uniqueFileName;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading file: {FileName}, Error: {ErrorMessage}", file.FileName, ex.Message);
                return null;
            }
        }

        public async Task<IActionResult> ManagePosts()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account");
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Kiểm tra quyền truy cập
            var isEmployer = await _userManager.IsInRoleAsync(user, "Employer");
            if (!isEmployer)
            {
                return RedirectToAction("Register", "EmployerRegistration");
            }

            var employer = await _context.Employers
                .FirstOrDefaultAsync(e => e.UserId == user.Id);

            if (employer == null)
            {
                return RedirectToAction("Register", "EmployerRegistration");
            }

            var posts = await _context.Posts
                .Where(p => p.EmployerId == employer.EmployerId)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            return View(posts);
        }

        public async Task<IActionResult> ViewApplications(string postId)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account");
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Kiểm tra quyền truy cập
            var isEmployer = await _userManager.IsInRoleAsync(user, "Employer");
            if (!isEmployer)
            {
                return RedirectToAction("Register", "EmployerRegistration");
            }

            var employer = await _context.Employers
                .FirstOrDefaultAsync(e => e.UserId == user.Id);

            if (employer == null)
            {
                return RedirectToAction("Register", "EmployerRegistration");
            }

            // Kiểm tra xem bài đăng có thuộc về nhà tuyển dụng này không
            var post = await _context.Posts
                .FirstOrDefaultAsync(p => p.PostId.ToString() == postId && p.EmployerId == employer.EmployerId);

            if (post == null)
            {
                return NotFound();
            }

            var applications = await _context.UserCVs
                .Include(cv => cv.Users)
                .Where(cv => cv.PostId == post.PostId)
                .OrderByDescending(cv => cv.AppliedDate)
                .ToListAsync();

            ViewBag.PostTitle = post.Title;
            ViewBag.PostId = post.PostId;

            return View(applications);
        }
    }
}
