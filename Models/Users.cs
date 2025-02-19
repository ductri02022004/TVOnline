using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace TVOnline.Models {
    public class Users : IdentityUser {
        public string? FullName { get; set; }
        public string? UserCity { get; set; }
        public string? UserJob { get; set; }
        public DateTime? Age { get; set; }

        [InverseProperty("User")]
        public virtual PremiumUser? PremiumUser { get; set; }
        
        public virtual ICollection<InterviewInvitation>? InterviewInvitations { get; set; }
        public virtual ICollection<Feedbacks>? Feedbacks { get; set; }
        public virtual ICollection<Payment>? Payments { get; set; }
        public virtual ICollection<UserCV>? UserCVs { get; set; }
    }
}