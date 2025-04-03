using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TVOnline.Models;
using TVOnline.Services;

namespace TVOnline.Controllers.Account
{
    public class AccountStatusController : Controller
    {
        private readonly UserManager<Users> _userManager;
        private readonly IPremiumUserService _premiumUserService;
        private readonly ILogger<AccountStatusController> _logger;

        public AccountStatusController(
            UserManager<Users> userManager,
            IPremiumUserService premiumUserService,
            ILogger<AccountStatusController> logger)
        {
            _userManager = userManager;
            _premiumUserService = premiumUserService;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    _logger.LogWarning("User not found when accessing account status");
                    return RedirectToAction("Login", "Account");
                }

                var isPremium = await _premiumUserService.IsUserPremium(user.Id);

                var viewModel = new AccountStatusViewModel
                {
                    UserName = user.UserName,
                    Email = user.Email,
                    IsPremium = isPremium
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while accessing account status");
                return View("Error");
            }
        }
    }
}