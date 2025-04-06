using TVOnline.Service.DTO;
using TVOnline.Models;

namespace TVOnline.Service.UserCVs
{
        public interface IUserCvService
        {
                Task<UserCvResponse> SaveCv(UserCvAddRequest? cv);
                Task<List<AppliedJob>> GetAppliedJobsByUserIdAsync(string userId);
                Task<bool> CancelAppliedJobAsync(string userId, string postId);
                Task<List<UserCV>> GetApplicationsByUser(string userId);
                Task<UserCV> GetApplicationByUserAndPost(string userId, string postId);
                Task<UserCV> GetApplicationById(string cvId);
                Task<UserCV> UpdateApplicationStatus(string cvId, string status);
                Task<UserCV> UpdateEmployerNotes(string cvId, string notes);
                Task<int> GetUserDailyApplicationCount(string userId);
                Task<bool> CanUserApplyToday(string userId, bool isPremiumUser);
        }
}
