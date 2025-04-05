using TVOnline.Models;

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
        Task<List<SavedJob>> GetSavedPostsForJobSeeker(string jobSeekerId);
        Task<List<Post>> SearchPosts(string keyword, int? cityId, int page, int pageSize);
        Task<int> CountSearchPosts(string keyword, int? cityId);
        Task<List<Post>> FilterPosts(string keyword, int? cityId, decimal? minSalary, decimal? maxSalary, int? minExperience, int? maxExperience, int page, int pageSize);
        Task<int> CountFilteredPosts(string keyword, int? cityId, decimal? minSalary, decimal? maxSalary, int? minExperience, int? maxExperience);
        Task<List<Post>> GetPostsByEmployerId(string employerId);
    }
}
