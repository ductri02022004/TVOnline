using Microsoft.EntityFrameworkCore;
using TVOnline.Data;
using TVOnline.Models;

namespace TVOnline.Repository.Posts
{
    public class PostRepository(AppDbContext context) : IPostRepository
    {
        private readonly AppDbContext _context = context;

        public async Task<Post?> FindPostById(string id) => await _context.Posts.Include(p => p.Employer).Include(p => p.City).FirstOrDefaultAsync(p => p.PostId == id);
    }
}
