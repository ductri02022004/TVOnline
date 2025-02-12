using System.ComponentModel.DataAnnotations;

namespace TVOnline.ViewModels {
    public class LoginViewModel {
        [Required(ErrorMessage = " Yêu cầu nhập tài khoản ")]
        [Display(Name ="Tên đăng nhập")]
        public string UserName{ get; set; }
        [Required(ErrorMessage = "Yêu cầu nhập mật khẩu")]
        [DataType(DataType.Password)]
        [Display(Name = "Mật khẩu")]
        public string Password { get; set; }
        [Display(Name = "Ghi nhớ đăng nhập")]
        public bool RememberMe{ get; set; }
    }
}
