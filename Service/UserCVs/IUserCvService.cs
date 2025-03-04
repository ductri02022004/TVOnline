using TVOnline.Models;
using TVOnline.Service.DTO;

namespace TVOnline.Service.UserCVs
{
    public interface IUserCvService
    {
        Task<UserCvResponse> SaveCv(UserCvAddRequest? cv);
        Task<List<UserCV>> GetApplicationsByUser(string userId);
        Task<UserCV> GetApplicationByUserAndPost(string userId, string postId);
        Task<UserCV> GetApplicationById(string cvId);
        Task<UserCV> UpdateApplicationStatus(string cvId, string status);
        Task<UserCV> UpdateEmployerNotes(string cvId, string notes);
    }
}
