using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using TVOnline.Areas.Premium.Models;

namespace TVOnline.Areas.Premium.Controllers
{
    [Area("Premium")]
    [Authorize(Policy = "PremiumUser")]
    public class TaxCalculatorController : Controller
    {
        public IActionResult Index()
        {
            var model = new TaxCalculationModel();
            return View(model);
        }

        [HttpPost]
        public IActionResult Calculate(TaxCalculationModel model)
        {
            if (!ModelState.IsValid)
            {
                return View("Index", model);
            }

            // Tính các khoản bảo hiểm
            model.SocialInsurance = Math.Round(model.GrossSalary * 0.08m);
            model.HealthInsurance = Math.Round(model.GrossSalary * 0.015m);
            model.UnemploymentInsurance = Math.Round(model.GrossSalary * 0.01m);

            // Tính thu nhập trước thuế
            model.PreTaxIncome = model.GrossSalary - model.SocialInsurance - model.HealthInsurance - model.UnemploymentInsurance;

            // Tính thu nhập chịu thuế
            model.Dependent = model.DependentDeduction * 4400000; // Giảm trừ gia cảnh cho người phụ thuộc
            model.TaxableIncome = model.PreTaxIncome - model.PersonalDeduction - model.DependentDeduction;
            if (model.TaxableIncome < 0)
            {
                model.TaxableIncome = 0;
            }

            // Tính thuế theo từng bậc
            decimal remainingIncome = model.TaxableIncome;
            decimal tax = 0;

            // Bậc 1: 5% cho thu nhập đến 5 triệu
            if (remainingIncome > 0)
            {
                decimal taxableAmount = Math.Min(remainingIncome, 5000000);
                model.TaxLevel1 = Math.Round(taxableAmount * 0.05m);
                tax += model.TaxLevel1;
                remainingIncome -= taxableAmount;
            }

            // Bậc 2: 10% cho thu nhập trên 5 triệu đến 10 triệu
            if (remainingIncome > 0)
            {
                decimal taxableAmount = Math.Min(remainingIncome, 5000000);
                model.TaxLevel2 = Math.Round(taxableAmount * 0.1m);
                tax += model.TaxLevel2;
                remainingIncome -= taxableAmount;
            }

            // Bậc 3: 15% cho thu nhập trên 10 triệu đến 18 triệu
            if (remainingIncome > 0)
            {
                decimal taxableAmount = Math.Min(remainingIncome, 8000000);
                model.TaxLevel3 = Math.Round(taxableAmount * 0.15m);
                tax += model.TaxLevel3;
                remainingIncome -= taxableAmount;
            }

            // Bậc 4: 20% cho thu nhập trên 18 triệu đến 32 triệu
            if (remainingIncome > 0)
            {
                decimal taxableAmount = Math.Min(remainingIncome, 14000000);
                model.TaxLevel4 = Math.Round(taxableAmount * 0.2m);
                tax += model.TaxLevel4;
                remainingIncome -= taxableAmount;
            }

            // Bậc 5: 25% cho thu nhập trên 32 triệu đến 52 triệu
            if (remainingIncome > 0)
            {
                decimal taxableAmount = Math.Min(remainingIncome, 20000000);
                model.TaxLevel5 = Math.Round(taxableAmount * 0.25m);
                tax += model.TaxLevel5;
                remainingIncome -= taxableAmount;
            }

            // Bậc 6: 30% cho thu nhập trên 52 triệu đến 80 triệu
            if (remainingIncome > 0)
            {
                decimal taxableAmount = Math.Min(remainingIncome, 28000000);
                model.TaxLevel6 = Math.Round(taxableAmount * 0.3m);
                tax += model.TaxLevel6;
                remainingIncome -= taxableAmount;
            }

            // Bậc 7: 35% cho thu nhập trên 80 triệu
            if (remainingIncome > 0)
            {
                model.TaxLevel7 = Math.Round(remainingIncome * 0.35m);
                tax += model.TaxLevel7;
            }

            model.TaxAmount = tax;

            return View("Result", model);
        }
    }
} 