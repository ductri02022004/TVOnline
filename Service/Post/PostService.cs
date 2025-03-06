using TVOnline.Models;
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

        public async Task<List<PostResponse>> GetAllPosts(string userId)
        {
            var posts = await _postRepository.GetAllPosts();
            var postResponses = posts.Select(p => p.ToPostResponse()).ToList();
            //return posts.Select(p => p.ToPostResponse()).ToList();
            foreach (var postResponse in postResponses)
            {
                var savedJob = await _postRepository.FindSavedJob(postResponse.PostId, userId);
                postResponse.IsSaved = savedJob != null;
            }
            return postResponses;
        }

        public async Task<List<PostResponse>> GetSeveralPosts(int quantity)
        {
            if (quantity < 1) throw new ArgumentException("Quantity must be greater than 0");
            var posts = await _postRepository.GetSeveralPosts(quantity);
            return posts.Select(p => p.ToPostResponse()).ToList();
        }

        public async Task<bool> SaveJobForJobSeeker(string postId, string userId)
        {
            if (string.IsNullOrEmpty(postId) || string.IsNullOrEmpty(userId))
                return false;

            var existingSavedJob = await _postRepository.FindSavedJob(postId, userId);
            if (existingSavedJob != null)
                return false;

            var savedJob = new SavedJob
            {
                SavedJobId = Guid.NewGuid().ToString(),
                PostId = postId,
                UserId = userId,
                SavedAt = DateTime.Now
            };

            await _postRepository.AddSavedJob(savedJob);
            return true;
        }

        public async Task<List<SavedJobResponse>> GetSavedPostsForJobSeeker(string userId)
        {
            var savedPosts = await _postRepository.GetSavedPostsForJobSeeker(userId);
            return savedPosts.Select(savedJob => savedJob.ToSavedJobResponse()).ToList(); // Use ToSavedJobResponse extension
        }

        public async Task<bool> DeleteSavedJobForJobSeeker(string postId, string userId)
        {
            if (string.IsNullOrEmpty(postId) || string.IsNullOrEmpty(userId))
            {
                return false;
            }

            var existingSavedJob = await _postRepository.FindSavedJob(postId, userId);
            if (existingSavedJob == null)
            {
                return false;
            }

            return await _postRepository.DeleteSavedJob(existingSavedJob);
        }
    }
}
