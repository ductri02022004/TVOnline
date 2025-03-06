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
    }
}
