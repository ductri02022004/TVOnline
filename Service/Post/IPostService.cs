using TVOnline.Service.DTO;

namespace TVOnline.Service.Post
{
    public interface IPostService
    {
        Task<PostResponse?> FindPostById(string? id);
        Task<List<PostResponse>> GetAllPosts();
        Task<List<PostResponse>> GetSeveralPosts(int quantity);
    }
}
