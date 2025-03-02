using Microsoft.VisualBasic.CompilerServices;
using MimeKit.Utils;
using TVOnline.Library;
using TVOnline.Models.Vnpay;
using static System.Runtime.CompilerServices.RuntimeHelpers;

namespace TVOnline.Service.Vnpay
{


    public class VnPayService : IVnPayService
    {
        private readonly IConfiguration _configuration;

        public VnPayService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string CreatePaymentUrl(PaymentInformationModel model, HttpContext context)
        {
            var tick = DateTime.Now.Ticks.ToString();
            var timeNow = DateTime.Now;
            var pay = new VnPayLibrary();

            // Set cứng các giá trị
            string urlCallBack = "https://yourdomain.com/payment/callback";
            string hashSecret = "FLLMBQ21VBPOM0IV5PZ70QD4DXERVYH9";
            string tmnCode = "WK6I2UJZ";

            pay.AddRequestData("vnp_Version", _configuration["Vnpay:Version"]);
            pay.AddRequestData("vnp_Command", _configuration["Vnpay:Command"]);
            pay.AddRequestData("vnp_TmnCode", _configuration["Vnpay:TmnCode"]);
            pay.AddRequestData("vnp_Amount", "500000");  // 500,000 VND (set cứng)
            pay.AddRequestData("vnp_CreateDate", timeNow.ToString("yyyyMMddHHmmss"));
            pay.AddRequestData("vnp_CurrCode", _configuration["Vnpay:CurrCode"]);
            pay.AddRequestData("vnp_IpAddr", pay.GetIpAddress(context));
            pay.AddRequestData("vnp_Locale", _configuration["Vnpay:Locale"]);
            pay.AddRequestData("vnp_OrderInfo", "Thanh toán đơn hàng " + model.OrderId);
            pay.AddRequestData("vnp_OrderType", "billpayment");
            pay.AddRequestData("vnp_ReturnUrl", _configuration["Vnpay:PaymentBackReturnUrl"]);
            pay.AddRequestData("vnp_TxnRef", $"{tick}");

            var paymentUrl = pay.CreateRequestUrl(_configuration["Vnpay:BaseUrl"], _configuration["Vnpay:HashSecret"]);
            return paymentUrl;
        }

        public PaymentResponseModel PaymentExecute(IQueryCollection collections)
        {
            var pay = new VnPayLibrary();
            foreach (var (key, value) in collections)
            {
                pay.AddResponseData(key, value.ToString());
            }

            var vnp_orderId = Convert.ToInt64(pay.GetResponseData("vnp_TxnRef"));
            var vnp_TransactionId = Convert.ToInt64(pay.GetResponseData("vnp_TransactionNo"));
            var vnp_SecureHash = collections.FirstOrDefault(k => k.Key == "vnp_SecureHash").Value;
            var vnp_ResponseCode = pay.GetResponseData("vnp_ResponseCode");
            var vnp_OrderInfo = pay.GetResponseData("vnp_OrderInfo");
            bool checkSignature = pay.ValidateSignature(vnp_SecureHash, _configuration["Vnpay:HashSecret"]);
            if (!checkSignature)
            {
                return new PaymentResponseModel()
                {
                    Success = false
                };
            }

            return new PaymentResponseModel()
            {
                Success = true, 
                PaymentMethod = "VnPay",
                OrderDescription = vnp_OrderInfo,
                OrderId = vnp_orderId.ToString(),
                TransactionId = vnp_TransactionId.ToString(),
                Token = vnp_SecureHash,
                VnPayResponseCode = vnp_ResponseCode
            };
        }


    }

}