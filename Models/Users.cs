using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace TVOnline.Models {
    public class Users : IdentityUser{
        [Key]
        public int UserID { get; set; }

        [Required]
        [StringLength(100)]
        public string FullName { get; set; } 

        [Required]
        [EmailAddress]
        [StringLength(100)]
        public string Email { get; set; }

        [Required]
        [Phone]
        [StringLength(15)]
        public string PhoneNumber { get; set; } 

        [Required]
        public int Age { get; set; } 

        [ForeignKey("City")]
        public int CityID { get; set; }
        public virtual City City { get; set; }

        public virtual UserCV UserCVs { get; set; }
        public virtual PremiumUser PremiumUser { get; set; }
        public virtual ICollection<Payments> Payments { get; set; }
        public virtual ICollection<Feedback> Feedbacks { get; set; } // Feedbacks mà User đưa ra
        public virtual ICollection<InterviewInvitations> InterviewInvitations { get; set; }
    }
}   