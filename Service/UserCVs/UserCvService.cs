using TVOnline.Service.DTO;
using TVOnline.Models;
using TVOnline.Repository.UserCVs;

namespace TVOnline.Service.UserCVs
{
    public class UserCvService(IUserCvRepository userCvRepository) : IUserCvService
    {
        private readonly IUserCvRepository _userCvRepository = userCvRepository;

        public async Task<List<AppliedJob>> GetAppliedJobsByUserIdAsync(string userId)
        {
            var userCVs = await _userCvRepository.GetUserCVsByUserIdAsync(userId);
            if (userCVs.Count == 0) return [];

            var postIds = userCVs.Select(cv => cv.PostId).ToList();
            var posts = await _userCvRepository.GetPostsByUserCVsAsync(postIds);

            var appliedJobs = userCVs
                .Join(posts,
                    userCV => userCV.PostId,
                    post => post.PostId,
                    (userCV, post) => new AppliedJob
                    {
                        PostId = post.PostId,
                        PostTitle = post.Title,
                        CompanyIndustry = post.Employer?.Field,
                        CompanyLogoURL = post.Employer?.LogoURL,
                        CvStatus = userCV.CVStatus,
                        ApplicationDate = userCV.ApplicationDate,
                        CvURL = userCV.CVFileUrl,

                        // Thêm dữ liệu từ Post
                        Description = post.Description,
                        Benefits = post.Benefits,
                        Salary = post.Salary,
                        Position = post.Position,
                        Experience = post.Experience,
                        Requirements = post.Requirements,
                        JobType = post.JobType,
                        Location = post.City?.CityName ?? "Không xác định"
                    })
                .ToList();

            return appliedJobs;
        }

        public async Task<bool> CancelAppliedJobAsync(string userId, string postId) => await _userCvRepository.DeleteUserCvAsync(userId, postId);


        public async Task<UserCvResponse> SaveCv(UserCvAddRequest? cv)
        {
            if (cv == null) throw new ArgumentNullException(nameof(cv));
            UserCV userCv = cv.ToUserCv();
            await _userCvRepository.AddCv(userCv);
            return userCv.ToUserCvResponse();
        }

        public async Task<List<UserCV>> GetApplicationsByUser(string userId)
        {
            return await _userCvRepository.GetCvsByUserId(userId);
        }

        public async Task<UserCV> GetApplicationByUserAndPost(string userId, string postId)
        {
            return await _userCvRepository.GetCvByUserAndPost(userId, postId);
        }

        public async Task<UserCV> GetApplicationById(string cvId)
        {
            return await _userCvRepository.GetCvById(cvId);
        }

        public async Task<UserCV> UpdateApplicationStatus(string cvId, string status)
        {
            return await _userCvRepository.UpdateCvStatus(cvId, status);
        }

        public async Task<UserCV> UpdateEmployerNotes(string cvId, string notes)
        {
            return await _userCvRepository.UpdateCvNotes(cvId, notes);
        }
    }
}
