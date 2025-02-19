using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TVOnline.Models
{
    [Table("Post")]
    public class Post
    {
        [Key]
        public int PostId { get; set; }

        public DateTime Date { get; set; }
        [Required]
        public string Description { get; set; }
        [Required]
        [ForeignKey("Employer")]
        public int EmployerId { get; set; }

        public virtual Employer Employer { get; set; }
    }
}
