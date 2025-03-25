using TVOnline.Models;
using TVOnline.Service.UserCVs;

namespace TVOnline.Repository.UserCVs
{
    public class UserCvService(IUserCvRepository userCvRepository) : IUserCvService
    {
        private readonly IUserCvRepository _userCvRepository = userCvRepository;
        public async Task SaveCv(UserCV cv)
        {
            await _userCvRepository.AddCv(cv);
        }
    }
}
