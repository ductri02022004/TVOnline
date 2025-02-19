using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using TVOnline.Migrations;

namespace TVOnline.Models
{
    public class UserCV
    {
        [Key] // Khóa chính
        public int CV_ID { get; set; }

        // Khóa ngoại tham chiếu đến bảng Users
        [ForeignKey("User")]
        public int UserID { get; set; }
        public virtual Users User { get; set; }

        [Required]
        [StringLength(255)]
        public string CVFileUrl { get; set; } // Đường dẫn đến file CV

        [Required]
        [StringLength(50)]
        public string CVStatus { get; set; } // Trạng thái CV (VD: "Pending", "Approved", "Rejected")
    }
}
