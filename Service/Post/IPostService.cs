using TVOnline.Service.DTO;

namespace TVOnline.Service.Post
{
    public interface IPostService
    {
        Task<PostResponse?> FindPostById(string? id);
        Task<List<PostResponse>> GetAllPosts(string userId);
        Task<List<PostResponse>> GetSeveralPosts(int quantity);
        Task<bool> SaveJobForJobSeeker(string postId, string userId);
        Task<List<SavedJobResponse>> GetSavedPostsForJobSeeker(string userId);
        Task<bool> DeleteSavedJobForJobSeeker(string postId, string userId);
    }
}
