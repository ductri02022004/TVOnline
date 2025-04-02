using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;
using TVOnline.Hubs;
using TVOnline.Models;
using TVOnline.Service;
using TVOnline.ViewModels.Chat;

namespace TVOnline.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ChatController : ControllerBase
    {
        private readonly IChatService _chatService;
        private readonly UserManager<Users> _userManager;
        private readonly IHubContext<ChatHub> _hubContext;
        private readonly ILogger<ChatController> _logger;

        public ChatController(
            IChatService chatService,
            UserManager<Users> userManager,
            IHubContext<ChatHub> hubContext,
            ILogger<ChatController> logger)
        {
            _chatService = chatService;
            _userManager = userManager;
            _hubContext = hubContext;
            _logger = logger;
        }

        [HttpGet("history")]
        public async Task<IActionResult> GetChatHistory(string userId, string otherUserId)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(currentUserId))
                return Unauthorized("User not authenticated");

            // If userId is not provided, use current user's ID
            if (string.IsNullOrEmpty(userId))
                userId = currentUserId;

            // If otherUserId is not provided, return bad request
            if (string.IsNullOrEmpty(otherUserId))
                return BadRequest("Receiver ID is required");

            try
            {
                _logger.LogInformation($"Getting chat history between users {userId} and {otherUserId}");
                var messages = await _chatService.GetChatHistoryAsync(userId, otherUserId);
                return Ok(messages);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving chat history: {ex.Message}");
                return StatusCode(500, new { message = $"An error occurred: {ex.Message}" });
            }
        }

        [HttpPost("messages")]
        public async Task<IActionResult> SendMessage([FromBody] SendMessageViewModel model)
        {
            _logger.LogInformation("Receiving message: {SenderId} -> {ReceiverId}, content length: {Length}",
                model?.SenderId, model?.ReceiverId, model?.Message?.Length ?? 0);

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("Unauthorized attempt to send message");
                return Unauthorized(new { message = "Bạn cần đăng nhập để gửi tin nhắn" });
            }

            // If senderId is provided but doesn't match the current user, use the current user's ID
            if (!string.IsNullOrEmpty(model.SenderId) && model.SenderId != userId)
            {
                _logger.LogWarning("SenderId mismatch. Provided: {ProvidedId}, Actual: {ActualId}", model.SenderId, userId);
                model.SenderId = userId; // Override with the authenticated user's ID
            }
            // If senderId is not provided, use the current user's ID
            else if (string.IsNullOrEmpty(model.SenderId))
            {
                model.SenderId = userId;
            }

            if (string.IsNullOrEmpty(model.Message))
            {
                _logger.LogWarning("Empty message content");
                return BadRequest(new { message = "Nội dung tin nhắn không được để trống" });
            }

            if (string.IsNullOrEmpty(model.ReceiverId))
            {
                _logger.LogWarning("Missing receiver ID");
                return BadRequest(new { message = "ID người nhận không được để trống" });
            }

            try
            {
                var message = new ChatMessage
                {
                    SenderId = model.SenderId, // This is now guaranteed to be the current user's ID
                    ReceiverId = model.ReceiverId,
                    Content = model.Message,
                    Timestamp = DateTime.Now,
                    IsRead = false
                };

                var savedMessage = await _chatService.SaveMessageAsync(message);

                // Send the message through SignalR
                await _hubContext.Clients.Group(model.ReceiverId).SendAsync("ReceiveMessage", savedMessage);
                await _hubContext.Clients.Group(model.SenderId).SendAsync("ReceiveMessage", savedMessage);

                return Ok(savedMessage);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending message: {Message}", ex.Message);
                return StatusCode(500, new { message = $"Đã xảy ra lỗi khi gửi tin nhắn: {ex.Message}" });
            }
        }

        [HttpPut("messages/{id}")]
        public async Task<IActionResult> UpdateMessage(int id, [FromBody] UpdateMessageViewModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            try
            {
                var message = await _chatService.UpdateMessageAsync(id, model.Content, userId, false);

                // Notify users about the update through SignalR
                await _hubContext.Clients.Group(message.SenderId).SendAsync("MessageUpdated", message);
                await _hubContext.Clients.Group(message.ReceiverId).SendAsync("MessageUpdated", message);

                return Ok(message);
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid("You can only edit your own messages");
            }
            catch (KeyNotFoundException)
            {
                return NotFound("Message not found");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        [HttpDelete("messages/{id}")]
        public async Task<IActionResult> DeleteMessage(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { message = "Bạn cần đăng nhập để thực hiện hành động này" });

            try
            {
                // Get message to determine receiver before deletion
                var message = await _chatService.GetMessageByIdAsync(id);
                if (message == null)
                    return NotFound(new { message = "Không tìm thấy tin nhắn" });

                if (message.SenderId != userId)
                    return StatusCode(403, new { message = "Bạn chỉ có thể xóa tin nhắn của mình" });

                var receiverId = message.ReceiverId;

                var success = await _chatService.DeleteMessageAsync(id, userId, false);
                if (!success)
                    return NotFound(new { message = "Không tìm thấy tin nhắn" });

                // Notify users about the deletion through SignalR
                await _hubContext.Clients.Group(userId).SendAsync("MessageDeleted", id);
                await _hubContext.Clients.Group(receiverId).SendAsync("MessageDeleted", id);

                return Ok(new { message = "Đã xóa tin nhắn thành công" });
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403, new { message = $"Lỗi quyền truy cập: {ex.Message}" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi xóa tin nhắn {Id}", id);
                return StatusCode(500, new { message = $"Đã xảy ra lỗi: {ex.Message}" });
            }
        }

        [HttpGet("admin-id")]
        public async Task<IActionResult> GetAdminId()
        {
            var admin = await _userManager.GetUsersInRoleAsync("Admin");
            var adminId = admin.FirstOrDefault()?.Id;

            if (string.IsNullOrEmpty(adminId))
                return NotFound("Admin not found");

            return Ok(new { adminId });
        }

        [HttpPost("mark-as-read")]
        public async Task<IActionResult> MarkAsRead([FromBody] MarkAsReadViewModel model)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            if (string.IsNullOrEmpty(model.SenderId))
                return BadRequest("Sender ID is required.");

            await _chatService.MarkAsReadAsync(model.SenderId, userId);
            return Ok();
        }

        [HttpGet("unread-count")]
        public async Task<IActionResult> GetUnreadCount()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var count = await _chatService.GetUnreadCountAsync(userId);
            return Ok(new { count });
        }

        [HttpGet("users-with-conversations")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetUsersWithChatHistory()
        {
            try
            {
                _logger.LogInformation("Getting users with chat history");
                var userIds = await _chatService.GetUserIdsWithChatHistoryAsync("admin");
                _logger.LogInformation($"Found {userIds.Count} users with chat history");
                return Ok(userIds);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting users with chat history");
                return StatusCode(500, "An error occurred while getting users with chat history");
            }
        }

        [HttpGet("history/{userId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetChatHistoryWithUser(string userId)
        {
            try
            {
                if (string.IsNullOrEmpty(userId))
                {
                    _logger.LogWarning("GetChatHistoryWithUser called with missing user ID");
                    return BadRequest("User ID is required");
                }

                _logger.LogInformation($"Getting chat history with user {userId}");
                var messages = await _chatService.GetChatHistoryAsync(userId, "admin");
                _logger.LogInformation($"Retrieved {messages.Count()} messages");
                return Ok(messages);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting chat history with user {userId}");
                return StatusCode(500, "An error occurred while retrieving chat history");
            }
        }

        [HttpPost("send-with-attachment")]
        public async Task<IActionResult> SendMessageWithAttachment([FromForm] SendMessageViewModel model)
        {
            _logger.LogInformation("Receiving message with attachment: {SenderId} -> {ReceiverId}",
                model?.SenderId, model?.ReceiverId);

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("Unauthorized attempt to send message with attachment");
                return Unauthorized(new { message = "Bạn cần đăng nhập để gửi tin nhắn" });
            }

            // If senderId is provided but doesn't match the current user, use the current user's ID
            if (!string.IsNullOrEmpty(model.SenderId) && model.SenderId != userId)
            {
                _logger.LogWarning("SenderId mismatch. Provided: {ProvidedId}, Actual: {ActualId}", model.SenderId, userId);
                model.SenderId = userId; // Override with the authenticated user's ID
            }
            // If senderId is not provided, use the current user's ID
            else if (string.IsNullOrEmpty(model.SenderId))
            {
                model.SenderId = userId;
            }

            if (string.IsNullOrEmpty(model.ReceiverId))
            {
                _logger.LogWarning("Missing receiver ID in attachment message");
                return BadRequest(new { message = "ID người nhận không được để trống" });
            }

            try
            {
                string messageContent = model.Message;

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
                    messageContent = model.Message + "\n[FILE]" + fileUrl + "\n" + model.Attachment.FileName + "\n" + extension;
                }

                var message = new ChatMessage
                {
                    SenderId = model.SenderId,
                    ReceiverId = model.ReceiverId,
                    Content = messageContent,
                    Timestamp = DateTime.Now,
                    IsRead = false
                };

                var savedMessage = await _chatService.SaveMessageAsync(message);

                // Send the message through SignalR
                await _hubContext.Clients.Group(model.ReceiverId).SendAsync("ReceiveMessage", savedMessage);
                await _hubContext.Clients.Group(model.SenderId).SendAsync("ReceiveMessage", savedMessage);

                return Ok(savedMessage);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending message with attachment: {Message}", ex.Message);
                return StatusCode(500, new { message = $"Đã xảy ra lỗi khi gửi tin nhắn: {ex.Message}" });
            }
        }
    }
}