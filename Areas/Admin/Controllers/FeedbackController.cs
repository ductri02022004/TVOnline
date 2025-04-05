using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using TVOnline.Data;
using TVOnline.Models;
using TVOnline.Utils;
using System.Collections.Generic;

namespace TVOnline.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class FeedbackController : Controller
    {
        private readonly AppDbContext _context;

        public FeedbackController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Admin/Feedback
        public async Task<IActionResult> Index()
        {
            var usersWithFeedback = await _context.Users
                .Where(u => u.Feedbacks.Any())
                .Select(u => new UserFeedbackViewModel
                {
                    UserId = u.Id,
                    UserName = u.UserName,
                    Email = u.Email,
                    TotalFeedbacks = u.Feedbacks.Count,
                    AverageRating = u.Feedbacks.Any() ? u.Feedbacks.Average(f => f.Rating) : 0,
                    LatestFeedbackDate = u.Feedbacks.Any() ? u.Feedbacks.Max(f => f.Date) : DateTime.MinValue
                })
                .ToListAsync();

            var employersWithFeedback = await _context.Employers
                .Where(e => e.Feedbacks.Any())
                .Select(e => new UserFeedbackViewModel
                {
                    UserId = e.EmployerId,
                    UserName = e.CompanyName,
                    Email = e.Email,
                    IsEmployer = true,
                    TotalFeedbacks = e.Feedbacks.Count,
                    AverageRating = e.Feedbacks.Any() ? e.Feedbacks.Average(f => f.Rating) : 0,
                    LatestFeedbackDate = e.Feedbacks.Any() ? e.Feedbacks.Max(f => f.Date) : DateTime.MinValue
                })
                .ToListAsync();

            var allUsersWithFeedback = usersWithFeedback.Concat(employersWithFeedback).OrderByDescending(u => u.TotalFeedbacks).ToList();

            var dashboardStats = new FeedbackDashboardStatsViewModel
            {
                TotalUsers = allUsersWithFeedback.Count,
                TotalFeedbacks = allUsersWithFeedback.Sum(u => u.TotalFeedbacks),
                AverageRating = allUsersWithFeedback.Any() ? allUsersWithFeedback.Average(u => u.AverageRating) : 0,
                HighestRatedUser = allUsersWithFeedback.OrderByDescending(u => u.AverageRating).FirstOrDefault(),
                MostActiveFeedbackUser = allUsersWithFeedback.OrderByDescending(u => u.TotalFeedbacks).FirstOrDefault()
            };

            var viewModel = new FeedbackIndexViewModel
            {
                Users = allUsersWithFeedback,
                Stats = dashboardStats
            };

            return View(viewModel);
        }

        // GET: Admin/Feedback/UserFeedbacks/5
        public async Task<IActionResult> UserFeedbacks(string id, string sortOrder, string currentFilter, string searchString, int? pageNumber)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            ViewData["CurrentSort"] = sortOrder;
            ViewData["DateSortParm"] = String.IsNullOrEmpty(sortOrder) ? "date_desc" : "";
            ViewData["RatingSortParm"] = sortOrder == "rating" ? "rating_desc" : "rating";

            if (searchString != null)
            {
                pageNumber = 1;
            }
            else
            {
                searchString = currentFilter;
            }

            ViewData["CurrentFilter"] = searchString;

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
            var employer = await _context.Employers.FirstOrDefaultAsync(e => e.EmployerId == id);

            if (user == null && employer == null)
            {
                return NotFound();
            }

            var feedbacks = _context.Feedbacks.AsQueryable();

            if (user != null)
            {
                ViewData["UserName"] = user.UserName;
                ViewData["IsEmployer"] = false;
                feedbacks = feedbacks.Where(f => f.UserId == id);
            }
            else
            {
                ViewData["UserName"] = employer.CompanyName;
                ViewData["IsEmployer"] = true;
                feedbacks = feedbacks.Where(f => f.EmployerId == id);
            }

            if (!String.IsNullOrEmpty(searchString))
            {
                feedbacks = feedbacks.Where(f => f.Content != null && f.Content.Contains(searchString));
            }

            switch (sortOrder)
            {
                case "date_desc":
                    feedbacks = feedbacks.OrderByDescending(f => f.Date);
                    break;
                case "rating":
                    feedbacks = feedbacks.OrderBy(f => f.Rating);
                    break;
                case "rating_desc":
                    feedbacks = feedbacks.OrderByDescending(f => f.Rating);
                    break;
                default:
                    feedbacks = feedbacks.OrderBy(f => f.Date);
                    break;
            }

            int pageSize = 10;
            return View(await PaginatedList<Feedbacks>.CreateAsync(feedbacks.AsNoTracking(), pageNumber ?? 1, pageSize));
        }

        // GET: Admin/Feedback/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var feedback = await _context.Feedbacks
                .Include(f => f.User)
                .Include(f => f.Employer)
                .FirstOrDefaultAsync(m => m.FeedbackId == id);

            if (feedback == null)
            {
                return NotFound();
            }

            return View(feedback);
        }

        // GET: Admin/Feedback/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var feedback = await _context.Feedbacks
                .Include(f => f.User)
                .Include(f => f.Employer)
                .FirstOrDefaultAsync(m => m.FeedbackId == id);

            if (feedback == null)
            {
                return NotFound();
            }

            return View(feedback);
        }

        // POST: Admin/Feedback/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var feedback = await _context.Feedbacks.FindAsync(id);
            if (feedback != null)
            {
                _context.Feedbacks.Remove(feedback);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Phản hồi đã được xóa thành công.";
            }
            
            // Redirect back to the user's feedback list if we have the user ID
            if (!string.IsNullOrEmpty(feedback?.UserId))
            {
                return RedirectToAction(nameof(UserFeedbacks), new { id = feedback.UserId });
            }
            else if (!string.IsNullOrEmpty(feedback?.EmployerId))
            {
                return RedirectToAction(nameof(UserFeedbacks), new { id = feedback.EmployerId });
            }
            
            return RedirectToAction(nameof(Index));
        }

        // GET: Admin/Feedback/Reply/5
        public async Task<IActionResult> Reply(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var feedback = await _context.Feedbacks
                .Include(f => f.User)
                .Include(f => f.Employer)
                .FirstOrDefaultAsync(m => m.FeedbackId == id);

            if (feedback == null)
            {
                return NotFound();
            }

            var model = new FeedbackReplyViewModel
            {
                FeedbackId = feedback.FeedbackId,
                UserName = feedback.User != null ? feedback.User.UserName : (feedback.Employer != null ? feedback.Employer.CompanyName : "Người dùng ẩn danh"),
                FeedbackContent = feedback.Content,
                FeedbackDate = feedback.Date,
                Rating = feedback.Rating,
                ReplyContent = feedback.AdminReply
            };

            return View(model);
        }

        // POST: Admin/Feedback/Reply
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reply(FeedbackReplyViewModel model)
        {
            if (ModelState.IsValid)
            {
                var feedback = await _context.Feedbacks.FindAsync(model.FeedbackId);
                if (feedback != null)
                {
                    feedback.AdminReply = model.ReplyContent;
                    feedback.AdminReplyDate = DateTime.Now;
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Phản hồi đã được gửi thành công.";
                    
                    // Redirect back to the user's feedback list if we have the user ID
                    if (!string.IsNullOrEmpty(feedback.UserId))
                    {
                        return RedirectToAction(nameof(UserFeedbacks), new { id = feedback.UserId });
                    }
                    else if (!string.IsNullOrEmpty(feedback.EmployerId))
                    {
                        return RedirectToAction(nameof(UserFeedbacks), new { id = feedback.EmployerId });
                    }
                    
                    return RedirectToAction(nameof(Index));
                }
            }
            return View(model);
        }

        // GET: Admin/Feedback/Dashboard
        public async Task<IActionResult> Dashboard()
        {
            var totalFeedbacks = await _context.Feedbacks.CountAsync();
            var avgRating = await _context.Feedbacks.AverageAsync(f => f.Rating);

            var ratingCounts = await _context.Feedbacks
                .GroupBy(f => f.Rating)
                .Select(g => new { Rating = g.Key, Count = g.Count() })
                .ToListAsync();

            var recentFeedbacks = await _context.Feedbacks
                .Include(f => f.User)
                .Include(f => f.Employer)
                .OrderByDescending(f => f.Date)
                .Take(20)
                .ToListAsync();

            var dashboardModel = new FeedbackDashboardViewModel
            {
                TotalFeedbacks = totalFeedbacks,
                AverageRating = avgRating,
                OneStar = ratingCounts.FirstOrDefault(r => r.Rating == 1)?.Count ?? 0,
                TwoStars = ratingCounts.FirstOrDefault(r => r.Rating == 2)?.Count ?? 0,
                ThreeStars = ratingCounts.FirstOrDefault(r => r.Rating == 3)?.Count ?? 0,
                FourStars = ratingCounts.FirstOrDefault(r => r.Rating == 4)?.Count ?? 0,
                FiveStars = ratingCounts.FirstOrDefault(r => r.Rating == 5)?.Count ?? 0,
                RecentFeedbacks = recentFeedbacks
            };

            return View(dashboardModel);
        }
    }

    public class UserFeedbackViewModel
    {
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public bool IsEmployer { get; set; }
        public int TotalFeedbacks { get; set; }
        public double AverageRating { get; set; }
        public DateTime LatestFeedbackDate { get; set; }
    }

    public class FeedbackDashboardStatsViewModel
    {
        public int TotalUsers { get; set; }
        public int TotalFeedbacks { get; set; }
        public double AverageRating { get; set; }
        public UserFeedbackViewModel HighestRatedUser { get; set; }
        public UserFeedbackViewModel MostActiveFeedbackUser { get; set; }
    }

    public class FeedbackIndexViewModel
    {
        public List<UserFeedbackViewModel> Users { get; set; }
        public FeedbackDashboardStatsViewModel Stats { get; set; }
    }

    public class FeedbackReplyViewModel
    {
        public string FeedbackId { get; set; }
        public string UserName { get; set; }
        public string FeedbackContent { get; set; }
        public DateTime FeedbackDate { get; set; }
        public int Rating { get; set; }
        public string ReplyContent { get; set; }
    }

    public class FeedbackDashboardViewModel
    {
        public int TotalFeedbacks { get; set; }
        public double AverageRating { get; set; }
        public int OneStar { get; set; }
        public int TwoStars { get; set; }
        public int ThreeStars { get; set; }
        public int FourStars { get; set; }
        public int FiveStars { get; set; }
        public List<Feedbacks> RecentFeedbacks { get; set; }
    }
}
