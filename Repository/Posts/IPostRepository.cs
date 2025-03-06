namespace TVOnline.Repository.Posts
{
    public interface IPostRepository
    {
        Task<Post?> FindPostById(string id);
        Task<List<Post>> GetAllPosts();
        Task<List<Post>> GetSeveralPosts(int quantity);
        Task<SavedJob?> FindSavedJob(string postId, string userId);
        Task<SavedJob> AddSavedJob(SavedJob savedJob);
        Task<bool> DeleteSavedJob(SavedJob savedJob);
        Task<List<SavedJob>> GetSavedPostsForJobSeeker(string userId);
    }
}
