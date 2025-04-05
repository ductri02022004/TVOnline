using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TVOnline.Models
{
    public class Employers
    {
        [Key]
        public string EmployerId { get; set; }

        [ForeignKey("UserId")]
        [ValidateNever]
        public Users? User { get; set; }
        public string? UserId { get; set; }

        [Required]
        public string Email { get; set; }
        [Required]
        public string CompanyName { get; set; }
        [Required]
        public string Description { get; set; }
        [Required]
        public string Field { get; set; }
        [NotMapped]
        public IFormFile? Logo { get; set; }
        public string? LogoURL { get; set; }

        public string? Website { get; set; }

        [ForeignKey("CityId")]
        [ValidateNever]
        public Location.Cities? City { get; set; }
        [Required]
        public int CityId { get; set; }

        public DateTime CreatedAt { get; set; }

        public virtual ICollection<Post>? Posts { get; set; } = [];
        public virtual ICollection<Feedbacks>? Feedbacks { get; set; } = [];
        public virtual ICollection<InterviewInvitation>? InterviewInvitations { get; set; } = [];
    }
}
