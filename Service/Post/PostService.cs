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
    }
}
