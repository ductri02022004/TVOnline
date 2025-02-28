using TVOnline.Library;
using TVOnline.Models.Vnpay;

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
            var timeNow = DateTime.Now;
            var pay = new VnPayLibrary();

            // Set cứng các giá trị
            string urlCallBack = "https://yourdomain.com/payment/callback";
            string baseUrl = "https://sandbox.vnpayment.vn/paymentv2/vpcpay.html";
            string hashSecret = "FLLMBQ21VBPOM0IV5PZ70QD4DXERVYH9";
            string tmnCode = "WK6I2UJZ";

            pay.AddRequestData("vnp_Version", "2.1.0");
            pay.AddRequestData("vnp_Command", "pay");
            pay.AddRequestData("vnp_TmnCode", tmnCode);
            pay.AddRequestData("vnp_Amount", "5000000");  // 500,000 VND (set cứng)
            pay.AddRequestData("vnp_CreateDate", timeNow.ToString("yyyyMMddHHmmss"));
            pay.AddRequestData("vnp_CurrCode", "VND");
            pay.AddRequestData("vnp_IpAddr", "127.0.0.1"); // Set cứng IP
            pay.AddRequestData("vnp_Locale", "vn");
            pay.AddRequestData("vnp_OrderInfo", "Thanh toán đơn hàng 123456");
            pay.AddRequestData("vnp_OrderType", "billpayment");
            pay.AddRequestData("vnp_ReturnUrl", urlCallBack);
            pay.AddRequestData("vnp_TxnRef", "1234567890"); // Mã giao dịch set cứng

            // Tạo URL thanh toán
            var paymentUrl = pay.CreateRequestUrl(baseUrl, hashSecret);
            return paymentUrl;
        }

        public PaymentResponseModel PaymentExecute(IQueryCollection collections)
        {
            var pay = new VnPayLibrary();
            var response = pay.GetFullResponseData(collections, _configuration["Vnpay:HashSecret"]);

            return response;
        }


    }

}