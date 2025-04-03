using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using TVOnline.Models;
using TVOnline.Service;

namespace TVOnline.Hubs
{
    [Authorize]
    public class ChatHub : Hub
    {
        private readonly IChatService _chatService;
        private readonly ILogger<ChatHub> _logger;

        public ChatHub(
            IChatService chatService,
            ILogger<ChatHub> logger)
        {
            _chatService = chatService;
            _logger = logger;
        }

        // Lưu kết nối của người dùng vào một group riêng
        public async Task JoinGroup(string userId)
        {
            try
            {
                _logger.LogInformation($"User {userId} joining group");
                await Groups.AddToGroupAsync(Context.ConnectionId, userId);
                _logger.LogInformation($"User {userId} joined group successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error joining group for user {userId}");
                throw;
            }
        }

        // Rời khỏi group
        public async Task LeaveGroup(string userId)
        {
            try
            {
                _logger.LogInformation($"User {userId} leaving group");
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, userId);
                _logger.LogInformation($"User {userId} left group successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error leaving group for user {userId}");
                throw;
            }
        }

        // Gửi tin nhắn giữa người dùng và admin
        public async Task SendMessage(string senderId, string receiverId, string message)
        {
            try
            {
                if (string.IsNullOrEmpty(message))
                {
                    _logger.LogWarning($"Empty message from user {senderId}");
                    return;
                }

                _logger.LogInformation($"Sending message from {senderId} to {receiverId}");

                var chatMessage = new ChatMessage
                {
                    SenderId = senderId,
                    ReceiverId = receiverId,
                    Content = message,
                    Timestamp = DateTime.Now,
                    IsRead = false
                };

                // Lưu tin nhắn vào database
                var savedMessage = await _chatService.SaveMessageAsync(chatMessage);
                _logger.LogInformation($"Message saved successfully with ID {savedMessage.Id}");

                // Gửi tin nhắn đến người nhận và người gửi (để hiển thị trên cả hai phía)
                await Clients.Group(receiverId).SendAsync("ReceiveMessage", savedMessage);
                await Clients.Group(senderId).SendAsync("ReceiveMessage", savedMessage);
                _logger.LogInformation($"Message sent to groups {senderId} and {receiverId}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error sending message from {senderId} to {receiverId}");
                throw;
            }
        }

        // Đánh dấu tin nhắn đã đọc
        public async Task MarkAsRead(string senderId, string receiverId)
        {
            await _chatService.MarkAsReadAsync(senderId, receiverId);

            // Cập nhật số lượng tin nhắn chưa đọc
            var unreadCount = await _chatService.GetUnreadCountAsync(receiverId);
            await Clients.Group(receiverId).SendAsync("UpdateUnreadCount", unreadCount);
        }

        // Gửi tin nhắn đến admin
        public async Task SendMessageToAdmin(string message)
        {
            var userId = Context.UserIdentifier;

            if (string.IsNullOrEmpty(userId))
            {
                throw new HubException("User không xác định");
            }

            try
            {
                // Lưu tin nhắn vào database
                var chatMessage = new ChatMessage
                {
                    SenderId = userId,
                    ReceiverId = "admin",
                    Content = message,
                    Timestamp = DateTime.Now,
                    IsRead = false
                };

                var savedMessage = await _chatService.SaveMessageAsync(chatMessage);

                // Gửi tin nhắn đến admin
                await Clients.User("admin").SendAsync("ReceiveMessage", new
                {
                    id = savedMessage.Id,
                    senderId = savedMessage.SenderId,
                    content = savedMessage.Content,
                    timestamp = savedMessage.Timestamp,
                    isFromAdmin = false
                });

                // Xác nhận với người gửi
                await Clients.Caller.SendAsync("MessageSent", new
                {
                    id = savedMessage.Id,
                    content = savedMessage.Content,
                    timestamp = savedMessage.Timestamp,
                    isFromAdmin = false
                });
            }
            catch (Exception ex)
            {
                await Clients.Caller.SendAsync("ErrorOccurred", ex.Message);
                throw new HubException($"Có lỗi khi gửi tin nhắn: {ex.Message}");
            }
        }

        // Admin gửi tin nhắn đến user
        [Authorize(Roles = "Admin")]
        public async Task SendMessageToUser(string userId, string message)
        {
            if (string.IsNullOrEmpty(userId))
            {
                throw new HubException("User không xác định");
            }

            try
            {
                // Lưu tin nhắn vào database
                var chatMessage = new ChatMessage
                {
                    SenderId = "admin",
                    ReceiverId = userId,
                    Content = message,
                    Timestamp = DateTime.Now,
                    IsRead = false
                };

                var savedMessage = await _chatService.SaveMessageAsync(chatMessage);

                // Gửi tin nhắn đến user
                await Clients.User(userId).SendAsync("ReceiveMessage", new
                {
                    id = savedMessage.Id,
                    senderId = savedMessage.SenderId,
                    content = savedMessage.Content,
                    timestamp = savedMessage.Timestamp,
                    isFromAdmin = true
                });

                // Xác nhận với admin
                await Clients.Caller.SendAsync("MessageSent", new
                {
                    id = savedMessage.Id,
                    userId = userId,
                    content = savedMessage.Content,
                    timestamp = savedMessage.Timestamp,
                    isFromAdmin = true
                });
            }
            catch (Exception ex)
            {
                await Clients.Caller.SendAsync("ErrorOccurred", ex.Message);
                throw new HubException($"Có lỗi khi gửi tin nhắn: {ex.Message}");
            }
        }

        // Kết nối
        public override async Task OnConnectedAsync()
        {
            try
            {
                _logger.LogInformation($"Client connected: {Context.ConnectionId}");
                await base.OnConnectedAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error in OnConnectedAsync for connection {Context.ConnectionId}");
                throw;
            }
        }

        // Ngắt kết nối
        public override async Task OnDisconnectedAsync(Exception exception)
        {
            try
            {
                _logger.LogInformation($"Client disconnected: {Context.ConnectionId}");
                if (exception != null)
                {
                    _logger.LogError(exception, $"Client disconnected with error: {Context.ConnectionId}");
                }
                await base.OnDisconnectedAsync(exception);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error in OnDisconnectedAsync for connection {Context.ConnectionId}");
                throw;
            }
        }
    }
}