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

        public virtual ICollection<UserCV> UserCVs { get; set; }
        public virtual ICollection<Feedback> Feedbacks { get; set; }
        public virtual ICollection<Payment> Payments { get; set; }

        public User()
        {
            UserCVs = new HashSet<UserCV>();
            Feedbacks = new HashSet<Feedback>();
            Payments = new HashSet<Payment>();
        }
    }
}   