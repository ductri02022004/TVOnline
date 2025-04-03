namespace TVOnline.Service.DTO
{
    public class AppliedJob
    {
        public string PostId { get; set; }
        public string PostTitle { get; set; }
        public string? CompanyIndustry { get; set; }
        public string? CompanyLogoURL { get; set; }
        public string? CvStatus { get; set; }
        public DateTime ApplicationDate { get; set; }
        public string? CvURL { get; set; }
        public string Description { get; set; }
        public string Benefits { get; set; }
        public decimal Salary { get; set; }
        public string Position { get; set; }
        public string Experience { get; set; }
        public string Requirements { get; set; }
        public string JobType { get; set; }
        public string Location { get; set; }
        public string EmployerId { get; set; }
    }
}