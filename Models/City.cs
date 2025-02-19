using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TVOnline.Migrations;

namespace TVOnline.Models
{
    public class City
    {
        [Key]
        public int CityID { get; set; }

        [Required]
        public string? CityName { get; set; }
        [Required]
        public int ZoneId { get; set; }
        [ForeignKey("ZoneId")]
        public Zone Zone { get; set; }

        public ICollection<Employer> Employers { get; set; } = [];
        public ICollection<Users> Users { get; set; } = [];
    }
}