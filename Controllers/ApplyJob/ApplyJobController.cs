using Microsoft.AspNetCore.Mvc;
using TVOnline.Models;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using System.IO;
using System;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TVOnline.Service.Post;
using TVOnline.ViewModels.Post;
using TVOnline.Data;
using TVOnline.Service.DTO;
using TVOnline.Service.Location;
using TVOnline.Service.UserCVs;
using TVOnline.ViewModels.JobsViewModel;
using Microsoft.AspNetCore.Authorization;

namespace TVOnline.Controllers {
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
            var posts = await _postService.GetAllPosts();
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
            
            // Check if user is logged in to show application status
            if (User.Identity.IsAuthenticated)
            {
                var user = await _userManager.GetUserAsync(User);
                if (user != null)
                {
                    // Check if user has already applied to this job
                    var existingApplication = await _userCvService.GetApplicationByUserAndPost(user.Id, postID);
                    if (existingApplication != null)
                    {
                        ViewBag.HasApplied = true;
                        ViewBag.ApplicationStatus = existingApplication.CVStatus;
                    }
                    else
                    {
                        ViewBag.HasApplied = false;
                    }
                }
            }
            
            return View("JobDetails", post);
        }

        [HttpPost]
        [Route("[action]/{postID}")]
        [Authorize]
        public async Task<IActionResult> Apply(IFormFile cvFile, string postId) {
            if (cvFile is { Length: > 0 }) {
                var user = await _userManager.GetUserAsync(User);
                if (user == null) {
                    TempData["ErrorMessage"] = "Bạn cần đăng nhập để ứng tuyển.";
                    return RedirectToAction("Details", new { postId = postId });
                }
                
                // Check if user has already applied to this job
                var existingApplication = await _userCvService.GetApplicationByUserAndPost(user.Id, postId);
                if (existingApplication != null)
                {
                    TempData["ErrorMessage"] = "Bạn đã ứng tuyển vào vị trí này rồi.";
                    return RedirectToAction("Details", new { postId = postId });
                }
                
                // Get post details to validate
                var post = await _postService.FindPostById(postId);
                if (post == null)
                {
                    TempData["ErrorMessage"] = "Không tìm thấy vị trí công việc.";
                    return RedirectToAction("Index");
                }

                // Ensure uploads directory exists
                var uploadsDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads");
                if (!Directory.Exists(uploadsDir))
                {
                    Directory.CreateDirectory(uploadsDir);
                }
                
                // Create a unique filename to prevent overwriting
                var fileName = $"{user.Id}_{DateTime.Now:yyyyMMddHHmmss}_{Path.GetFileName(cvFile.FileName)}";
                var filePath = Path.Combine(uploadsDir, fileName);

                await using (var stream = new FileStream(filePath, FileMode.Create)) {
                    await cvFile.CopyToAsync(stream);
                }

                // Lưu thông tin ứng tuyển vào cơ sở dữ liệu
                var userCvAddRequest = new UserCvAddRequest()
                {
                    CvId = Guid.NewGuid().ToString(),
                    CvFileUrl = fileName,
                    CvStatus = "Applied", // Trạng thái mặc định khi mới nộp
                    UserId = user.Id,
                    PostId = postId,
                };

                await _userCvService.SaveCv(userCvAddRequest);

                TempData["SuccessMessage"] = "Ứng tuyển thành công! Nhà tuyển dụng sẽ xem xét hồ sơ của bạn.";
                return RedirectToAction("Details", new { postId = postId });
            }

            TempData["ErrorMessage"] = "Vui lòng tải lên CV của bạn.";
            return RedirectToAction("Details", new { postId = postId });
        }
        
        [Authorize]
        [Route("[action]")]
        public async Task<IActionResult> MyApplications()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }
            
            var applications = await _userCvService.GetApplicationsByUser(user.Id);
            return View(applications);
        }
    }
}