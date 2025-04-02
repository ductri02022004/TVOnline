using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using TVOnline.Data;
using TVOnline.Models;
using TVOnline.Models.Error;
using TVOnline.Service.Location;
using TVOnline.Service.Post;
using TVOnline.ViewModels.Home;
using TVOnline.ViewModels.JobsViewModel;

namespace TVOnline.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly UserManager<Users> _userManager;
        private readonly AppDbContext _context;
        private readonly IPostService _postService;
        private readonly ILocationService _locationService;

        public HomeController(
            ILogger<HomeController> logger,
            UserManager<Users> userManager,
            AppDbContext context, IPostService postService, ILocationService locationService)
        {
            _logger = logger;
            _userManager = userManager;
            _context = context;
            _postService = postService;
            _locationService = locationService;
        }

        public async Task<IActionResult> Index()
        {
            var posts = await _postService.GetSeveralPosts(6);
            var locations = await _locationService.GetAllCities();

            // Find admin user for chat widget
            var adminUser = await _userManager.FindByEmailAsync("admin@tvonline.com");
            if (adminUser != null)
            {
                ViewBag.AdminId = adminUser.Id;
            }
            else
            {
                _logger.LogWarning("Admin user not found. Chat functionality may not work properly.");
            }

            var homeViewModel = new HomeIndexViewModel
            {
                Posts = posts,
                Locations = locations
            };
            return View(homeViewModel);
        }

        public async Task<IActionResult> SearchJobs(string keyword, int? cityId, int page = 1)
        {
            const int PageSize = 10;
            var posts = await _postService.SearchPosts(keyword, cityId, page, PageSize);
            var totalPosts = await _postService.CountSearchPosts(keyword, cityId);
            var locations = await _locationService.GetAllCities();

            var viewModel = new JobsViewModel
            {
                Posts = posts,
                Locations = locations,
                SearchKeyword = keyword ?? string.Empty
            };

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling(totalPosts / (double)PageSize);

            return View("~/Views/ApplyJob/Index.cshtml", viewModel);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public async Task<IActionResult> BecomeEmployer()
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

            // Kiểm tra xem người dùng đã là nhà tuyển dụng chưa
            var existingEmployer = await _context.Employers.FirstOrDefaultAsync(e => e.UserId == user.Id);
            if (existingEmployer != null)
            {
                // Nếu đã là employer, chuyển thẳng đến dashboard
                return RedirectToAction("Index", "EmployerDashboard");
            }

            // Nếu chưa là employer, chuyển đến trang đăng ký
            return RedirectToAction("Register", "EmployerRegistration");
        }
    }
}
