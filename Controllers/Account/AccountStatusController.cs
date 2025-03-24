using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using TVOnline.Models;
using Microsoft.EntityFrameworkCore;
using TVOnline.Data;

namespace TVOnline.Controllers
{
    public class AccountStatusController : Controller
    {
        private readonly UserManager<Users> _userManager;
        private readonly AppDbContext _context;

        public AccountStatusController(UserManager<Users> userManager, AppDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var premiumUser = await _context.PremiumUsers
                .FirstOrDefaultAsync(p => p.UserId == user.Id);

            var viewModel = new AccountStatusViewModel
            {
                UserName = user.UserName,
                Email = user.Email,
                IsPremium = premiumUser != null,
                PremiumUserId = premiumUser?.PremiumUserId
            };

            return View(viewModel);
        }
    }
} 