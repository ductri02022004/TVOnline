using TVOnline.Repository.Posts;

namespace TVOnline.Service.Post
{
    public class PostService(IPostRepository postRepository) : IPostService
    {
        private readonly IPostRepository _postRepository = postRepository;
    }
}
