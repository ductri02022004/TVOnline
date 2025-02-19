using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using TVOnline.Migrations;

namespace TVOnline.Models
{
    public class PremiumUser
    {
        [Key]
        public int PremiumUserID { get; set; }

        [Required]
        public int UserID { get; set; }

        [ForeignKey("UserID")]
        public Users User { get; set; }

        public ICollection<Templates> Templates { get; set; }
        public ICollection<UserCV> UserCVs { get; set; }
    }
}
