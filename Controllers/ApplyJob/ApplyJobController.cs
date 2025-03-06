using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using TVOnline.ViewModels.Post;

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
            var posts = await _postService.GetAllPosts(user.Id);
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

        [Route("[action]/{postID}")]
        public async Task<IActionResult> Details(string postID)
        {
            var post = await _postService.FindPostById(postID);
            Users? user = await _userManager.GetUserAsync(User);

            var postDetailViewModel = new PostDetailViewModel
            {
                Post = post,
                CurrentUser = user
            };

            return View("JobDetails", postDetailViewModel);
        }

        [HttpPost]
        [Route("[action]/{postID}")]
        public async Task<IActionResult> Apply(IFormFile cvFile, string postId)
        {
            if (cvFile is { Length: > 0 })
            {
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads", cvFile.FileName);
                var post = await _postService.FindPostById(postId);

                await using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await cvFile.CopyToAsync(stream);
                }

                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    TempData["ErrorMessage"] = "Bạn cần đăng nhập để ứng tuyển.";
                    return RedirectToAction("Details", new { postId = postId });
                }

                // Lưu thông tin ứng tuyển vào cơ sở dữ liệu hoặc xử lý thêm tại đây
                // Ví dụ: Lưu thông tin ứng viên và đường dẫn CV vào cơ sở dữ liệu
                var userCvAddRequest = new UserCvAddRequest()
                {
                    CvId = Guid.NewGuid().ToString(),
                    CvFileUrl = cvFile.FileName,
                    CvStatus = "Applied",
                    UserId = user.Id,
                    PostId = postId,
                    ApplicationDate = DateTime.Now
                };

                 await _userCvService.SaveCv(userCvAddRequest);

                TempData["SuccessMessage"] = "Ứng tuyển thành công!";
                return RedirectToAction("Details", new { id = postId });
            }

            TempData["ErrorMessage"] = "Vui lòng tải lên CV của bạn.";
            return RedirectToAction("Details", new { id = postId });
        }

        [HttpPost]
        [Route("[action]/{postId}")] // Route for saving a job, now in ApplyJobController (e.g., /ApplyJob/SaveJob/POST001)
        public async Task<IActionResult> SaveJob(string postId)
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

            bool isSaved = await _postService.SaveJobForJobSeeker(postId, userId);

            return isSaved
                ? RedirectToAction("Index", "ApplyJob")
                : BadRequest(new
                {
                    message = "Failed to save job or job already saved."
                });
        }

        [HttpPost]
        [Route("[action]/{postId}")]
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
                ? RedirectToAction("Index", "ApplyJob")
                : BadRequest(new { message = "Failed to unsave job." });
        }
    }
}