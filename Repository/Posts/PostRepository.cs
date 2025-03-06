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
    }
}
