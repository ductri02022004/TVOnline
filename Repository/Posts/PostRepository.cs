using TVOnline.Data;
using TVOnline.Models;

namespace TVOnline.Repository.Posts
{
    public class PostRepository(AppDbContext context) : IPostRepository
    {
        private readonly AppDbContext _context = context;

        public Post? FindPostById(int id) => _context.Posts.FirstOrDefault(post => post.PostId == id);
    }
}
