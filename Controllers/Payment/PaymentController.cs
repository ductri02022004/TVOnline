using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Net.payOS.Types;
using Net.payOS;
using System;
using System.Threading.Tasks;
using TVOnline.Data;
using TVOnline.Models;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using System.Linq;

[Route("Payment")]
[ApiController]
public class PaymentController : Controller
{
    private readonly PayOS _payOS;
    private readonly string _domain;
    private readonly AppDbContext _context;


    public PaymentController(AppDbContext context, IConfiguration configuration)
    {
        _context = context; // Inject AppDbContext

        var clientId = configuration["PayOS:ClientId"];
        var apiKey = configuration["PayOS:ApiKey"];
        var checksumKey = configuration["PayOS:ChecksumKey"];

        Console.WriteLine($"ClientId: {clientId}");
        Console.WriteLine($"ApiKey: {apiKey}");
        Console.WriteLine($"ChecksumKey: {checksumKey}");

        _payOS = new PayOS(clientId, apiKey, checksumKey);
        _domain = configuration["AppSettings:Domain"] ?? "http://localhost:3030";
    }

    [HttpPost("create-payment-link")]
    public async Task<IActionResult> CreatePaymentLink()
    {
        try
        {
            var orderCode = int.Parse(DateTimeOffset.Now.ToString("ffffff"));

            var paymentLinkRequest = new PaymentData(
                orderCode: orderCode,
                amount: 10000,
                description: "Thanh toán đơn hàng",
                items: [new("Nâng cấp tài khoản", 1, 10000)],
                returnUrl: "https://tvonline20250307004019.azurewebsites.net/Payment/success",
                cancelUrl: "https://tvonline20250307004019.azurewebsites.net/Payment/cancel"
            );

            var response = await _payOS.createPaymentLink(paymentLinkRequest);

            if (response == null || string.IsNullOrEmpty(response.checkoutUrl))
            {
                return BadRequest(new { message = "Không thể tạo link thanh toán." });
            }

            return Redirect(response.checkoutUrl); // 🔥 Tự động chuyển hướng đến trang thanh toán
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Lỗi server", details = ex.Message });
        }
    }

    [HttpGet("PaymentHistory")]
    public async Task<IActionResult> PaymentHistory()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized("Bạn cần đăng nhập để xem lịch sử giao dịch.");
        }

        // Use FromSqlRaw to get payment history
        var sqlQuery = "SELECT * FROM Payments WHERE UserId = @UserId ORDER BY PaymentDate DESC";
        var payments = await _context.Set<Payment>()
            .FromSqlRaw(sqlQuery, new SqlParameter("@UserId", userId))
            .ToListAsync();

        return View(payments);
    }

    [HttpGet("Success")]
    public async Task<IActionResult> Success(
        [FromQuery] string id,
        [FromQuery] string orderCode,
        [FromQuery] double amount,
        [FromQuery] string? paymentMethod = "Unknown"
    )
    {
        if (string.IsNullOrEmpty(id) || string.IsNullOrEmpty(orderCode))
        {
            return BadRequest("Thông tin giao dịch không hợp lệ.");
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized("Bạn cần đăng nhập để thực hiện giao dịch.");
        }

        var vietnamTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
        var vietnamTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, vietnamTimeZone);

        var payment = new Payment
        {
            PaymentId = id,
            PaymentDate = vietnamTime,
            PaymentMethod = paymentMethod ?? "PAYOS",
            Amount = amount,
            Status = "Success",
            UserId = userId
        };

        try
        {
            // Use direct SQL with parameters to insert payment record
            var sql = @"INSERT INTO Payments (PaymentId, PaymentDate, PaymentMethod, Amount, Status, UserId) 
                       VALUES (@PaymentId, @PaymentDate, @PaymentMethod, @Amount, @Status, @UserId)";
            
            await _context.Database.ExecuteSqlRawAsync(sql,
                new SqlParameter("@PaymentId", payment.PaymentId),
                new SqlParameter("@PaymentDate", payment.PaymentDate),
                new SqlParameter("@PaymentMethod", payment.PaymentMethod ?? (object)DBNull.Value),
                new SqlParameter("@Amount", payment.Amount),
                new SqlParameter("@Status", payment.Status),
                new SqlParameter("@UserId", payment.UserId));
            
            // Check if user is already premium
            var existingPremiumUser = await _context.PremiumUsers
                .FirstOrDefaultAsync(p => p.UserId == userId);

            if (existingPremiumUser == null)
            {
                // Create new premium user
                var premiumUser = new PremiumUser
                {
                    PremiumUserId = Guid.NewGuid().ToString(),
                    UserId = userId
                };
                _context.PremiumUsers.Add(premiumUser);
                await _context.SaveChangesAsync();
            }

            return View("Success", payment);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Lỗi khi lưu thông tin thanh toán", details = ex.Message });
        }
    }

    [HttpGet("Cancel")]
    public IActionResult Cancel()
    {
        return View();
    }

    [HttpGet("Index")]
    public IActionResult Index()
    {
        return View();
    }
}
