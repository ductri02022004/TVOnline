using TVOnline.Service.DTO;

namespace TVOnline.Service.Post
{
    public interface IPostService
    {
        Task<PostResponse?> FindPostById(string? id);
        Task<List<PostResponse>> GetAllPosts(string? userId);
        Task<List<PostResponse>> GetSeveralPosts(int quantity);
        Task<bool> SaveJobForJobSeeker(string postId, string userId);
        Task<List<SavedJobResponse>> GetSavedPostsForJobSeeker(string userId);
        Task<bool> DeleteSavedJobForJobSeeker(string postId, string userId);
        Task<List<PostResponse>> SearchPosts(string keyword, int? cityId, int page, int pageSize);
        Task<int> CountSearchPosts(string keyword, int? cityId);
        Task<List<PostResponse>> FilterPosts(string keyword, int? cityId, decimal? minSalary, decimal? maxSalary, int? minExperience, int? maxExperience, int page, int pageSize, string? userId = null);
        Task<int> CountFilteredPosts(string keyword, int? cityId, decimal? minSalary, decimal? maxSalary, int? minExperience, int? maxExperience);
    }
}
