using Microsoft.AspNetCore.Mvc;
using TVOnline.Services;
using Microsoft.AspNetCore.Authorization;

namespace TVOnline.Areas.Premium.Controllers
{
    [Area("Premium")]
    [Authorize(Policy = "PremiumUser")]
    public class HomeController : Controller
    {
        private readonly IPremiumUserService _premiumUserService;

        public HomeController(IPremiumUserService premiumUserService)
        {
            _premiumUserService = premiumUserService;
        }

        public async Task<IActionResult> Index()
        {
            return View();
        }
    }
} 