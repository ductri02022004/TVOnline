using TVOnline.Data;
using TVOnline.Repository.Posts;
using Microsoft.EntityFrameworkCore;
using TVOnline.Data;
using TVOnline.Models;
using TVOnline.Repository.UserCVs;

namespace TVOnline.Repository.UserCVs
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
        public async Task<List<UserCV>> GetCvsByUserId(string userId)
        {
            return await _context.UserCVs
                .Include(c => c.Post)
                .Where(c => c.UserId == userId)
                .ToListAsync();
        }

        public async Task<UserCV> GetCvByUserAndPost(string userId, string postId)
        {
            return await _context.UserCVs
                .Include(c => c.Post)
                .FirstOrDefaultAsync(c => c.UserId == userId && c.PostId == postId);
        }

        public async Task<UserCV> GetCvById(string cvId)
        {
            return await _context.UserCVs
                .Include(c => c.Post)
                .Include(c => c.Users)
                .FirstOrDefaultAsync(c => c.CvID == cvId);
        }

        public async Task<UserCV> UpdateCvStatus(string cvId, string status)
        {
            var cv = await _context.UserCVs.FindAsync(cvId);
            if (cv != null)
            {
                cv.CVStatus = status;
                await _context.SaveChangesAsync();
            }
            return cv;
        }

        public async Task<UserCV> UpdateCvNotes(string cvId, string notes)
        {
            var cv = await _context.UserCVs.FindAsync(cvId);
            if (cv != null)
            {
                cv.EmployerNotes = notes;
                await _context.SaveChangesAsync();
            }
            return cv;
        }
    }
}
