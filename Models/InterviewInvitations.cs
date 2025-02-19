using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using TVOnline.Migrations;

namespace TVOnline.Models
{
    public class InterviewInvitations
    {

        [Key] // Khóa chính
        public int InvitationID { get; set; }

        public DateTime InvitationDate { get; set; }

        // Khóa ngoại tham chiếu đến bảng Users
        [ForeignKey("User")]
        public int UserID { get; set; }
        public virtual User User { get; set; }

        // Khóa ngoại tham chiếu đến bảng Employers
        [ForeignKey("Employer")]
        public int EmployerID { get; set; }
        public virtual Employer Employer { get; set; }
    }
}
