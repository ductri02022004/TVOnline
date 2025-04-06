using System.ComponentModel.DataAnnotations;

namespace TVOnline.Areas.Premium.Models
{
    public class TaxCalculationModel
    {
        [Required(ErrorMessage = "Vui lòng nhập lương GROSS")]
        [Display(Name = "Lương GROSS (VNĐ)")]
        [Range(0, double.MaxValue, ErrorMessage = "Lương GROSS phải lớn hơn 0")]
        public decimal GrossSalary { get; set; }

        [Display(Name = "Bảo hiểm xã hội (8%)")]
        public decimal SocialInsurance { get; set; }

        [Display(Name = "Bảo hiểm y tế (1.5%)")]
        public decimal HealthInsurance { get; set; }

        [Display(Name = "Bảo hiểm thất nghiệp (1%)")]
        public decimal UnemploymentInsurance { get; set; }

        [Display(Name = "Thu nhập trước thuế")]
        public decimal PreTaxIncome { get; set; }

        [Display(Name = "Giảm trừ gia cảnh bản thân")]
        public decimal PersonalDeduction { get; set; } = 11000000; // Mặc định 11 triệu/tháng

        [Display(Name = "Giảm trừ gia cảnh người phụ thuộc")]
        public decimal DependentDeduction { get; set; }

        [Display(Name = "Thu nhập chịu thuế (VNĐ)")]
        public decimal TaxableIncome { get; set; }

        [Display(Name = "Số thuế phải nộp (VNĐ)")]
        public decimal TaxAmount { get; set; }

        // Chi tiết thuế theo từng bậc
        public decimal TaxLevel1 { get; set; } // 5%
        public decimal TaxLevel2 { get; set; } // 10%
        public decimal TaxLevel3 { get; set; } // 15%
        public decimal TaxLevel4 { get; set; } // 20%
        public decimal TaxLevel5 { get; set; } // 25%
        public decimal TaxLevel6 { get; set; } // 30%
        public decimal TaxLevel7 { get; set; } // 35%
    }
} 