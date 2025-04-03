using TVOnline.Data;
using TVOnline.Models;
using Microsoft.EntityFrameworkCore;
using System;

namespace TVOnline.Services
{
    public interface IPremiumUserService
    {
        Task<bool> IsUserPremium(string userId);
    }

    public class PremiumUserService : IPremiumUserService
    {
        private readonly AppDbContext _context;

        public PremiumUserService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<bool> IsUserPremium(string userId)
        {
            if (string.IsNullOrEmpty(userId))
                return false;

            // Kiểm tra trong bảng AccountStatuses thay vì PremiumUsers
            var accountStatus = await _context.AccountStatuses
                .FirstOrDefaultAsync(a => a.UserId == userId);
                
            if (accountStatus == null)
                return false;
                
            // Kiểm tra nếu tài khoản có trạng thái Premium và chưa hết hạn
            return accountStatus.IsPremium && 
                  (accountStatus.EndDate == null || accountStatus.EndDate >= DateTime.Now);
        }
    }
}