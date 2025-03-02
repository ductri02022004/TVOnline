using Microsoft.AspNetCore.Mvc;
using TVOnline.Models.Vnpay;
using TVOnline.Service.Vnpay;

[Route("payment")]
[ApiController]
public class PaymentController : Controller
{
    private readonly IVnPayService _vnPayService;
    private readonly ILogger<PaymentController> _logger;

    public PaymentController(IVnPayService vnPayService, ILogger<PaymentController> logger)
    {
        _vnPayService = vnPayService;
        _logger = logger;
    }

    [HttpPost("create")]
    public IActionResult Create()
    {
        var model = new PaymentInformationModel
        {
            Amount = 0, // Số tiền cần thanh toán     
            OrderDescription = "thanhtoan",
            Name = "zamola",
            OrderType = "other" // Mã đơn hàng
        };

        var paymentUrl = _vnPayService.CreatePaymentUrl(model, HttpContext);
        return Redirect(paymentUrl);
    }

    [HttpGet("callback")]
    public IActionResult PaymentCallback()
    {
        try
        {
            var response = _vnPayService.PaymentExecute(Request.Query);
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Lỗi xử lý callback từ VNPay: {ex.Message}");
            return BadRequest(new { message = "Lỗi xử lý thanh toán." });
        }
    }

    [HttpGet]
    [Route("Index")]
    public IActionResult Index()
    {
        // Xử lý logic cần thiết tại đây
        return View("~/Views/Payment/Index.cshtml");
    }
}