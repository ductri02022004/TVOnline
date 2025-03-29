using System.Collections.Generic;
using System.Threading.Tasks;
using TVOnline.Models;

namespace TVOnline.Service
{
    public interface IChatService
    {
        Task<IEnumerable<ChatMessage>> GetChatHistoryAsync(string userId, string otherUserId);
        Task<ChatMessage> SaveMessageAsync(ChatMessage message);
        Task<bool> MarkAsReadAsync(string senderId, string receiverId);
        Task<int> GetUnreadCountAsync(string userId);
        Task<int> GetUnreadCountFromUserAsync(string senderId, string receiverId);
        Task<List<string>> GetUserIdsWithChatHistoryAsync(string userId);
        Task<ChatMessage> UpdateMessageAsync(int messageId, string newContent, string userId);
        Task<bool> DeleteMessageAsync(int messageId, string userId);
    }
}