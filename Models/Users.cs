using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;
using TVOnline.Models.Vnpay;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace TVOnline.Models {
    public class Users : IdentityUser {
        public string? FullName { get; set; }
        public string? UserCity { get; set; }
        public string? UserJob { get; set; }
        public DateTime? Dob { get; set; }

        [InverseProperty("User")]
        public virtual PremiumUser? PremiumUser { get; set; }
        

        public virtual Employers? Employer { get; set; }
        public string? EmployerId { get; set; }
        public virtual ICollection<InterviewInvitation>? InterviewInvitations { get; set; }
        public virtual ICollection<Feedbacks>? Feedbacks { get; set; }
        public virtual ICollection<PaymentInformationModel>? Payments { get; set; }
        public virtual ICollection<UserCV>? UserCVs { get; set; }
    }
}