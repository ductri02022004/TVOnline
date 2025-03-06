using System;
using System.Collections.Generic;

namespace TVOnline.ViewModels.Employer
{
    public class EmployerDashboardViewModel
    {
        public CompanyViewModel Company { get; set; }
        public CompanyInfoViewModel CompanyInfo { get; set; }
        public int TotalPosts { get; set; }
        public int TotalApplications { get; set; }
        public List<RecentApplicationViewModel> RecentApplications { get; set; }
        public ApplicationStatisticsViewModel ApplicationStatistics { get; set; }
    }

    public class CompanyInfoViewModel
    {
        public string EmployerId { get; set; }
        public string CompanyName { get; set; }
        public string Field { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string Description { get; set; }
        public string LogoURL { get; set; }
        public int CityId { get; set; }
        public string CityName { get; set; }
    }

    public class RecentApplicationViewModel
    {
        public string UserName { get; set; }
        public string PostTitle { get; set; }
        public DateTime AppliedDate { get; set; }
        public string Status { get; set; }
        public string CvId { get; set; }
    }
    
    public class ApplicationStatisticsViewModel
    {
        public int TotalApplications { get; set; }
        public int AppliedCount { get; set; }
        public int ReviewingCount { get; set; }
        public int ShortlistedCount { get; set; }
        public int RejectedCount { get; set; }
        public int InterviewedCount { get; set; }
    }
}
