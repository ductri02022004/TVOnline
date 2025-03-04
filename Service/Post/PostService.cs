using TVOnline.Repository.Posts;
using TVOnline.Service.DTO;

namespace TVOnline.Service.Post
{
    public class PostService(IPostRepository postRepository) : IPostService
    {
        private readonly IPostRepository _postRepository = postRepository;

        public async Task<PostResponse?> FindPostById(string? id)
        {
            if (id == null) return null;
            var post = await _postRepository.FindPostById(id);
            return post?.ToPostResponse();
        }

        public async Task<List<PostResponse>> GetAllPosts()
        {
            var posts = await _postRepository.GetAllPosts();
            return posts.Select(p => p.ToPostResponse()).ToList();
        }

        public async Task<List<PostResponse>> GetSeveralPosts(int quantity)
        {
            if (quantity < 1) throw new ArgumentException("Quantity must be greater than 0");
            var posts = await _postRepository.GetSeveralPosts(quantity);
            return posts.Select(p => p.ToPostResponse()).ToList();
        }
    }
}
