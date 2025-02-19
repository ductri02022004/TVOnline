using System.ComponentModel.DataAnnotations;

namespace TVOnline.Models
{
    public class Zone
    {
        [Key]
        public int ZoneID { get; set; }
        [Required]
        public string ZoneName { get; set; }

        public ICollection<City> Cities { get; set; } = [];
    }
}
