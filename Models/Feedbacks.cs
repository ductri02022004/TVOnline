using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TVOnline.Models
{
    public class Feedbacks
    {
        [Key]
        public string FeedbackId { get; set; } = Guid.NewGuid().ToString();
        public string? Content { get; set; }
        public DateTime Date { get; set; }

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
