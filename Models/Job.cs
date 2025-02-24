using System.ComponentModel.DataAnnotations;
namespace TVOnline.Models {
    public class Job {
        [Key]
        public int JobId { get; set; }
        public string? JobName { get; set; }
    }
}
