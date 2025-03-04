using TVOnline.Models;
using TVOnline.Service.DTO;

namespace TVOnline.Service.UserCVs
{
    public class UserCvService(IUserCvRepository userCvRepository) : IUserCvService
    {
        private readonly IUserCvRepository _userCvRepository = userCvRepository;
        public async Task<UserCvResponse> SaveCv(UserCvAddRequest? cv)
        {
            if (cv == null) throw new ArgumentNullException(nameof(cv));
            UserCV userCv = cv.ToUserCv();
            await _userCvRepository.AddCv(userCv);
            return userCv.ToUserCvResponse();
        }

        public async Task<List<UserCV>> GetApplicationsByUser(string userId)
        {
            return await _userCvRepository.GetCvsByUserId(userId);
        }

        public async Task<UserCV> GetApplicationByUserAndPost(string userId, string postId)
        {
            return await _userCvRepository.GetCvByUserAndPost(userId, postId);
        }

        public async Task<UserCV> GetApplicationById(string cvId)
        {
            return await _userCvRepository.GetCvById(cvId);
        }

        public async Task<UserCV> UpdateApplicationStatus(string cvId, string status)
        {
            return await _userCvRepository.UpdateCvStatus(cvId, status);
        }

        public async Task<UserCV> UpdateEmployerNotes(string cvId, string notes)
        {
            return await _userCvRepository.UpdateCvNotes(cvId, notes);
        }
    }
}
