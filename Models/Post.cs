using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using static TVOnline.Models.Location;

namespace TVOnline.Models {
    public class Post {
        [Key]
        public int PostId { get; set; }

        [ForeignKey("EmployerId")]
        [ValidateNever]
        public virtual Employers? Employer { get; set; }
        public string? EmployerId { get; set; }

        [Required]
        public string Title { get; set; }

        [Required]
        public string Description { get; set; }

        [Required]
        public string Benefits { get; set; }

        [Required]
        public decimal Salary { get; set; }

        [Required]
        public string Position { get; set; }

        [Required]
        public string Experience { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn địa điểm làm việc")]
        public int CityId { get; set; }

        [ForeignKey("CityId")]
        [ValidateNever]
        public virtual Cities? City { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public bool IsActive { get; set; } = true;
        public string Requirements { get; set; }
        public string JobType { get; set; } // Full-time, Part-time, etc.
    }
}
