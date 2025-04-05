namespace TVOnline.Areas.Admin.Models
{
    public class AdminDashboardViewModel
    {
        public int TotalUsers { get; set; }
        public int TotalEmployers { get; set; }
        public int TotalJobSeekers { get; set; }
        public int TotalPosts { get; set; }
        public int TotalApplications { get; set; }
        
        // Thuộc tính mới cho dashboard
        public int? TotalPremiumUsers { get; set; }
        public int NewUsersThisMonth { get; set; }
        public int NewPostsThisMonth { get; set; }
    }
}
