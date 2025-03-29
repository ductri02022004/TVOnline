using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Threading.Tasks;
using TVOnline.Hubs;
using TVOnline.Models;
using TVOnline.Service;
using TVOnline.ViewModels.Chat;
using Microsoft.Extensions.Logging;

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
            try
            {
                if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(otherUserId))
                {
                    _logger.LogWarning("GetChatHistory called with missing user IDs");
                    return BadRequest("Both user IDs are required");
                }

                _logger.LogInformation($"Getting chat history between {userId} and {otherUserId}");
                var messages = await _chatService.GetChatHistoryAsync(userId, otherUserId);
                _logger.LogInformation($"Retrieved {messages.Count()} messages");
                return Ok(messages);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting chat history between {userId} and {otherUserId}");
                return StatusCode(500, "An error occurred while retrieving chat history");
            }
        }

        [HttpPost("send")]
        public async Task<IActionResult> SendMessage([FromBody] SendMessageViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("SendMessage called with invalid model state");
                    return BadRequest(ModelState);
                }

                _logger.LogInformation($"Sending message from {model.SenderId} to {model.ReceiverId}");

                var message = new ChatMessage
                {
                    SenderId = model.SenderId,
                    ReceiverId = model.ReceiverId,
                    Content = model.Message,
                    Timestamp = DateTime.Now,
                    IsRead = false
                };

                var savedMessage = await _chatService.SaveMessageAsync(message);
                _logger.LogInformation($"Message saved with ID {savedMessage.Id}");

                // Send the message through SignalR
                await _hubContext.Clients.Group(model.ReceiverId).SendAsync("ReceiveMessage", savedMessage);
                await _hubContext.Clients.Group(model.SenderId).SendAsync("ReceiveMessage", savedMessage);
                _logger.LogInformation("Message sent through SignalR");

                // Update unread count for receiver
                var unreadCount = await _chatService.GetUnreadCountAsync(model.ReceiverId);
                await _hubContext.Clients.Group(model.ReceiverId).SendAsync("UpdateUnreadCount", unreadCount);
                _logger.LogInformation($"Updated unread count for {model.ReceiverId}: {unreadCount}");

                return Ok(savedMessage);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error sending message from {model.SenderId} to {model.ReceiverId}");
                return StatusCode(500, "An error occurred while sending the message");
            }
        }

        [HttpPost("mark-as-read")]
        public async Task<IActionResult> MarkAsRead([FromBody] MarkAsReadViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("MarkAsRead called with invalid model state");
                    return BadRequest(ModelState);
                }

                _logger.LogInformation($"Marking messages as read from {model.SenderId} to {model.ReceiverId}");
                await _chatService.MarkAsReadAsync(model.SenderId, model.ReceiverId);

                // Update unread count
                var unreadCount = await _chatService.GetUnreadCountAsync(model.ReceiverId);
                await _hubContext.Clients.Group(model.ReceiverId).SendAsync("UpdateUnreadCount", unreadCount);
                _logger.LogInformation($"Updated unread count for {model.ReceiverId}: {unreadCount}");

                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error marking messages as read from {model.SenderId} to {model.ReceiverId}");
                return StatusCode(500, "An error occurred while marking messages as read");
            }
        }

        [HttpGet("unread-count")]
        public async Task<IActionResult> GetUnreadCount(string userId)
        {
            try
            {
                if (string.IsNullOrEmpty(userId))
                {
                    _logger.LogWarning("GetUnreadCount called with missing user ID");
                    return BadRequest("User ID is required");
                }

                _logger.LogInformation($"Getting unread count for user {userId}");
                var count = await _chatService.GetUnreadCountAsync(userId);
                _logger.LogInformation($"Unread count for {userId}: {count}");
                return Ok(new { count });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting unread count for user {userId}");
                return StatusCode(500, "An error occurred while getting unread count");
            }
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

        [HttpGet("admin-id")]
        public async Task<IActionResult> GetAdminId()
        {
            try
            {
                _logger.LogInformation("Getting admin ID");
                var adminUser = await _userManager.FindByEmailAsync("admin@tvonline.com");
                if (adminUser == null)
                {
                    _logger.LogWarning("Admin user not found");
                    return NotFound("Admin user not found");
                }

                _logger.LogInformation($"Admin ID retrieved: {adminUser.Id}");
                return Ok(new { adminId = adminUser.Id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting admin ID");
                return StatusCode(500, "An error occurred while getting admin ID");
            }
        }

        [HttpPut("messages/{id}")]
        public async Task<IActionResult> UpdateMessage(int id, [FromBody] UpdateMessageViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("UpdateMessage called with invalid model state");
                    return BadRequest(ModelState);
                }

                var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    _logger.LogWarning("UpdateMessage called without user ID");
                    return Unauthorized("User not authenticated");
                }

                _logger.LogInformation($"Updating message {id} for user {userId}");
                var updatedMessage = await _chatService.UpdateMessageAsync(id, model.Content, userId);

                // Notify both users about the update
                await _hubContext.Clients.Group(updatedMessage.SenderId).SendAsync("MessageUpdated", updatedMessage);
                await _hubContext.Clients.Group(updatedMessage.ReceiverId).SendAsync("MessageUpdated", updatedMessage);

                _logger.LogInformation($"Message {id} updated successfully");
                return Ok(updatedMessage);
            }
            catch (KeyNotFoundException)
            {
                _logger.LogWarning($"Message {id} not found");
                return NotFound("Message not found");
            }
            catch (UnauthorizedAccessException)
            {
                _logger.LogWarning($"Unauthorized attempt to update message {id}");
                return Forbid();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating message {id}");
                return StatusCode(500, "An error occurred while updating the message");
            }
        }

        [HttpDelete("messages/{id}")]
        public async Task<IActionResult> DeleteMessage(int id)
        {
            try
            {
                var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    _logger.LogWarning("DeleteMessage called without user ID");
                    return Unauthorized("User not authenticated");
                }

                _logger.LogInformation($"Deleting message {id} for user {userId}");
                var result = await _chatService.DeleteMessageAsync(id, userId);

                if (!result)
                {
                    _logger.LogWarning($"Message {id} not found");
                    return NotFound("Message not found");
                }

                // Notify users about the deletion
                await _hubContext.Clients.All.SendAsync("MessageDeleted", id);

                _logger.LogInformation($"Message {id} deleted successfully");
                return Ok();
            }
            catch (UnauthorizedAccessException)
            {
                _logger.LogWarning($"Unauthorized attempt to delete message {id}");
                return Forbid();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting message {id}");
                return StatusCode(500, "An error occurred while deleting the message");
            }
        }
    }
}