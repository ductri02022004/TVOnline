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

namespace TVOnline.Controllers
{
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
            return View("JobDetails", post);
        }

        [HttpPost]
        [Route("[action]/{postID}")]
        public async Task<IActionResult> Apply(IFormFile cvFile, string postId)
        {
            if (cvFile is { Length: > 0 })
            {
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads", cvFile.FileName);
                var post = await _postService.FindPostById(postId);

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
                var userCvAddRequest = new UserCvAddRequest()
                {
                    CvId = Guid.NewGuid().ToString(),
                    CvFileUrl = cvFile.FileName,
                    CvStatus = "Applied",
                    UserId = user.Id,
                    PostId = postId,
                };

                 await _userCvService.SaveCv(userCvAddRequest);

                TempData["SuccessMessage"] = "Ứng tuyển thành công!";
                return RedirectToAction("Details", new { id = postId });
            }

            TempData["ErrorMessage"] = "Vui lòng tải lên CV của bạn.";
            return RedirectToAction("Details", new { id = postId });
        }
    }
}