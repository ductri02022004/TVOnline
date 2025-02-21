using Microsoft.AspNetCore.Mvc;
using TVOnline.Models;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using System.IO;
using System;
using Microsoft.AspNetCore.Identity;
using TVOnline.Repository.UserCVs;
using TVOnline.Service.Post;

namespace TVOnline.Controllers
{
    [Route("[controller]")]
    public class PostController(IUserCvService userCvService, IPostService postService, UserManager<Users> userManager) : Controller
    {
        private readonly IUserCvService _userCvService = userCvService;
        private readonly IPostService _postService = postService;
        private readonly UserManager<Users> _userManager = userManager;


        private static List<Post> posts =
        [
            new()
            {
                PostId = 1, Description = "Đây là bài đăng 1", EmployerId = 101.ToString(), Date = DateTime.Now.AddDays(-1)
            },
            new()
            {
                PostId = 2, Description = "Mô tả bài đăng 2", EmployerId = 102.ToString(), Date = DateTime.Now.AddDays(-2)
            },
            new() { PostId = 3, Description = "Mô tả bài đăng 3", EmployerId = 103.ToString(), Date = DateTime.Now.AddDays(-3) }
        ];

        [Route("[action]")]
        public IActionResult Index()
        {
            return View("Details", posts);
        }

        [Route("[action]/{postID}")]
        public IActionResult Details(string postID)
        {
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