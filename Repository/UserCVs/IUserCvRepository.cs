using TVOnline.Models;

namespace TVOnline.Service.UserCVs
{
    public interface IUserCvRepository
    {
        Task<UserCV> AddCv(UserCV cv);
    }
}
