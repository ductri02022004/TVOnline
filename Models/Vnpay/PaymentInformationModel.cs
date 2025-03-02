using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;


namespace TVOnline.Models.Vnpay

{
    public class PaymentInformationModel
    {
        [Key]
        public int OrderId { get; set; }
        public string OrderType { get; set; }
        public double? Amount { get; set; }
        public string? OrderDescription { get; set; }
        public string? Name { get; set; }
    }

}
