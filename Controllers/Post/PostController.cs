using Microsoft.AspNetCore.Mvc;
using TVOnline.Models;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using System.IO;
using System;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TVOnline.Repository.UserCVs;
using TVOnline.Service.Post;
using TVOnline.ViewModels.Post;
using TVOnline.Data;

namespace TVOnline.Controllers
{
    [Route("[controller]")]
    public class PostController(IUserCvService userCvService, IPostService postService, UserManager<Users> userManager, AppDbContext context) : Controller
    {
        private readonly IUserCvService _userCvService = userCvService;
        private readonly IPostService _postService = postService;
        private readonly UserManager<Users> _userManager = userManager;
        private readonly AppDbContext _context = context;

        [Route("[action]")]
        public async Task<IActionResult> Index(int page = 1)
        {
            var posts = await _context.Posts
            .Include(p => p.Employer)
            .Include(p => p.City)
            .OrderByDescending(p => p.CreatedAt)
            .Select(p => new PostListViewModel
            {
                PostId = p.PostId.ToString(),
                Title = p.Title,
                CompanyName = p.Employer.CompanyName,
                Location = p.City.CityName,
                Salary = p.Salary,
                JobType = p.JobType,
                Experience = p.Experience,
                CreatedAt = p.CreatedAt
            }).ToListAsync();

            int pageSize = 5;
            int totalPosts = posts.Count();
            int totalPages = (int)Math.Ceiling((double)totalPosts / pageSize);

            var pagedPosts = posts.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalPosts = totalPosts;

            return View("Details", pagedPosts);
        }

        [Route("[action]/{postID}")]
        public async Task<IActionResult> Details(string postID)
        {
            var posts = await _context.Posts
                .Include(p => p.Employer)
                .Include(p => p.City)
                .OrderByDescending(p => p.CreatedAt)
                .Select(p => new PostListViewModel
                {
                    PostId = p.PostId.ToString(),
                    Title = p.Title,
                    CompanyName = p.Employer.CompanyName,
                    Location = p.City.CityName,
                    Salary = p.Salary,
                    JobType = p.JobType,
                    Experience = p.Experience,
                    Description = p.Description,
                    Requirements = p.Requirements,
                    Benefits = p.Benefits,
                    CreatedAt = p.CreatedAt
                }).ToListAsync();

            var post = posts.FirstOrDefault(p => p.PostId.ToString() == postID);
            if (post == null)
            {
                return NotFound();
            }
            return View("JobDetails", post);
        }

        [HttpPost]
        [Route("[action]/{postID}")]
        public async Task<IActionResult> Apply(IFormFile cvFile, string postId)
        {
            if (cvFile is { Length: > 0 })
            {
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads", cvFile.FileName);

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
                var userCv = new UserCV
                {
                    CvFile = cvFile,
                    CVFileUrl = cvFile.FileName,
                    CVStatus = "Applied",
                    Users = user,
                    UserId = user.Id
                };

                await _userCvService.SaveCv(userCv);

                TempData["SuccessMessage"] = "Ứng tuyển thành công!";
                return RedirectToAction("Details", new { id = postId });
            }

            TempData["ErrorMessage"] = "Vui lòng tải lên CV của bạn.";
            return RedirectToAction("Details", new { id = postId });
        }
    }
}