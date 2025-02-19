using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using TVOnline.Migrations;

namespace TVOnline.Models
{
    public class Payments
    {
        [Key]
        public int PaymentID { get; set; }

        [Required]
        public DateTime PaymentDate { get; set; }

        [Required]
        public string PaymentMethod { get; set; }

        [Required]
        public int UserID { get; set; }

        [Required]
        public long Amount { get; set; }

        [ForeignKey("UserID")]
        public Users User { get; set; }
    }
}
