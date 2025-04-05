using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TVOnline.Models
{
    public class InterviewInvitation
    {
        [Key]
        public string InvitationId { get; set; }
        public DateTime InvitationDate { get; set; }

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
