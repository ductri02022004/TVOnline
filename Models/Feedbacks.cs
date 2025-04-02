using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace TVOnline.Models {
    public class Feedbacks {
        [Key]
        public string FeedbackId { get; set; } = Guid.NewGuid().ToString();
        public string? Content { get; set; }
        public DateTime Date { get; set; }

        [Range(1, 5, ErrorMessage = "Đánh giá sao phải từ 1 đến 5")]
        public int Rating { get; set; }

        [ForeignKey("UserId")]
        [ValidateNever]
        public Users? User { get; set; }
        public string? UserId { get; set; }

        [ForeignKey("EmployerId")]
        [ValidateNever]
        public Employers? Employer { get; set; }
        public string? EmployerId { get; set; }
    }
}
