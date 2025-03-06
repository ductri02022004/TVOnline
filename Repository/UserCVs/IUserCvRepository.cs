using TVOnline.Models;

namespace TVOnline.Service.UserCVs
{
    public interface IUserCvRepository
    {
        Task<UserCV> AddCv(UserCV cv);
        Task<List<UserCV>> GetCvsByUserId(string userId);
        Task<UserCV> GetCvByUserAndPost(string userId, string postId);
        Task<UserCV> GetCvById(string cvId);
        Task<UserCV> UpdateCvStatus(string cvId, string status);
        Task<UserCV> UpdateCvNotes(string cvId, string notes);
    }
}
