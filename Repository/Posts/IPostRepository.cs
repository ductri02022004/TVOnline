using TVOnline.Models;
namespace TVOnline.Repository.Posts
{
    public interface IPostRepository
    {
        Task<Post?> FindPostById(string id);
        Task<List<Post>> GetAllPosts();
        Task<List<Post>> GetSeveralPosts(int quantity);
    }
}
