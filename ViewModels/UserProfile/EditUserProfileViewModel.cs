using System.ComponentModel.DataAnnotations;

namespace TVOnline.ViewModels.UserProfile
{
    public class EditUserProfileViewModel
    {
        public string Id { get; set; }

        [Display(Name = "Họ và tên")]
        public string? Name { get; set; }

        [Display(Name = "Ngày sinh")]
        [DataType(DataType.Date)]
        public DateTime? Dob { get; set; }

        [Display(Name = "Thành phố")]
        public string? City { get; set; }

        [Display(Name = "Số điện thoại")]
        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        public string? PhoneNumber { get; set; }

        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Display(Name = "Chuyên ngành")]
        public string? Job { get; set; }
    }
}
