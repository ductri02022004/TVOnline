using TVOnline.Service.DTO;

namespace TVOnline.Service.UserCVs
{
    public interface IUserCvService
    {
        Task<UserCvResponse> SaveCv(UserCvAddRequest? cv);
        Task<List<AppliedJob>> GetAppliedJobsByUserIdAsync(string userId);
        Task<bool> CancelAppliedJobAsync(string userId, string postId);
    }
}
