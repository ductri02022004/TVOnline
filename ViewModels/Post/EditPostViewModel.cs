using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace TVOnline.ViewModels.Post
{
    public class EditPostViewModel
    {
        public string PostId { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tiêu đề")]
        [StringLength(200, ErrorMessage = "Tiêu đề không được vượt quá 200 ký tự")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập mô tả công việc")]
        public string Description { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập yêu cầu")]
        public string Requirements { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập quyền lợi")]
        public string Benefits { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập vị trí công việc")]
        public string Position { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập mức lương")]
        [Range(0, double.MaxValue, ErrorMessage = "Mức lương phải lớn hơn 0")]
        public decimal Salary { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn loại công việc")]
        public string JobType { get; set; } // Full-time, Part-time, etc.

        [Required(ErrorMessage = "Vui lòng chọn kinh nghiệm yêu cầu")]
        public string Experience { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn địa điểm làm việc")]
        public int CityId { get; set; }

        public bool IsActive { get; set; } = true;

        public List<SelectListItem>? Cities { get; set; }
        public List<SelectListItem>? JobTypes { get; set; }
        public List<SelectListItem>? ExperienceLevels { get; set; }
    }
}
