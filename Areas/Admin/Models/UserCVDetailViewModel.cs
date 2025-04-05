using System;

namespace TVOnline.Areas.Admin.Models
{
    public class UserCVDetailViewModel
    {
        public string CvID { get; set; }
        public string CVFileUrl { get; set; }
        public string CVStatus { get; set; }
        public DateTime ApplicationDate { get; set; }
        public DateTime AppliedDate { get; set; }
        public string PostTitle { get; set; }
    }
}
