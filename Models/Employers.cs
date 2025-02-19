using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Http;
using static TVOnline.Models.Location;

namespace TVOnline.Models {
    public class Employers {
        [Key]
        public string EmployerId { get; set; }

        [ForeignKey("UserId")]
        [ValidateNever]
        public Users? User { get; set; }
        public string? UserId { get; set; }

        public string? Email { get; set; }
        public string? CompanyName { get; set; }
        public string? Description { get; set; }

        [NotMapped]
        public IFormFile? Logo { get; set; }
        public string? LogoURL { get; set; }

        [ForeignKey("CityId")]
        [ValidateNever]
        public Location.Cities? City { get; set; }
        public int? CityId { get; set; }

        public virtual ICollection<Post>? Posts { get; set; }
        public virtual ICollection<Feedbacks>? Feedbacks { get; set; }
        public virtual ICollection<InterviewInvitation>? InterviewInvitations { get; set; }
    }
}
