using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using TVOnline.Migrations;

namespace TVOnline.Models
{
    public class Employer
    {
        [Key]
        public int EmployerID { get; set; }

        [Required]
        public int UserID { get; set; }

        [ForeignKey("UserID")]
        public Users User { get; set; }

        [Required]
        public int CityID { get; set; }

        [ForeignKey("CityID")]
        public City City { get; set; }

        public int? FeedBackID;
        [ForeignKey("FeedbackID")]
        public Feedback Feedback { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string CompanyName { get; set; }

        public string Description { get; set; }

        public ICollection<Feedback> Feedbacks { get; set; } = [];
        public ICollection<Post> Posts { get; set; } = [];
        public ICollection<InterviewInvitations> InterviewInvitations { get; set; }

    }
}
