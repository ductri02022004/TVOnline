using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace TVOnline.Models
{
    public class Employer
    {
        [Key] // Khóa chính
        public int EmployerID { get; set; }

        [Required]
        [StringLength(100)]
        public string CompanyName { get; set; }

        [StringLength(255)]
        public string Description { get; set; }

        [Required]
        [StringLength(100)]
        public string Email { get; set; }

        // Khóa ngoại tham chiếu đến bảng Cities
        [ForeignKey("City")]
        public int CityID { get; set; }
        public virtual City City { get; set; }

        // Navigation properties
        public virtual ICollection<InterviewInvitations> InterviewInvitations { get; set; }
        public virtual ICollection<Feedback> Feedbacks { get; set; }
        public virtual ICollection<Post> Posts { get; set; }

        public Employer()
        {
            InterviewInvitations = new HashSet<InterviewInvitations>();
            Feedbacks = new HashSet<Feedback>();
            Posts = new HashSet<Post>();
        }
    }
}
