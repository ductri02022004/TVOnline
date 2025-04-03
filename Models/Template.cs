using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TVOnline.Models
{
    public class Template
    {
        [Key]
        public string TemplateId { get; set; }
        public string? TemplateName { get; set; }

        [NotMapped]
        public IFormFile? TemplateFile { get; set; }
        public string? TemplateFileURL { get; set; }

        [ForeignKey("PremiumUserId")]
        [ValidateNever]
        public virtual PremiumUser? PremiumUser { get; set; }
        public string? PremiumUserId { get; set; }
    }
}
