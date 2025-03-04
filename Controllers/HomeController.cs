using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using TVOnline.Models;
using System.Collections.Generic;
using TVOnline.Models.Error;
using Microsoft.AspNetCore.Identity;
using TVOnline.Data;
using Microsoft.EntityFrameworkCore;
using TVOnline.Service.Location;
using TVOnline.Service.Post;
using TVOnline.ViewModels.Home;
using TVOnline.ViewModels.Post;


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

            var homeViewModel = new HomeIndexViewModel
            {
                Posts = posts,
                Locations = locations
            };
            return View(homeViewModel);
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
