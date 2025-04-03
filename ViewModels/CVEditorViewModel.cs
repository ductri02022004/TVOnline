using System.ComponentModel.DataAnnotations;

namespace TVOnline.ViewModels
{
    public class CVEditorViewModel
    {
        public string TemplateId { get; set; }
        
        public string TemplateName { get; set; }
        
        public string HtmlContent { get; set; }
        
        public string CssContent { get; set; }
        
        [Required(ErrorMessage = "Vui lòng nhập họ tên")]
        [Display(Name = "Họ và tên")]
        public string FullName { get; set; }
        
        [Required(ErrorMessage = "Vui lòng nhập email")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        [Display(Name = "Email")]
        public string Email { get; set; }
        
        [Display(Name = "Số điện thoại")]
        public string Phone { get; set; }
        
        [Display(Name = "Địa chỉ")]
        public string Address { get; set; }
        
        [Display(Name = "Vị trí ứng tuyển")]
        public string JobTitle { get; set; }
        
        [Display(Name = "Tóm tắt bản thân")]
        public string Summary { get; set; }
        
        [Display(Name = "Học vấn")]
        public string Education { get; set; }
        
        [Display(Name = "Kinh nghiệm làm việc")]
        public string Experience { get; set; }
        
        [Display(Name = "Kỹ năng")]
        public string Skills { get; set; }
        
        [Display(Name = "Ngôn ngữ")]
        public string Languages { get; set; }
        
        [Display(Name = "Chứng chỉ")]
        public string Certificates { get; set; }
        
        [Display(Name = "Dự án")]
        public string Projects { get; set; }
        
        [Display(Name = "Sở thích")]
        public string Interests { get; set; }
    }
}
