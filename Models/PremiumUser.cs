using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using TVOnline.Migrations;

namespace TVOnline.Models
{
    public class PremiumUser
    {
        [Key] // Khóa chính
        public int PremiumUserID { get; set; }

        // Khóa ngoại tham chiếu đến bảng Users
        [ForeignKey("User")]
        public int UserID { get; set; }
        public virtual User User { get; set; }
    }
}
