using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace TVOnline.Models {
    public class Post {
        [Key]
        public int PostId { get; set; }

        [ForeignKey("EmployerId")]
        [ValidateNever]
        public virtual Employers? Employer { get; set; }
        public string? EmployerId { get; set; }

        public DateTime? Date { get; set; }
        public string? Description { get; set; }
    }
}
