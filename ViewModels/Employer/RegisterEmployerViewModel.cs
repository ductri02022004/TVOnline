using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace TVOnline.ViewModels.Employer
{
    public class RegisterEmployerViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập tên công ty")]
        [Display(Name = "Tên công ty")]
        public string? CompanyName { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập email")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        [Display(Name = "Email")]
        public string? Email { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn thành phố")]
        [Display(Name = "Thành phố")]
        public int CityId { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập mô tả về công ty")]
        [Display(Name = "Mô tả")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập lĩnh vực hoạt động")]
        [Display(Name = "Lĩnh vực")]
        public string? Field { get; set; }

        // Danh sách các thành phố để hiển thị trong dropdown
        public List<SelectListItem>? Cities { get; set; }
    }
}
