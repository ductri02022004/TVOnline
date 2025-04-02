using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using TVOnline.Data;
using TVOnline.Hubs;
using TVOnline.Models;
using TVOnline.Service;
using TVOnline.ViewModels.Chat;
using Microsoft.Extensions.Logging;

namespace TVOnline.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class AdminChatController : Controller
    {
        private readonly IChatService _chatService;
        private readonly UserManager<Users> _userManager;
        private readonly IHubContext<ChatHub> _hubContext;
        private readonly AppDbContext _context;
        private readonly ILogger<AdminChatController> _logger;

        public AdminChatController(
            IChatService chatService,
            UserManager<Users> userManager,
            IHubContext<ChatHub> hubContext,
            AppDbContext context,
            ILogger<AdminChatController> logger)
        {
            _chatService = chatService;
            _userManager = userManager;
            _hubContext = hubContext;
            _context = context;
            _logger = logger;
        }

        // Thiết lập ViewBag cho các thông báo
        private async Task SetupCommonViewBag()
        {
            var adminId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrEmpty(adminId))
            {
                try
                {
                    var unreadCount = await _context.ChatMessages
                        .Where(m => m.ReceiverId == adminId && !m.IsRead)
                        .CountAsync();

                    ViewBag.UnreadMessagesCount = unreadCount;

                    // Get users with chat history - use cached data if available
                    var userIds = await _chatService.GetUserIdsWithChatHistoryAsync(adminId);
                    ViewBag.ChatUsersCount = userIds?.Count ?? 0;

                    _logger.LogInformation($"SetupCommonViewBag: Admin {adminId} has {unreadCount} unread messages and {ViewBag.ChatUsersCount} chat users");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error setting up common ViewBag");
                    ViewBag.UnreadMessagesCount = 0;
                    ViewBag.ChatUsersCount = 0;
                }
            }
        }

        public async Task<IActionResult> Index()
        {
            await SetupCommonViewBag();
            return View();
        }

        [HttpGet]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public async Task<IActionResult> GetUsersWithChatHistory()
        {
            var startTime = DateTime.Now;
            _logger.LogInformation($"GetUsersWithChatHistory started at {startTime}");

            try
            {
                var adminId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                _logger.LogInformation($"GetUsersWithChatHistory called. AdminId: {adminId ?? "null"}");

                if (string.IsNullOrEmpty(adminId))
                {
                    _logger.LogWarning("Admin ID not found when retrieving chat users");
                    return BadRequest("Admin ID not found");
                }

                // Verify admin exists in database
                var adminUser = await _userManager.FindByIdAsync(adminId);
                if (adminUser == null)
                {
                    _logger.LogWarning($"Admin user with ID {adminId} not found in database");
                    return BadRequest("Admin user not found");
                }

                _logger.LogInformation($"Admin user found: {adminUser.UserName}");

                // Direct database verification for message count
                var directMessageCount = await _context.ChatMessages
                    .AsNoTracking()
                    .Where(m => m.SenderId == adminId || m.ReceiverId == adminId)
                    .CountAsync();

                _logger.LogInformation($"Direct count check: {directMessageCount} messages for admin {adminId}");

                // Get users with chat history - optimized version
                var userIds = await _chatService.GetUserIdsWithChatHistoryAsync(adminId);

                var endTime = DateTime.Now;
                var duration = (endTime - startTime).TotalMilliseconds;
                _logger.LogInformation($"GetUsersWithChatHistory completed in {duration:F2}ms, found {userIds.Count} users");

                // Check for inconsistency as a last resort
                if (directMessageCount > 0 && (userIds == null || userIds.Count == 0))
                {
                    _logger.LogWarning($"Critical inconsistency: {directMessageCount} messages exist but no users returned. Doing manual fallback...");

                    // Emergency fallback - direct database query
                    var manualUserIds = await _context.ChatMessages
                        .AsNoTracking()
                        .Where(m => m.SenderId == adminId || m.ReceiverId == adminId)
                        .Select(m => m.SenderId == adminId ? m.ReceiverId : m.SenderId)
                        .Where(id => id != adminId && !string.IsNullOrEmpty(id))
                        .Distinct()
                        .ToListAsync();

                    _logger.LogInformation($"Emergency fallback found {manualUserIds.Count} users");
                    return Json(manualUserIds);
                }

                return Json(userIds);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving users with chat history");

                // Try emergency fallback
                try
                {
                    var adminId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                    if (!string.IsNullOrEmpty(adminId))
                    {
                        _logger.LogWarning("Attempting emergency direct DB query for user IDs after exception");
                        var emergencyUserIds = await _context.ChatMessages
                            .AsNoTracking()
                            .Where(m => m.SenderId == adminId || m.ReceiverId == adminId)
                            .Select(m => m.SenderId == adminId ? m.ReceiverId : m.SenderId)
                            .Where(id => id != adminId && !string.IsNullOrEmpty(id))
                            .Distinct()
                            .ToListAsync();

                        _logger.LogInformation($"Emergency query found {emergencyUserIds.Count} users");
                        return Json(emergencyUserIds);
                    }
                }
                catch (Exception innerEx)
                {
                    _logger.LogError(innerEx, "Emergency fallback also failed");
                }

                return StatusCode(500, new { message = "Error retrieving users", error = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetUserDetails(string userId)
        {
            if (string.IsNullOrEmpty(userId))
                return BadRequest("User ID is required");

            var user = await _userManager.FindByIdAsync(userId);
            return user == null ? NotFound("User not found") : Json(new { userName = user.UserName, email = user.Email });
        }

        [HttpGet]
        public async Task<IActionResult> GetAllUsersDetails()
        {
            var adminId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(adminId))
                return BadRequest("Admin ID not found");

            try
            {
                // Get all users who have chat history with admin
                var userIds = await _chatService.GetUserIdsWithChatHistoryAsync(adminId);

                if (userIds.Count == 0)
                {
                    return Json(new List<object>());
                }

                // Use a single query to get all users at once instead of individual queries
                var users = await _userManager.Users
                    .Where(u => userIds.Contains(u.Id))
                    .Select(u => new { id = u.Id, userName = u.UserName, email = u.Email })
                    .ToListAsync();

                return Json(users);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user details");
                return StatusCode(500, new { message = "Error getting user details" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> GetAllUsersDetails([FromBody] List<string> userIds)
        {
            var adminId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(adminId))
                return BadRequest("Admin ID not found");

            try
            {
                if (userIds == null || userIds.Count == 0)
                {
                    return Json(new List<object>());
                }

                // Use a single query to get all users at once instead of individual queries
                var users = await _userManager.Users
                    .Where(u => userIds.Contains(u.Id))
                    .Select(u => new { id = u.Id, name = u.UserName, email = u.Email })
                    .ToListAsync();

                // Convert to dictionary for client-side lookup
                var userDict = users.ToDictionary(u => u.id, u => u);
                return Json(userDict);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user details");
                return StatusCode(500, new { message = "Error getting user details" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAllUnreadCounts()
        {
            var adminId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(adminId))
                return BadRequest("Admin ID not found");

            try
            {
                // Get all users who have chat history with admin
                var userIds = await _chatService.GetUserIdsWithChatHistoryAsync(adminId);

                if (userIds.Count == 0)
                {
                    return Json(new Dictionary<string, int>());
                }

                // Use a single query to get all unread counts at once
                var unreadCounts = await _context.ChatMessages
                    .Where(m => userIds.Contains(m.SenderId) && m.ReceiverId == adminId && !m.IsRead)
                    .GroupBy(m => m.SenderId)
                    .Select(g => new { userId = g.Key, count = g.Count() })
                    .ToDictionaryAsync(
                        g => g.userId,
                        g => g.count
                    );

                // Make sure all userIds have an entry (default 0 if no unread messages)
                var result = userIds.ToDictionary(
                    id => id,
                    id => unreadCounts.ContainsKey(id) ? unreadCounts[id] : 0
                );

                return Json(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting unread counts");
                return StatusCode(500, new { message = "Error getting unread counts" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetChatHistory(string userId, string otherUserId)
        {
            _logger.LogInformation("GetChatHistory requested for userId: {UserId} and otherUserId: {OtherUserId}", userId, otherUserId);

            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(otherUserId))
            {
                _logger.LogWarning("GetChatHistory called with missing parameters. UserId: {UserId}, OtherUserId: {OtherUserId}",
                    userId ?? "null", otherUserId ?? "null");
                return BadRequest(new { message = "Both user IDs are required" });
            }

            try
            {
                // Verify that users exist
                var adminUser = await _userManager.FindByIdAsync(userId);
                if (adminUser == null)
                {
                    _logger.LogWarning("GetChatHistory: Admin user not found. UserId: {UserId}", userId);
                    return BadRequest(new { message = $"Admin user with ID {userId} not found" });
                }

                var otherUser = await _userManager.FindByIdAsync(otherUserId);
                if (otherUser == null)
                {
                    _logger.LogWarning("GetChatHistory: Other user not found. OtherUserId: {OtherUserId}", otherUserId);
                    return BadRequest(new { message = $"User with ID {otherUserId} not found" });
                }

                _logger.LogInformation("GetChatHistory: Both users found. Retrieving messages between {AdminName} and {UserName}",
                    adminUser.UserName, otherUser.UserName);

                var messages = await _chatService.GetChatHistoryAsync(userId, otherUserId);
                _logger.LogInformation("Successfully retrieved {Count} messages for conversation between {UserId} and {OtherUserId}",
                    messages.Count(), userId, otherUserId);
                return Json(messages);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving chat history for userId: {UserId} and otherUserId: {OtherUserId}. Error: {ErrorMessage}",
                    userId, otherUserId, ex.Message);

                // Include inner exception details if available
                string errorMessage = ex.Message;
                if (ex.InnerException != null)
                {
                    errorMessage += $" Inner exception: {ex.InnerException.Message}";
                    _logger.LogError(ex.InnerException, "Inner exception details");
                }

                return StatusCode(500, new { message = $"An error occurred: {errorMessage}" });
            }
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

                // Handle file attachment if exists
                if (model.Attachment != null && model.Attachment.Length > 0)
                {
                    // Validate file size (5MB max)
                    if (model.Attachment.Length > 5 * 1024 * 1024)
                    {
                        return BadRequest(new { message = "Kích thước file quá lớn, giới hạn 5MB" });
                    }

                    // Get file extension and validate
                    var extension = Path.GetExtension(model.Attachment.FileName).ToLowerInvariant();
                    var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".pdf", ".doc", ".docx", ".txt", ".xls", ".xlsx" };

                    if (!allowedExtensions.Contains(extension))
                    {
                        return BadRequest(new { message = "Định dạng file không được hỗ trợ" });
                    }

                    // Create unique filename
                    var uniqueFileName = Guid.NewGuid().ToString() + extension;
                    var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "chat");

                    // Create directory if it doesn't exist
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    // Save file
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await model.Attachment.CopyToAsync(stream);
                    }

                    // Update message content to include file link
                    var fileUrl = $"/uploads/chat/{uniqueFileName}";
                    var isImage = new[] { ".jpg", ".jpeg", ".png", ".gif" }.Contains(extension);

                    // If message text is empty, use file name
                    if (string.IsNullOrEmpty(model.Message))
                    {
                        model.Message = model.Attachment.FileName;
                    }

                    // Append file information to message content
                    message.Content = model.Message + "\n[FILE]" + fileUrl + "\n" + model.Attachment.FileName + "\n" + extension;
                }

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
                _logger.LogError(ex, "Lỗi khi gửi tin nhắn từ Admin");
                return StatusCode(500, new { message = $"Đã xảy ra lỗi: {ex.Message}" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> MarkAsRead([FromBody] MarkAsReadViewModel model)
        {
            _logger.LogInformation("MarkAsRead request received. ModelState valid: {IsValid}", ModelState.IsValid);

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state in MarkAsRead: {Errors}",
                    string.Join(", ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)));
                return BadRequest(new { message = "Invalid data", errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage) });
            }

            try
            {
                _logger.LogInformation("Marking messages as read from sender {SenderId} to receiver {ReceiverId}", model.SenderId, model.ReceiverId);

                if (string.IsNullOrEmpty(model.SenderId) || string.IsNullOrEmpty(model.ReceiverId))
                {
                    _logger.LogWarning("SenderId or ReceiverId is null or empty");
                    return BadRequest(new { message = "SenderId and ReceiverId are required" });
                }

                await _chatService.MarkAsReadAsync(model.SenderId, model.ReceiverId);

                // Update unread count
                var unreadCount = await _chatService.GetUnreadCountAsync(model.ReceiverId);
                await _hubContext.Clients.Group(model.ReceiverId).SendAsync("UpdateUnreadCount", unreadCount);

                return Ok(new { success = true, message = "Messages marked as read" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking messages as read from {SenderId} to {ReceiverId}", model.SenderId, model.ReceiverId);
                return StatusCode(500, new { message = $"An error occurred: {ex.Message}" });
            }
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

        [HttpPut]
        [Route("/Admin/api/chat/messages/{id}")]
        public async Task<IActionResult> UpdateMessage(int id, [FromBody] UpdateMessageViewModel model)
        {
            _logger.LogInformation("Updating message ID: {MessageId}", id);

            if (!ModelState.IsValid)
            {
                var errors = string.Join(", ", ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage));
                _logger.LogWarning("Invalid update message data: {Errors}", errors);
                return BadRequest(new { message = "Invalid data: " + errors });
            }

            try
            {
                var adminId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(adminId))
                {
                    _logger.LogWarning("Admin ID not found when updating message ID: {MessageId}", id);
                    return BadRequest(new { message = "Admin ID not found" });
                }

                // Check if message exists
                var messageExists = await _chatService.GetMessageByIdAsync(id);
                if (messageExists == null)
                {
                    _logger.LogWarning("Message not found ID: {MessageId}", id);
                    return NotFound(new { message = "Message not found" });
                }

                _logger.LogInformation("Message ID: {MessageId} found, proceeding with update", id);
                var message = await _chatService.UpdateMessageAsync(id, model.Content, adminId, true);

                // Notify users about the update
                await _hubContext.Clients.Group(message.SenderId).SendAsync("MessageUpdated", message);
                await _hubContext.Clients.Group(message.ReceiverId).SendAsync("MessageUpdated", message);

                _logger.LogInformation("Message update successful ID: {MessageId}", id);
                return Ok(new { message = "Message updated successfully", data = message });
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogWarning(ex, "Concurrency error when updating message ID: {MessageId}", id);
                return StatusCode(409, new { message = "The message was modified by another user. Please refresh and try again." });
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Database error updating message ID: {MessageId}", id);
                return StatusCode(500, new { message = $"Database error: {ex.Message}" });
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Authorization error when updating message ID: {MessageId}", id);
                return StatusCode(403, new { message = $"Authorization error: {ex.Message}" });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Message not found ID: {MessageId}", id);
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating message ID: {MessageId}", id);
                var errorMessage = ex.InnerException != null
                    ? $"{ex.Message}. {ex.InnerException.Message}"
                    : ex.Message;
                return StatusCode(500, new { message = $"An error occurred: {errorMessage}" });
            }
        }

        [HttpDelete]
        [Route("/Admin/api/chat/messages/{id}")]
        public async Task<IActionResult> DeleteMessage(int id)
        {
            _logger.LogInformation("Deleting message ID: {MessageId}", id);

            try
            {
                var adminId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(adminId))
                {
                    _logger.LogWarning("Admin ID not found when deleting message ID: {MessageId}", id);
                    return BadRequest(new { message = "Admin ID not found" });
                }

                // Get message to determine receiver before deletion
                var message = await _chatService.GetMessageByIdAsync(id);
                if (message == null)
                {
                    _logger.LogWarning("Message not found ID: {MessageId}", id);
                    return NotFound(new { message = "Message not found" });
                }

                // Keep sender and receiver info for notifications
                var receiverId = message.ReceiverId;
                var senderId = message.SenderId;

                _logger.LogInformation("Message ID: {MessageId} found, proceeding with deletion", id);

                // Force delete for admin
                var success = await _chatService.DeleteMessageAsync(id, adminId, true);
                if (!success)
                {
                    _logger.LogWarning("Delete message failed ID: {MessageId}", id);
                    return NotFound(new { message = "Message not found" });
                }

                // Notify users about the deletion
                await _hubContext.Clients.Group(adminId).SendAsync("MessageDeleted", id);

                // Notify related users (if not admin)
                if (senderId != adminId)
                    await _hubContext.Clients.Group(senderId).SendAsync("MessageDeleted", id);

                if (receiverId != adminId)
                    await _hubContext.Clients.Group(receiverId).SendAsync("MessageDeleted", id);

                _logger.LogInformation("Message deleted successfully ID: {MessageId}", id);
                return Ok(new { message = "Message deleted successfully" });
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogWarning(ex, "Concurrency error when deleting message ID: {MessageId}", id);
                return StatusCode(409, new { message = "The message was modified by another user. Please refresh and try again." });
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Database error deleting message ID: {MessageId}", id);
                return StatusCode(500, new { message = $"Database error: {ex.Message}" });
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Authorization error when deleting message ID: {MessageId}", id);
                return StatusCode(403, new { message = $"Authorization error: {ex.Message}" });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogError(ex, "Operation error when deleting message ID: {MessageId}", id);
                return StatusCode(500, new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting message ID: {MessageId}", id);
                var errorMessage = ex.InnerException != null
                    ? $"{ex.Message}. {ex.InnerException.Message}"
                    : ex.Message;
                return StatusCode(500, new { message = $"An error occurred: {errorMessage}" });
            }
        }

        [HttpGet]
        [Route("TestConnection")]
        public IActionResult TestConnection()
        {
            try
            {
                var adminId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

                return Json(new
                {
                    success = true,
                    timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                    adminId = adminId ?? "Not found",
                    environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Not set",
                    server = HttpContext.Request.Host.Value
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in test connection endpoint");
                return StatusCode(500, new { success = false, error = ex.Message });
            }
        }
    }
}