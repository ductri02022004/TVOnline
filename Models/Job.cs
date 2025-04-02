using System.ComponentModel.DataAnnotations;
namespace TVOnline.Models
{
    public class Job
    {
        [Key]
        public string JobId { get; set; }
        public string? JobName { get; set; }
    }
}
