using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TVOnline.Data;
using TVOnline.Models;
using Microsoft.AspNetCore.Identity;

namespace TVOnline.Service
{
    public class ChatService : IChatService
    {
        private readonly AppDbContext _context;
        private readonly UserManager<Users> _userManager;

        public ChatService(AppDbContext context, UserManager<Users> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IEnumerable<ChatMessage>> GetChatHistoryAsync(string senderId, string receiverId)
        {
            if (string.IsNullOrEmpty(senderId))
                throw new ArgumentNullException(nameof(senderId), "Sender ID cannot be null or empty");

            if (string.IsNullOrEmpty(receiverId))
                throw new ArgumentNullException(nameof(receiverId), "Receiver ID cannot be null or empty");

            try
            {
                // Check if users exist
                var sender = await _userManager.FindByIdAsync(senderId);
                var receiver = await _userManager.FindByIdAsync(receiverId);

                if (sender == null)
                    throw new InvalidOperationException($"User with ID {senderId} was not found");

                if (receiver == null)
                    throw new InvalidOperationException($"User with ID {receiverId} was not found");

                // Get messages between the two users
                var messages = await _context.ChatMessages
                    .Where(m => (m.SenderId == senderId && m.ReceiverId == receiverId) ||
                                (m.SenderId == receiverId && m.ReceiverId == senderId))
                    .OrderBy(m => m.Timestamp)
                    .Select(m => new ChatMessage
                    {
                        Id = m.Id,
                        Content = m.Content,
                        SenderId = m.SenderId,
                        ReceiverId = m.ReceiverId,
                        Timestamp = m.Timestamp,
                        IsRead = m.IsRead
                        // IsEdited is intentionally omitted as it doesn't exist in database
                    })
                    .ToListAsync();

                // Set IsEdited property manually for client-side use
                foreach (var message in messages)
                {
                    message.IsEdited = false; // Default to false
                }

                return messages;
            }
            catch (ArgumentNullException)
            {
                throw;
            }
            catch (InvalidOperationException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to retrieve chat history: " + ex.Message, ex);
            }
        }

        public async Task<ChatMessage> SaveMessageAsync(ChatMessage message)
        {
            await _context.ChatMessages.AddAsync(message);
            await _context.SaveChangesAsync();
            return message;
        }

        public async Task<bool> MarkAsReadAsync(string senderId, string receiverId)
        {
            if (string.IsNullOrEmpty(senderId))
                throw new ArgumentNullException(nameof(senderId), "Sender ID cannot be null or empty");

            if (string.IsNullOrEmpty(receiverId))
                throw new ArgumentNullException(nameof(receiverId), "Receiver ID cannot be null or empty");

            try
            {
                // Get all unread messages from senderId to receiverId
                var messages = await _context.ChatMessages
                    .Where(m => m.SenderId == senderId &&
                             m.ReceiverId == receiverId &&
                             m.IsRead == false)
                    .ToListAsync();

                // Mark all as read
                foreach (var message in messages)
                {
                    message.IsRead = true;
                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch (DbUpdateException ex)
            {
                throw new InvalidOperationException("Failed to mark messages as read: " + ex.Message, ex);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("An error occurred while marking messages as read: " + ex.Message, ex);
            }
        }

        public async Task<int> GetUnreadCountAsync(string userId)
        {
            return await _context.ChatMessages
                .Where(m => m.ReceiverId == userId && !m.IsRead)
                .CountAsync();
        }

        public async Task<int> GetUnreadCountFromUserAsync(string senderId, string receiverId)
        {
            return await _context.ChatMessages
                .Where(m => m.SenderId == senderId && m.ReceiverId == receiverId && !m.IsRead)
                .CountAsync();
        }

        public async Task<List<string>> GetUserIdsWithChatHistoryAsync(string userId)
        {
            // Check if userId is valid
            if (string.IsNullOrEmpty(userId))
            {
                Console.WriteLine("Warning: Empty userId passed to GetUserIdsWithChatHistoryAsync");
                return new List<string>();
            }

            try
            {
                // Thêm logging để debug
                Console.WriteLine($"GetUserIdsWithChatHistoryAsync called for userId: {userId}");
                
                // Kiểm tra xem có bản ghi nào trong bảng ChatMessages không
                var totalMessages = await _context.ChatMessages.CountAsync();
                Console.WriteLine($"Total messages in database: {totalMessages}");
                
                // Kiểm tra xem có tin nhắn nào liên quan đến userId không
                var userMessages = await _context.ChatMessages
                    .AsNoTracking()
                    .Where(m => m.SenderId == userId || m.ReceiverId == userId)
                    .CountAsync();
                Console.WriteLine($"Messages related to user {userId}: {userMessages}");

                // Use one optimized query instead of multiple queries
                var userIds = await _context.ChatMessages
                    .AsNoTracking()
                    .Where(m => m.SenderId == userId || m.ReceiverId == userId)
                    .Select(m => m.SenderId == userId ? m.ReceiverId : m.SenderId)
                    .Where(id => id != userId && !string.IsNullOrEmpty(id)) // Filter out current user and empty IDs
                    .Distinct()
                    .ToListAsync();

                Console.WriteLine($"Found {userIds.Count} distinct user IDs with chat history for user {userId}");

                // Return an empty list if none found
                if (userIds.Count == 0)
                {
                    Console.WriteLine($"No users with chat history found for user {userId}");

                    // Double check if there are messages but no users
                    var messageCount = await _context.ChatMessages
                        .AsNoTracking()
                        .Where(m => m.SenderId == userId || m.ReceiverId == userId)
                        .CountAsync();

                    Console.WriteLine($"Cross-check: found {messageCount} total messages for user {userId}");

                    if (messageCount > 0)
                    {
                        // If we have messages but no users, there's likely an issue - retry with explicit query
                        Console.WriteLine("Inconsistency detected. Retrying with explicit query...");

                        var explicitUserIds = await _context.ChatMessages
                            .AsNoTracking()
                            .Where(m => (m.SenderId == userId && !string.IsNullOrEmpty(m.ReceiverId) && m.ReceiverId != userId) ||
                                        (m.ReceiverId == userId && !string.IsNullOrEmpty(m.SenderId) && m.SenderId != userId))
                            .Select(m => m.SenderId == userId ? m.ReceiverId : m.SenderId)
                            .Distinct()
                            .ToListAsync();

                        Console.WriteLine($"Explicit query found {explicitUserIds.Count} user IDs");
                        return explicitUserIds;
                    }
                }

                return userIds;
            }
            catch (Exception ex)
            {
                // Log the error but return empty list to prevent crashing
                Console.WriteLine($"Error getting user IDs with chat history: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                return new List<string>();
            }
        }

        public async Task<ChatMessage> UpdateMessageAsync(int messageId, string content, string userId, bool isAdmin = false)
        {
            if (string.IsNullOrEmpty(content))
                throw new ArgumentNullException(nameof(content), "Message content cannot be null or empty");

            if (string.IsNullOrEmpty(userId))
                throw new ArgumentNullException(nameof(userId), "User ID cannot be null or empty");

            try
            {
                var message = await _context.ChatMessages.FindAsync(messageId);
                if (message == null)
                    throw new KeyNotFoundException($"Message with ID {messageId} was not found");

                // Verify authorization
                if (!isAdmin && message.SenderId != userId)
                    throw new UnauthorizedAccessException("You can only edit your own messages");

                // Get the original content before updating
                string originalContent = message.Content;

                // Update message content
                message.Content = content;

                // Track changes
                _context.Entry(message).State = EntityState.Modified;

                await _context.SaveChangesAsync();

                // Manually create a copy with IsEdited=true for the client
                var messageDto = new ChatMessage
                {
                    Id = message.Id,
                    Content = message.Content,
                    SenderId = message.SenderId,
                    ReceiverId = message.ReceiverId,
                    Timestamp = message.Timestamp,
                    IsRead = message.IsRead,
                    IsEdited = true // Set this for client-side UI, but it's not saved to DB
                };

                return messageDto;
            }
            catch (DbUpdateConcurrencyException ex)
            {
                // Handle the case where the message has been modified by another user
                throw new InvalidOperationException("The message was modified by another user. Please try again.", ex);
            }
            catch (DbUpdateException ex)
            {
                throw new InvalidOperationException("Failed to update message: " + ex.Message, ex);
            }
            catch (KeyNotFoundException)
            {
                throw;
            }
            catch (UnauthorizedAccessException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("An error occurred while updating the message: " + ex.Message, ex);
            }
        }

        public async Task<bool> DeleteMessageAsync(int messageId, string userId, bool forceDelete = false)
        {
            if (string.IsNullOrEmpty(userId))
                throw new ArgumentNullException(nameof(userId), "User ID cannot be null or empty");

            try
            {
                var message = await _context.ChatMessages.FindAsync(messageId);
                if (message == null)
                    return false;

                // Check if user is authorized to delete this message
                if (!forceDelete && message.SenderId != userId)
                    throw new UnauthorizedAccessException("You can only delete your own messages");

                // Remove message from DbSet
                _context.ChatMessages.Remove(message);

                // Save changes explicitly with try-catch for better error handling
                try
                {
                    await _context.SaveChangesAsync();
                    return true;
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    throw new InvalidOperationException("The message was modified by another user. Please try again.", ex);
                }
                catch (DbUpdateException ex)
                {
                    throw new InvalidOperationException($"Database error when deleting message: {ex.Message}", ex);
                }
            }
            catch (ArgumentNullException)
            {
                throw;
            }
            catch (UnauthorizedAccessException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("An error occurred while deleting the message: " + ex.Message, ex);
            }
        }

        public async Task<ChatMessage> GetMessageByIdAsync(int messageId)
        {
            return await _context.ChatMessages.FindAsync(messageId);
        }
    }
}