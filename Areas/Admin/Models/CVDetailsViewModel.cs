using System;

namespace TVOnline.Areas.Admin.Models
{
    public class CVDetailsViewModel
    {
        public string CvID { get; set; }
        public string UserName { get; set; }
        public string UserEmail { get; set; }
        public string CVFileUrl { get; set; }
        public string CVStatus { get; set; }
        public DateTime ApplicationDate { get; set; }
        public DateTime AppliedDate { get; set; }
        public string PostTitle { get; set; }
        public string PostCompany { get; set; }
        public string EmployerNotes { get; set; }
    }
}
