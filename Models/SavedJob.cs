using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace TVOnline.Models
{
    public class SavedJob
    {
        [Key]
        public string SavedJobId { get; set; } = null!; // String type for consistency

        [Required]
        public string PostId { get; set; } = null!;
        [ForeignKey("PostId")]
        public Post? Post { get; set; } // Navigation property to Post

        [Required]
        public string UserId { get; set; } = null!;

        [ForeignKey("UserId")]
        public Users? User { get; set; }

        public DateTime SavedAt { get; set; } = DateTime.Now; // Timestamp when saved
    }
}
