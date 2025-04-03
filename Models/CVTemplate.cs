using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TVOnline.Models
{
    public class CVTemplate
    {
        [Key]
        public string TemplateId { get; set; } = Guid.NewGuid().ToString();
        
        [Required]
        [StringLength(100)]
        public string Name { get; set; }
        
        [Required]
        [StringLength(500)]
        public string Description { get; set; }
        
        [Required]
        public string HtmlContent { get; set; }
        
        [Required]
        public string CssContent { get; set; }
        
        [StringLength(255)]
        public string ThumbnailPath { get; set; }
        
        public bool IsActive { get; set; } = true;
        
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        
        public DateTime? UpdatedAt { get; set; }
    }
}
