using System.ComponentModel.DataAnnotations;

namespace TVOnline.Models
{
    public class Job
    {
        [Key]
        public int JobID { get; set; }

        [Required]
        public string JobName { get; set; }
    }
}
