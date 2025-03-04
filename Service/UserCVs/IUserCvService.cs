using TVOnline.Models;
using TVOnline.Service.DTO;

namespace TVOnline.Service.UserCVs
{
    public interface IUserCvService
    {
        Task<UserCvResponse> SaveCv(UserCvAddRequest? cv);
    }
}
