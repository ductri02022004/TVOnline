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
    }
}
