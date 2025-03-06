namespace TVOnline.Service.UserCVs
{
    public interface IUserCvRepository
    {
        Task<UserCV> AddCv(UserCV cv);
        Task<List<UserCV>> GetUserCVsByUserIdAsync(string userId);
        Task<List<Models.Post>> GetPostsByUserCVsAsync(List<string> postIds);
        public Task<bool> DeleteUserCvAsync(string userId, string postId);
    }
}
