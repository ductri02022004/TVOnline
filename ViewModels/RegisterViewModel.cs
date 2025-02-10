using System.ComponentModel.DataAnnotations;

namespace TVOnline.ViewModels {
    public class RegisterViewModel {
        
        [Display(Name = "Thành phố")]
        public string? City { get; set; }

        [Display(Name = "Số điện thoại")]
        public string? PhoneNumber { get; set; }

        [Required(ErrorMessage = "Yêu cầu nhập tên")]
        [Display(Name= "Họ và tên")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Yêu cầu nhập Email")]
        [EmailAddress(ErrorMessage ="Địa chỉ email không hợp lệ")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Yêu cầu nhập mật khẩu")]
        [StringLength(40, MinimumLength = 6, ErrorMessage = "{0} tối thiểu {2} kí tự và tối đa {1} kí tự")]
        [DataType(DataType.Password)]
        [Display(Name = "Mật khẩu")]
        [Compare("ConfirmPassword", ErrorMessage = "Mật khẩu không khớp")]
        public string Password { get; set; }

        [Required(ErrorMessage = "Yêu cầu xác nhận mật khẩu")]
        [DataType(DataType.Password)]
        [Display(Name = "Xác nhận mật khẩu")]

        public string ConfirmPassword { get; set; }
    }
}
