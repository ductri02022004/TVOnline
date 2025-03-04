using TVOnline.Data;
using TVOnline.Models;

namespace TVOnline.Service.UserCVs
{
    public class UserCvRepository(AppDbContext context) : IUserCvRepository
    {
        private readonly AppDbContext _context = context;
        public async Task<UserCV> AddCv(UserCV cv)
        {
            _context.UserCVs.Add(cv);
            await _context.SaveChangesAsync();
            return cv;
        }

    }
}
