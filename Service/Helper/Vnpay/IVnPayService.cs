using TVOnline.Models.Vnpay;

namespace TVOnline.Service.Helper.Vnpay {
    public interface IVnPayService {
        string CreatePaymentUrl(PaymentInformationModel model, HttpContext context);
        PaymentResponseModel PaymentExecute(IQueryCollection collections);

    }
}
