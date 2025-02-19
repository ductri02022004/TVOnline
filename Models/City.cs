using System.ComponentModel.DataAnnotations;
using TVOnline.Migrations;

namespace TVOnline.Models
{
    public class City
    {
        [Key]
        public int CityID { get; set; }

        [Required]
        public string CityName { get; set; }

        public ICollection<Employer> Employers { get; set; }
        public ICollection<Users> Users { get; set; }
    }
}