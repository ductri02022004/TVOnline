namespace TVOnline.Service.DTO
{
    public class PostResponse
    {
        public string PostId { get; set; }
        public string? EmployerId { get; set; }
        public string Title { get; set; }
        public decimal Salary { get; set; }
        public int CityId { get; set; }
        public string? CityName { get; set; }
        public string JobType { get; set; }
        public string? CompanyName { get; set; }
        public string? CompanyLogoURL { get; set; }
        public string Experience { get; set; }
        public string Position { get; set; }
        public string Description { get; set; }
        public string Requirements { get; set; }
        public string Benefits { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsSaved { get; set; } = false;
    }

    public static class PostResponseExtension
    {
        public static PostResponse ToPostResponse(this Models.Post post)
        {
            return new PostResponse
            {
                PostId = post.PostId,
                EmployerId = post.EmployerId,
                Title = post.Title,
                Salary = post.Salary,
                CityId = post.CityId,
                CityName = post.City?.CityName,
                CompanyName = post.Employer?.CompanyName,
                CompanyLogoURL = post.Employer?.LogoURL,
                JobType = post.JobType,
                Experience = post.Experience,
                Position = post.Position,
                Description = post.Description,
                Requirements = post.Requirements,
                Benefits = post.Benefits,
                IsActive = post.IsActive,
                CreatedAt = post.CreatedAt
            };
        }
    }
}
