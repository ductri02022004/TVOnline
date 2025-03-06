namespace TVOnline.Service.DTO
{
    public class SavedJobResponse : PostResponse
    {
        public string SavedJobId { get; set; }
        public string UserId { get; set; }
        public DateTime SavedAt { get; set; } = DateTime.Now;
    }

    public static class ToSavedJobResponseExtensions
    {
        public static SavedJobResponse ToSavedJobResponse(this SavedJob savedJob)
        {
            return new SavedJobResponse
            {
                SavedJobId = savedJob.SavedJobId,
                SavedAt = savedJob.SavedAt,
                PostId = savedJob.PostId,
                UserId = savedJob.UserId,
                Title = savedJob.Post.Title,
                CompanyName = savedJob.Post.Employer.CompanyName,
                CityName = savedJob.Post.City.CityName,
                CreatedAt = savedJob.Post.CreatedAt,
                Experience = savedJob.Post.Experience,
                JobType = savedJob.Post.JobType,
                Position = savedJob.Post.Position,
                Requirements = savedJob.Post.Requirements,
                Salary = savedJob.Post.Salary,
                IsActive = savedJob.Post.IsActive,
                CompanyLogoURL = savedJob.Post.Employer.LogoURL

            };
        }
    }
}
