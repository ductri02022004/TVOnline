using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TVOnline.Data;
using TVOnline.Models;

namespace TVOnline.Controllers.Employers
{
    [Route("[controller]")]
    public class EmployersController(UserManager<Users> userManager, AppDbContext context) : Controller
    {
        private readonly UserManager<Users> _userManager = userManager;
        private readonly AppDbContext _context = context;
        public async Task<IActionResult> Index()
        {
            var employers = await _context.Employers.Include(em => em.City).Include(em => em.User).ToListAsync();
            return View(employers);
        }

        [Route("[action]/{employerId}")]
        public async Task<IActionResult> ViewEmployerDetail(string employerId)
        {
            var employer = await _context.Employers.Include(em => em.City).Include(em => em.City!.Zone).Include(em => em.User).FirstOrDefaultAsync(emp => emp.EmployerId == employerId);
            return View("Details", employer);
        }
    }
}
