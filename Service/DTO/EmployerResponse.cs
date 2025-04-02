namespace TVOnline.Service.DTO
{
    public class EmployerResponse
    {
        public string EmployerId { get; set; }
        public string? UserId { get; set; }
        public string Email { get; set; }
        public string CompanyName { get; set; }
        public string Description { get; set; }
        public string Field { get; set; }
        public string? LogoURL { get; set; }
        public string? LogoUrl => LogoURL;
        public int CityId { get; set; }
        public string? CityName { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public static class EmployerResponseExtension
    {
        public static EmployerResponse ToEmployerResponse(this Models.Employers employer)
        {
            return new EmployerResponse
            {
                EmployerId = employer.EmployerId,
                UserId = employer.UserId,
                Email = employer.Email,
                CompanyName = employer.CompanyName,
                Description = employer.Description,
                Field = employer.Field,
                LogoURL = employer.LogoURL,
                CityId = employer.CityId,
                CityName = employer.City?.CityName,
                CreatedAt = employer.CreatedAt
            };
        }
    }
}
