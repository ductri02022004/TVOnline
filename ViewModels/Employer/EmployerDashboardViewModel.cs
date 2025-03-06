namespace TVOnline.ViewModels.Employer
{
    public class EmployerDashboardViewModel
    {
        public string CompanyName { get; set; }
        public string Email { get; set; }
        public string City { get; set; }
        public string Zone { get; set; }
        public string Field { get; set; }
        public string Description { get; set; }
        public DateTime CreatedAt { get; set; }
        public int TotalPosts { get; set; }
        public int TotalInterviews { get; set; }
        public int TotalFeedbacks { get; set; }
    }
}
