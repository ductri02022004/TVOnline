using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using TVOnline.Migrations;

namespace TVOnline.Models
{
    public class InterviewInvitations
    {

        [Key]
        public int InvitationID { get; set; }

        [Required]
        public DateTime InvitationDate { get; set; }

        [Required]
        public int UserID { get; set; } // User nhận lời mời

        [Required]
        public int EmployerID { get; set; } // Employer gửi lời mời

        [ForeignKey("UserID")]
        public Users User { get; set; } // User nhận lời mời

        [ForeignKey("EmployerID")]
        public Employer Employer { get; set; } // Employer gửi lời mời
    }
}
