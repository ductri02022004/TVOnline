using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using TVOnline.Migrations;

namespace TVOnline.Models
{
    public class Feedback
    {
        [Key] // Khóa chính
        public int FeedbackID { get; set; }

        [Required]
        public string Content { get; set; }

        [Required]
        public DateTime Date { get; set; }

        // Khóa ngoại tham chiếu đến bảng Employers
        [ForeignKey("Employer")]
        public int EmployerID { get; set; }
        public virtual Employer Employer { get; set; }

        // Khóa ngoại tham chiếu đến bảng Users
        [ForeignKey("User")]
        public int UserID { get; set; }
        public virtual User User { get; set; }
    }
}
