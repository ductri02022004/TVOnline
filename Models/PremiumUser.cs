using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace TVOnline.Models {
    public class PremiumUser {
        [Key]
        public string PremiumUserId { get; set; }

        [ForeignKey("UserId")]
        [ValidateNever]
        public virtual Users? User { get; set; }
        public string? UserId { get; set; }

        public virtual ICollection<Template>? Templates { get; set; }
    }
}
