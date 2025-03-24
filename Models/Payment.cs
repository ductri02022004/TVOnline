using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace TVOnline.Models
{
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
        public string? Status { get; set; }
    }
}
