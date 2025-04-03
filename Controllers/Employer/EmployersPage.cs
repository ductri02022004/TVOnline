using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TVOnline.Data;
using TVOnline.Models;
using TVOnline.Service.Employers;
using TVOnline.Service.Location;
using TVOnline.ViewModels.Employer;

namespace TVOnline.Controllers.Employer
{
    [Route("[controller]")]
    public class EmployersPage(UserManager<Users> userManager, AppDbContext context, ILocationService locationService, IEmployersService employersService) : Controller
    {
        private readonly UserManager<Users> _userManager = userManager;
        private readonly AppDbContext _context = context;
        private readonly ILocationService _locationService = locationService;
        private readonly IEmployersService _employersService = employersService;

        public async Task<IActionResult> Index(int page = 1)
        {
            var employers = await _employersService.GetAllEmployers();
            var cities = await _locationService.GetAllCities();

            int pageSize = 6;
            int totalEmployers = employers.Count();
            int totalPages = (int)Math.Ceiling((double)totalEmployers / pageSize);

            var pagedEmployersList = employers.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            var companiesViewModel = new CompaniesListViewModel()
            {
                Employers = pagedEmployersList,
                Cities = cities
            };

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalEmployers = totalEmployers;
            return View(companiesViewModel);
        }

        [Route("[action]/{employerId}")]
        public async Task<IActionResult> ViewEmployerDetail(string employerId)
        {
            var employer = await _employersService.GetEmployerById(employerId);
            return View("Details", employer);
        }
    }
}
