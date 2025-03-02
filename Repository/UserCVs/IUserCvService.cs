using TVOnline.Models;

namespace TVOnline.Repository.UserCVs
{
    public interface IUserCvService
    {
        Task SaveCv(UserCV cv);
    }
}
