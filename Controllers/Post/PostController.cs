using Microsoft.AspNetCore.Mvc;
using TVOnline.Models;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using System.IO;
using System;

namespace TVOnline.Controllers
{
    public class PostController : Controller
    {
        private static List<Post> posts = new List<Post>
        {
            new Post { PostId = "1", Description = "Đây là bài đăng 1", EmployerID = 101, Date = DateTime.Now.AddDays(-1), ImageUrl = "/images/post1.jpg" },
            new Post { PostId = "2", Description = "Mô tả bài đăng 2", EmployerID = 102, Date = DateTime.Now.AddDays(-2), ImageUrl = "/images/post2.jpg" },
            new Post { PostId = "3", Description = "Mô tả bài đăng 3", EmployerID = 103, Date = DateTime.Now.AddDays(-3), ImageUrl = "/images/post3.jpg" }
        };

        public IActionResult Details(string id)
        {
            var post = posts.FirstOrDefault(p => p.PostId == id);
            if (post == null)
            {
                return NotFound();
            }
            return View(post);
        }

        [HttpPost]
        public IActionResult Apply(IFormFile cvFile, string postId)
        {
            if (cvFile != null && cvFile.Length > 0)
            {
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads", cvFile.FileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    cvFile.CopyTo(stream);
                }

                // Lưu thông tin ứng tuyển vào cơ sở dữ liệu hoặc xử lý thêm tại đây
                // Ví dụ: Lưu thông tin ứng viên và đường dẫn CV vào cơ sở dữ liệu

                TempData["SuccessMessage"] = "Ứng tuyển thành công!";
                return RedirectToAction("Details", new { id = postId });
            }

            TempData["ErrorMessage"] = "Vui lòng tải lên CV của bạn.";
            return RedirectToAction("Details", new { id = postId });
        }
    }
}