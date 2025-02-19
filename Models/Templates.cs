using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace TVOnline.Models
{
    public class Templates
    {
        [Key]
        public int TemplateID { get; set; }

        [Required]
        public string TemplateName { get; set; }

        [Required]
        public string TemplateFile { get; set; }

        [Required]
        public int PremiumUserID { get; set; }

        [ForeignKey("PremiumUserID")]
        public PremiumUser PremiumUser { get; set; }
    }
}
