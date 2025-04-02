using System.ComponentModel.DataAnnotations;

namespace TVOnline.ViewModels.Account
{
    public class ChangePasswordViewModel
    {
        [EmailAddress]
        public string Email { get; set; }

        [Required(ErrorMessage = "Yêu cầu nhập mật khẩu")]
        [StringLength(40, MinimumLength = 6, ErrorMessage = "{0} tối thiểu {2} kí tự và tối đa {1} kí tự")]
        [DataType(DataType.Password)]
        [Display(Name = "Mật khẩu mới")]
        [Compare("ConfirmNewPassword", ErrorMessage = "Mật khẩu không khớp")]
        public string NewPassword { get; set; }

        [Required(ErrorMessage = "Yêu cầu xác nhận mật khẩu")]
        [DataType(DataType.Password)]
        [Display(Name = "Xác nhận mật khẩu mới")]
        public string ConfirmNewPassword { get; set; }
    }
}
