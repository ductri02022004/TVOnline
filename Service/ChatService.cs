using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TVOnline.Data;
using TVOnline.Models;

namespace TVOnline.Service
{
    public class ChatService : IChatService
    {
        private readonly AppDbContext _context;

        public ChatService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<ChatMessage>> GetChatHistoryAsync(string userId, string otherUserId)
        {
            return await _context.ChatMessages
                .Where(m => (m.SenderId == userId && m.ReceiverId == otherUserId) ||
                           (m.SenderId == otherUserId && m.ReceiverId == userId))
                .OrderBy(m => m.Timestamp)
                .ToListAsync();
        }

        public async Task<ChatMessage> SaveMessageAsync(ChatMessage message)
        {
            await _context.ChatMessages.AddAsync(message);
            await _context.SaveChangesAsync();
            return message;
        }

        public async Task<bool> MarkAsReadAsync(string senderId, string receiverId)
        {
            var unreadMessages = await _context.ChatMessages
                .Where(m => m.SenderId == senderId && m.ReceiverId == receiverId && !m.IsRead)
                .ToListAsync();

            if (unreadMessages.Any())
            {
                foreach (var message in unreadMessages)
                {
                    message.IsRead = true;
                }
                await _context.SaveChangesAsync();
                return true;
            }
            return false;
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
            // Get unique users who have sent or received messages from/to this user
            var userIds = await _context.ChatMessages
                .Where(m => m.SenderId == userId || m.ReceiverId == userId)
                .Select(m => m.SenderId == userId ? m.ReceiverId : m.SenderId)
                .Distinct()
                .ToListAsync();

            return userIds;
        }

        public async Task<ChatMessage> UpdateMessageAsync(int messageId, string newContent, string userId)
        {
            var message = await _context.ChatMessages.FindAsync(messageId);

            if (message == null)
                throw new KeyNotFoundException("Message not found");

            if (message.SenderId != userId)
                throw new UnauthorizedAccessException("You can only edit your own messages");

            message.Content = newContent;
            await _context.SaveChangesAsync();
            return message;
        }

        public async Task<bool> DeleteMessageAsync(int messageId, string userId)
        {
            var message = await _context.ChatMessages.FindAsync(messageId);

            if (message == null)
                return false;

            if (message.SenderId != userId)
                throw new UnauthorizedAccessException("You can only delete your own messages");

            _context.ChatMessages.Remove(message);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}