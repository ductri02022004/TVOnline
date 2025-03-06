using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Http;

namespace TVOnline.Models {
    public class UserCV {
        [Key]
        public string CvID { get; set; }

        [ForeignKey("UserId")]
        [ValidateNever]
        public Users? Users { get; set; }
        public string? UserId { get; set; }

        [NotMapped]
        public IFormFile? CvFile { get; set; }
        
        public string? CVFileUrl { get; set; }
        public string? CVStatus { get; set; }

        [ForeignKey("PostId")]
        [ValidateNever]
        public Post? Post { get; set; }
        public string? PostId { get; set; }
        
        public DateTime AppliedDate { get; set; } = DateTime.Now;
        
        public string? EmployerNotes { get; set; }
    }
}
