using TVOnline.Models;
namespace TVOnline.Repository.Posts
{
    public interface IPostRepository
    {
        Task<Post?> FindPostById(string id);
    }
}
