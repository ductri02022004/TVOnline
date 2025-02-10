using System.ComponentModel.DataAnnotations;

namespace TVOnline.ViewModels {
    public class CheckEmailViewModel {
        [Required(ErrorMessage = "Yêu cầu nhập Email")]
        [EmailAddress]
        public string Email { get; set; }
    }
}
