using Microsoft.EntityFrameworkCore;
using TVOnline.Data;

namespace TVOnline.Repository.Posts
{
    public class PostRepository(AppDbContext context) : IPostRepository
    {
        private readonly AppDbContext _context = context;

        public async Task<Post?> FindPostById(string id) => await _context.Posts.Include(p => p.Employer).Include(p => p.City).FirstOrDefaultAsync(p => p.PostId == id);

        public async Task<List<Post>> GetAllPosts() => await _context.Posts
            .Include(p => p.Employer)
            .Include(p => p.City).ToListAsync();

        public async Task<List<Post>> GetSeveralPosts(int quantity) => await _context.Posts
            .Include(p => p.Employer)
            .Include(p => p.City)
            .OrderByDescending(p => p.CreatedAt)
            .Take(quantity)
            .ToListAsync();

        public async Task<SavedJob?> FindSavedJob(string postId, string userId) =>
            await _context.SavedJobs.FirstOrDefaultAsync(
                s => s.PostId == postId && s.UserId == userId);

        public async Task<SavedJob> AddSavedJob(SavedJob savedJob)
        {
            await _context.SavedJobs.AddAsync(savedJob);
            await _context.SaveChangesAsync();
            return savedJob;
        }
        public async Task<bool> DeleteSavedJob(SavedJob savedJob)
        {
            _context.SavedJobs.Remove(savedJob);
            var result = await _context.SaveChangesAsync();
            return result > 0;
        }

        public async Task<List<SavedJob>> GetSavedPostsForJobSeeker(string jobSeekerId) // Corrected return type!
        {
            return await _context.SavedJobs
                .Where(s => s.UserId == jobSeekerId)
                .Include(s => s.Post) // Eager load SavedJob.Post navigation property
                .ThenInclude(p => p.Employer) // Then eager load Post.Employer navigation property
                .Include(s => s.Post) // Include Post again to eager load City
                .ThenInclude(p => p.City) // Then eager load Post.City navigation property
                .ToListAsync(); // Now returning List<SavedJob> directly!
        }
    }
}
