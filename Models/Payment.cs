using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TVOnline.Models
{
    [Table("Payments")]
    public class Payment
    {
        [Key]
        public string PaymentId { get; set; }
        public DateTime? PaymentDate { get; set; }
        public string? PaymentMethod { get; set; }

        [ForeignKey("UserId")]
        [ValidateNever]
        public virtual Users? User { get; set; }
        public string? UserId { get; set; }

        public double? Amount { get; set; }

        // This maps to the existing Status column in the database
        [Column("Status")]
        public string? Status { get; set; }
    }
}
