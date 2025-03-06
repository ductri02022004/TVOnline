using TVOnline.Data;
using TVOnline.Repository.Posts;

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

        public async Task<List<UserCV>> GetUserCVsByUserIdAsync(string userId)
        {
            return await _context.UserCVs
                .Where(cv => cv.UserId == userId)
                .ToListAsync();
        }

        public async Task<List<Models.Post>> GetPostsByUserCVsAsync(List<string> postIds)
        {
            return await _context.Posts
                .Where(post => postIds.Contains(post.PostId))
                .Include(post => post.Employer)
                .ToListAsync();
        }

        public async Task<bool> DeleteUserCvAsync(string userId, string postId)
        {
            var userCv = await _context.UserCVs
                .FirstOrDefaultAsync(cv => cv.UserId == userId && cv.PostId == postId);

            if (userCv == null)
                return false; // Không tìm thấy UserCv

            _context.UserCVs.Remove(userCv);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
