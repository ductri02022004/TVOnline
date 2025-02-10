using System.ComponentModel.DataAnnotations;

namespace TVOnline.ViewModels {
    public class LoginViewModel {
        [Required(ErrorMessage = " Yêu cầu nhập địa chỉ Email ")]
        [EmailAddress]
        public string Email{ get; set; }
        [Required(ErrorMessage = "Yêu cầu nhập mật khẩu")]
        [DataType(DataType.Password)]
        public string Password { get; set; }
        [Display(Name = "Ghi nhớ đăng nhập")]
        public bool RememberMe{ get; set; }
    }
}
