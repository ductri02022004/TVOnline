using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using TVOnline.Data;
using TVOnline.Models;
using TVOnline.Service.DTO;
using TVOnline.Service.Employers;
using TVOnline.Service.Location;
using TVOnline.Service.Post;
using TVOnline.ViewModels.Employer;

namespace TVOnline.Controllers.Employer
{
    [Route("[controller]")]
    public class EmployersPage(UserManager<Users> userManager, AppDbContext context, ILocationService locationService,
        IEmployersService employersService, IPostService postService) : Controller
    {
        private readonly UserManager<Users> _userManager = userManager;
        private readonly AppDbContext _context = context;
        private readonly ILocationService _locationService = locationService;
        private readonly IEmployersService _employersService = employersService;
        private readonly IPostService _postService = postService;

        [Route("[action]")]
        public async Task<IActionResult> Index(string companyName = "", string field = "", string location = "", int page = 1)
        {
            // Get all cities and fields for the dropdowns
            var cities = await _locationService.GetAllCities();
            var fields = await _employersService.GetAllUniqueFields();

            // Search for employers based on criteria
            var employers = string.IsNullOrEmpty(companyName) && string.IsNullOrEmpty(field) && string.IsNullOrEmpty(location)
                ? await _employersService.GetAllEmployers()
                : await _employersService.SearchEmployers(companyName, field, location);

            int pageSize = 6;
            int totalEmployers = employers.Count();
            int totalPages = (int)Math.Ceiling((double)totalEmployers / pageSize);

            var pagedEmployersList = employers.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            var companiesViewModel = new CompaniesListViewModel()
            {
                Employers = pagedEmployersList,
                Cities = cities,
                Fields = fields,
                SearchCompanyName = companyName,
                SearchField = field,
                SearchLocation = location
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
            
            // Lấy danh sách công việc của nhà tuyển dụng
            var jobs = await _context.Posts
                .Where(p => p.EmployerId == employerId && p.IsActive)
                .OrderByDescending(p => p.CreatedAt)
                .Take(5)
                .ToListAsync();
                
            ViewBag.Jobs = jobs;
            
            return View("Details", employer);
        }

        /// <summary>
        /// Hiển thị tất cả công việc của một nhà tuyển dụng
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> ViewAllJobsByEmployers(
            string employerId,
            string sortOrder = "newest",
            string searchKeyword = "",
            List<string>? selectedJobTypes = null,
            List<string>? selectedLocations = null,
            List<string>? selectedExperiences = null,
            int minSalary = 0,
            int maxSalary = 50,
            int page = 1)
        {
            if (string.IsNullOrEmpty(employerId))
            {
                return RedirectToAction("Index", "Home");
            }

            // Get employer info
            EmployerResponse? employer = await _employersService.GetEmployerById(employerId);
            if (employer == null)
            {
                return NotFound();
            }

            // Get jobs from this employer
            var jobs = await _postService.GetPostsByEmployer(employerId);

            // Filter by search keyword
            if (!string.IsNullOrEmpty(searchKeyword))
            {
                var searchLower = searchKeyword.ToLower();
                jobs = jobs.Where(job =>
                    job.CityName != null && (job.Title.ToLower().Contains(searchLower) ||
                                             job.Title.ToLower().Contains(searchLower) ||
                                             job.JobType.ToLower().Contains(searchLower) ||
                                             job.CityName.ToLower().Contains(searchLower))
                ).ToList();
            }

            // Filter by job types
            if (selectedJobTypes != null && selectedJobTypes.Any())
            {
                jobs = jobs.Where(job => selectedJobTypes.Contains(job.JobType)).ToList();
            }

            // Filter by locations
            if (selectedLocations != null && selectedLocations.Any())
            {
                jobs = jobs.Where(job => selectedLocations.Contains(job.CityName)).ToList();
            }

            // Filter by experience levels
            if (selectedExperiences != null && selectedExperiences.Any())
            {
                jobs = jobs.Where(job => selectedExperiences.Contains(job.Experience)).ToList();
            }

            // Filter by salary range
            jobs = jobs.Where(job =>
                job.Salary >= minSalary * 1000000 &&
                (job.Salary <= maxSalary * 1000000 || (maxSalary == 50 * 1000000 && job.Salary > 50 * 1000000))
            ).ToList();

            // Apply sorting
            switch (sortOrder)
            {
                case "newest":
                    jobs = jobs.OrderByDescending(j => j.CreatedAt).ToList();
                    break;
                case "oldest":
                    jobs = jobs.OrderBy(j => j.CreatedAt).ToList();
                    break;
                case "salary_high":
                    jobs = jobs.OrderByDescending(j => j.Salary).ToList();
                    break;
                case "salary_low":
                    jobs = jobs.OrderBy(j => j.Salary).ToList();
                    break;
                case "az":
                    jobs = jobs.OrderBy(j => j.Title).ToList();
                    break;
                case "za":
                    jobs = jobs.OrderByDescending(j => j.Title).ToList();
                    break;
                default:
                    jobs = jobs.OrderByDescending(j => j.CreatedAt).ToList();
                    break;
            }

            // Save original count for display
            int totalPosts = jobs.Count;

            // Pagination
            int pageSize = 5;
            int totalPages = (int)Math.Ceiling(jobs.Count / (double)pageSize);

            // Ensure page is within valid range
            page = Math.Max(1, Math.Min(page, Math.Max(1, totalPages)));

            // Get jobs for the current page
            jobs = jobs.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            // Calculate visible page numbers
            List<int> pageNumbers = new List<int>();
            int maxPagesToShow = 5;
            int startPage = Math.Max(1, page - (maxPagesToShow / 2));
            int endPage = Math.Min(totalPages, startPage + maxPagesToShow - 1);

            if (endPage - startPage + 1 < maxPagesToShow && startPage > 1)
            {
                startPage = Math.Max(1, endPage - maxPagesToShow + 1);
            }

            for (int i = startPage; i <= endPage; i++)
            {
                pageNumbers.Add(i);
            }

            // Get unique job types, locations, and experience levels for filtering
            var allJobsForFilters = await _postService.GetPostsByEmployer(employerId);
            var jobTypes = allJobsForFilters.Select(j => j.JobType).Distinct().ToList();
            var locations = allJobsForFilters.Select(j => j.CityName).Distinct().ToList();
            var experienceLevels = allJobsForFilters.Select(j => j.Experience).Distinct().ToList();

            // Check if any jobs are saved by current user
            if (User.Identity.IsAuthenticated)
            {
                string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                foreach (var job in jobs)
                {
                    job.IsSaved = await _postService.IsJobSavedByUser(job.PostId, userId);
                }
            }

            // Set view bag for filtering
            ViewBag.EmployerId = employerId;
            ViewBag.SearchKeyword = searchKeyword;
            ViewBag.SortOrder = sortOrder;
            ViewBag.TotalPosts = totalPosts;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.PageNumbers = pageNumbers;
            ViewBag.SelectedJobTypes = selectedJobTypes;
            ViewBag.SelectedLocations = selectedLocations;
            ViewBag.SelectedExperiences = selectedExperiences;
            ViewBag.MinSalary = minSalary;
            ViewBag.MaxSalary = maxSalary;

            return View(jobs);
        }

        [HttpPost]
        [Route("[action]/{postId}")]
        public async Task<IActionResult> SaveJob(string postId, string employerId, int page = 1, string searchKeyword = "", string sortOrder = "")
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

            await _postService.SaveJobForJobSeeker(postId, userId);

            return RedirectToAction("ViewAllJobsByEmployers", new
            {
                employerId,
                searchKeyword,
                sortOrder,
                page
            });
        }

        [HttpPost]
        [Route("[action]/{postId}")]
        public async Task<IActionResult> UnsaveJob(string postId, string employerId, int page = 1, string searchKeyword = "", string sortOrder = "")
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

            await _postService.DeleteSavedJobForJobSeeker(postId, userId);

            return RedirectToAction("ViewAllJobsByEmployers", new
            {
                employerId,
                searchKeyword,
                sortOrder,
                page
            });
        }
    }
}
