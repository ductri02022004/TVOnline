using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace TVOnline.Models {
    public class Users : IdentityUser{
        [Key] // Khóa chính
        public int UserID { get; set; }

        [Required]
        [StringLength(100)]
        public string FullName { get; set; } // Họ và tên

        [Required]
        [EmailAddress]
        [StringLength(100)]
        public string Email { get; set; } // Email

        [Required]
        [Phone]
        [StringLength(15)]
        public string PhoneNumber { get; set; } // Số điện thoại

        [Required]
        public int Age { get; set; } // Tuổi

        // Khóa ngoại tham chiếu đến bảng Cities
        [ForeignKey("City")]
        public int CityID { get; set; }
        public virtual City City { get; set; }

        // Quan hệ 1-N: Một User có thể có nhiều UserCV, Feedback, và Payment
        public virtual ICollection<UserCV> UserCVs { get; set; }
        public virtual ICollection<Feedback> Feedbacks { get; set; }
        public virtual ICollection<Payment> Payments { get; set; }

        public User()
        {
            UserCVs = new HashSet<UserCV>();
            Feedbacks = new HashSet<Feedback>();
            Payments = new HashSet<Payment>();
        }
    }
}   