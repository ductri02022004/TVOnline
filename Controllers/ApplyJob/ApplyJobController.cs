using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TVOnline.Models;
using TVOnline.Service.DTO;
using TVOnline.Service.Location;
using TVOnline.Service.Post;
using TVOnline.Service.UserCVs;
using TVOnline.ViewModels.JobsViewModel;

namespace TVOnline.Controllers.ApplyJob
{
    [Route("[controller]")]
    public class ApplyJobController(IUserCvService userCvService, IPostService postService, UserManager<Users> userManager, ILocationService locationService) : Controller
    {
        private readonly IUserCvService _userCvService = userCvService;
        private readonly IPostService _postService = postService;
        private readonly ILocationService _locationService = locationService;
        private readonly UserManager<Users> _userManager = userManager;

        [Route("[action]")]
        public async Task<IActionResult> Index(int page = 1)
        {
            Users? user = await _userManager.GetUserAsync(User);


            var posts = await _postService.GetAllPosts(user?.Id);
            var cities = await _locationService.GetAllCities();

            int pageSize = 5;
            int totalPosts = posts.Count();
            int totalPages = (int)Math.Ceiling((double)totalPosts / pageSize);

            var pagedPosts = posts.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            var jobsViewModel = new JobsViewModel
            {
                Posts = pagedPosts,
                Locations = cities
            };

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalPosts = totalPosts;

            return View("Details", jobsViewModel);
        }

        [HttpGet]
        [Route("[action]")]
        public async Task<IActionResult> Filter(string keyword, int? cityId, decimal? minSalary, decimal? maxSalary,
            int? minExperience, int? maxExperience, int page = 1)
        {
            Users? user = await _userManager.GetUserAsync(User);
            var cities = await _locationService.GetAllCities();

            int pageSize = 5;

            // Get filtered posts
            var posts = await _postService.FilterPosts(
                keyword, cityId, minSalary, maxSalary, minExperience, maxExperience, page, pageSize, user?.Id);

            // Count total posts for pagination
            var totalPosts = await _postService.CountFilteredPosts(
                keyword, cityId, minSalary, maxSalary, minExperience, maxExperience);

            int totalPages = (int)Math.Ceiling((double)totalPosts / pageSize);

            var jobsViewModel = new JobsViewModel
            {
                Posts = posts,
                Locations = cities,
                SearchKeyword = keyword,
                MinSalary = minSalary,
                MaxSalary = maxSalary,
                MinExperience = minExperience,
                MaxExperience = maxExperience,
                SelectedCityId = cityId
            };

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalPosts = totalPosts;

            // Return the same view as Index but with filtered data
            return View("Details", jobsViewModel);
        }

        [Route("[action]/{postID}")]
        public async Task<IActionResult> Details(string postID)
        {
            var post = await _postService.FindPostById(postID);

            // Check if user is logged in to show application status
            if (User.Identity.IsAuthenticated)
            {
                var user = await _userManager.GetUserAsync(User);
                if (user != null)
                {
                    // Check if user has already applied to this job
                    var existingApplication = await _userCvService.GetApplicationByUserAndPost(user.Id, postID);
                    if (existingApplication != null)
                    {
                        ViewBag.HasApplied = true;
                        ViewBag.ApplicationStatus = existingApplication.CVStatus;
                    }
                    else
                    {
                        ViewBag.HasApplied = false;
                    }
                }
            }

            var viewModel = new ViewModels.Post.PostDetailViewModel
            {
                Post = post,
                CurrentUser = User.Identity.IsAuthenticated ? await _userManager.GetUserAsync(User) : null
            };

            return View("JobDetails", viewModel);
        }

        [HttpPost]
        [Route("[action]/{postID}")]
        [Authorize]
        public async Task<IActionResult> Apply(IFormFile cvFile, string postId)
        {
            if (cvFile is { Length: > 0 })
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    TempData["ErrorMessage"] = "Bạn cần đăng nhập để ứng tuyển.";
                    return RedirectToAction("Details", new { postId });
                }

                // Check if user has already applied to this job
                var existingApplication = await _userCvService.GetApplicationByUserAndPost(user.Id, postId);
                if (existingApplication != null)
                {
                    TempData["ErrorMessage"] = "Bạn đã ứng tuyển vào vị trí này rồi.";
                    return RedirectToAction("Details", new { postId });
                }

                // Get post details to validate
                var post = await _postService.FindPostById(postId);
                if (post == null)
                {
                    TempData["ErrorMessage"] = "Không tìm thấy vị trí công việc.";
                    return RedirectToAction("Index");
                }

                // Ensure uploads directory exists
                var uploadsDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads");
                if (!Directory.Exists(uploadsDir))
                {
                    Directory.CreateDirectory(uploadsDir);
                }

                // Create a unique filename to prevent overwriting
                var fileName = $"{user.Id}_{DateTime.Now:yyyyMMddHHmmss}_{Path.GetFileName(cvFile.FileName)}";
                var filePath = Path.Combine(uploadsDir, fileName);

                await using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await cvFile.CopyToAsync(stream);
                }

                // Lưu thông tin ứng tuyển vào cơ sở dữ liệu
                var userCvAddRequest = new UserCvAddRequest()
                {
                    CvId = Guid.NewGuid().ToString(),
                    CvFileUrl = fileName,
                    CvStatus = "Applied", // Trạng thái mặc định khi mới nộp
                    UserId = user.Id,
                    PostId = postId,
                    ApplicationDate = DateTime.Now
                };

                await _userCvService.SaveCv(userCvAddRequest);

                TempData["SuccessMessage"] = "Ứng tuyển thành công! Nhà tuyển dụng sẽ xem xét hồ sơ của bạn.";
                return RedirectToAction("Details", new { postId });
            }

            TempData["ErrorMessage"] = "Vui lòng tải lên CV của bạn.";
            return RedirectToAction("Details", new { postId });
        }

        [Authorize]
        [Route("[action]")]
        public async Task<IActionResult> MyApplications()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var applications = await _userCvService.GetApplicationsByUser(user.Id);
            return View(applications);
        }

        [HttpPost]
        [Route("[action]/{postId}")] // Route for saving a job, now in ApplyJobController (e.g., /ApplyJob/SaveJob/POST001)
        public async Task<IActionResult> SaveJob(string postId, int returnPage = 1, string keyword = null, int? cityId = null,
            decimal? minSalary = null, decimal? maxSalary = null, int? minExperience = null, int? maxExperience = null)
        {
            if (string.IsNullOrEmpty(postId))
            {
                return BadRequest("Post ID is required.");
            }

            // Check if user is authenticated
            if (!User.Identity.IsAuthenticated)
            {
                // Store the post ID in TempData to potentially use it after login
                TempData["PostToSave"] = postId;
                return RedirectToAction("Login", "Account");
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }

            bool isSaved = await _postService.SaveJobForJobSeeker(postId, userId);

            // Redirect based on whether we have filter parameters
            if (!string.IsNullOrEmpty(keyword) || cityId.HasValue || minSalary.HasValue ||
                maxSalary.HasValue || minExperience.HasValue || maxExperience.HasValue)
            {
                return RedirectToAction("Filter", new
                {
                    keyword,
                    cityId,
                    minSalary,
                    maxSalary,
                    minExperience,
                    maxExperience,
                    page = returnPage
                });
            }

            return RedirectToAction("Index", new { page = returnPage });
        }

        [HttpPost]
        [Route("[action]/{postId}")]
        public async Task<IActionResult> UnsaveJob(string postId, int returnPage = 1, string keyword = null, int? cityId = null,
            decimal? minSalary = null, decimal? maxSalary = null, int? minExperience = null, int? maxExperience = null)
        {
            if (string.IsNullOrEmpty(postId))
            {
                return BadRequest("Post ID is required.");
            }

            // Check if user is authenticated
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account");
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }

            bool isUnsaved = await _postService.DeleteSavedJobForJobSeeker(postId, userId);

            // Redirect based on whether we have filter parameters
            if (!string.IsNullOrEmpty(keyword) || cityId.HasValue || minSalary.HasValue ||
                maxSalary.HasValue || minExperience.HasValue || maxExperience.HasValue)
            {
                return RedirectToAction("Filter", new
                {
                    keyword,
                    cityId,
                    minSalary,
                    maxSalary,
                    minExperience,
                    maxExperience,
                    page = returnPage
                });
            }

            return RedirectToAction("Index", new { page = returnPage });
        }
    }
}