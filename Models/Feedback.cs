using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using TVOnline.Migrations;

namespace TVOnline.Models
{
    public class Feedback
    {
        [Key]
        public int FeedbackID { get; set; }

        [Required]
        public DateTime Date { get; set; }

        [Required]
        public int UserID { get; set; }

        [Required]
        public int EmployerID { get; set; }

        [ForeignKey("UserID")]
        public virtual Users User { get; set; }

        [ForeignKey("EmployerID")]
        public virtual Employer Employer { get; set; }
    }
}
