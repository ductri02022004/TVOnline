using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TVOnline.Models
{
    public class PremiumUserCV
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();
        
        [Required]
        public string UserId { get; set; }
        
        [ForeignKey("UserId")]
        public Users User { get; set; }
        
        [Required]
        public string TemplateId { get; set; }
        
        [ForeignKey("TemplateId")]
        public CVTemplate Template { get; set; }
        
        [Required]
        [StringLength(100)]
        public string Title { get; set; }
        
        [Required]
        public string Content { get; set; }
        
        [Required]
        public string HtmlContent { get; set; }
        
        [Required]
        public string CssContent { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        
        public DateTime? UpdatedAt { get; set; }
        
        public bool IsPublic { get; set; } = false;
    }
}
