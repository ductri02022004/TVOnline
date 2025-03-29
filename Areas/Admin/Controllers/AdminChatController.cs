using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TVOnline.Hubs;
using TVOnline.Models;
using TVOnline.Service;
using TVOnline.ViewModels.Chat;

namespace TVOnline.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class AdminChatController : Controller
    {
        private readonly IChatService _chatService;
        private readonly UserManager<Users> _userManager;
        private readonly IHubContext<ChatHub> _hubContext;

        public AdminChatController(
            IChatService chatService,
            UserManager<Users> userManager,
            IHubContext<ChatHub> hubContext)
        {
            _chatService = chatService;
            _userManager = userManager;
            _hubContext = hubContext;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetUsersWithChatHistory()
        {
            var adminId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(adminId))
                return BadRequest("Admin ID not found");

            var userIds = await _chatService.GetUserIdsWithChatHistoryAsync(adminId);
            return Json(userIds);
        }

        [HttpGet]
        public async Task<IActionResult> GetUserDetails(string userId)
        {
            if (string.IsNullOrEmpty(userId))
                return BadRequest("User ID is required");

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return NotFound("User not found");

            return Json(new { userName = user.UserName, email = user.Email });
        }

        [HttpGet]
        public async Task<IActionResult> GetChatHistory(string userId, string otherUserId)
        {
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(otherUserId))
                return BadRequest("Both user IDs are required");

            var messages = await _chatService.GetChatHistoryAsync(userId, otherUserId);
            return Json(messages);
        }

        [HttpPost]
        public async Task<IActionResult> SendMessage([FromForm] SendMessageViewModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var message = new ChatMessage
                {
                    SenderId = model.SenderId,
                    ReceiverId = model.ReceiverId,
                    Content = model.Message,
                    Timestamp = DateTime.Now,
                    IsRead = false
                };

                var savedMessage = await _chatService.SaveMessageAsync(message);

                // Send the message through SignalR
                await _hubContext.Clients.Group(model.ReceiverId).SendAsync("ReceiveMessage", savedMessage);
                await _hubContext.Clients.Group(model.SenderId).SendAsync("ReceiveMessage", savedMessage);

                // Update unread count for receiver
                var unreadCount = await _chatService.GetUnreadCountAsync(model.ReceiverId);
                await _hubContext.Clients.Group(model.ReceiverId).SendAsync("UpdateUnreadCount", unreadCount);

                return Ok(savedMessage);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        [HttpPost]
        public async Task<IActionResult> MarkAsRead([FromForm] MarkAsReadViewModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            await _chatService.MarkAsReadAsync(model.SenderId, model.ReceiverId);

            // Update unread count
            var unreadCount = await _chatService.GetUnreadCountAsync(model.ReceiverId);
            await _hubContext.Clients.Group(model.ReceiverId).SendAsync("UpdateUnreadCount", unreadCount);

            return Ok();
        }

        [HttpGet]
        public async Task<IActionResult> GetUnreadCount(string userId)
        {
            if (string.IsNullOrEmpty(userId))
                return BadRequest("User ID is required");

            var adminId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(adminId))
                return BadRequest("Admin ID not found");

            var count = await _chatService.GetUnreadCountFromUserAsync(userId, adminId);
            return Json(count);
        }
    }
}