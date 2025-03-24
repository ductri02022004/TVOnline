using TVOnline.Models;

namespace TVOnline.Service.UserCVs
{
    public interface IUserCvRepository
    {
        Task AddCv(UserCV cv);
    }
}
