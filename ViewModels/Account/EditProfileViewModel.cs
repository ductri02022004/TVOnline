using System.ComponentModel.DataAnnotations;

namespace TVOnline.ViewModels.Account {
    public class EditProfileViewModel {
        public string Id { get; set; }
        [Required(ErrorMessage = "Không được để trống")]
        [Display(Name = "Họ và tên")]
        public string? Name { get; set; }

        [Display(Name = "Năm sinh")]
        [DataType(DataType.Date)]
        public DateTime? Age { get; set; }

        [Display(Name = "Thành phố")]
        public string? City { get; set; }

        [Display(Name = "Số điện thoại")]
        public string? PhoneNumber { get; set; }

        [Display(Name = "Email")]
        public string Email { get; set; }

        [Display(Name = "Ngành")]
        public string? Job { get; set; }
    }
}
