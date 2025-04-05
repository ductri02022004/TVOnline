using System.ComponentModel.DataAnnotations;

namespace TVOnline.ViewModels.Employer
{
    public class EditCompanyProfileViewModel
    {
        public string? EmployerId { get; set; }

        [Required(ErrorMessage = "Tên công ty không được để trống")]
        [Display(Name = "Tên công ty")]
        public string? CompanyName { get; set; }

        [Required(ErrorMessage = "Lĩnh vực hoạt động không được để trống")]
        [Display(Name = "Lĩnh vực hoạt động")]
        public string? Industry { get; set; }

        [Required(ErrorMessage = "Email không được để trống")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        [Display(Name = "Email")]
        public string? Email { get; set; }

        [Required(ErrorMessage = "Số điện thoại không được để trống")]
        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        [Display(Name = "Số điện thoại")]
        public string? Phone { get; set; }

        [Display(Name = "Website")]
        public string? Website { get; set; }

        [Display(Name = "Mô tả công ty")]
        public string? Description { get; set; }

        [Display(Name = "Logo công ty")]
        public IFormFile? LogoFile { get; set; }

        public string? CurrentLogoUrl { get; set; }

        [Display(Name = "Thành phố")]
        public int CityId { get; set; }
    }
}
