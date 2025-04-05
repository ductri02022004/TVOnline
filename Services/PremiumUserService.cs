using TVOnline.Data;
using TVOnline.Models;
using Microsoft.EntityFrameworkCore;
using System;

namespace TVOnline.Services
{
    public interface IPremiumUserService
    {
        Task<bool> IsUserPremium(string userId);
        Task<PremiumUser> GetPremiumUser(string userId);
        Task<PremiumUser> CreatePremiumUser(string userId);
        Task<bool> DeletePremiumUser(string userId);
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

        public async Task<PremiumUser> GetPremiumUser(string userId)
        {
            return await _context.PremiumUsers
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.UserId == userId);
        }

        public async Task<PremiumUser> CreatePremiumUser(string userId)
        {
            var premiumUser = new PremiumUser
            {
                PremiumUserId = Guid.NewGuid().ToString(),
                UserId = userId
            };

            _context.PremiumUsers.Add(premiumUser);
            await _context.SaveChangesAsync();

            return premiumUser;
        }

        public async Task<bool> DeletePremiumUser(string userId)
        {
            var premiumUser = await GetPremiumUser(userId);
            if (premiumUser == null)
                return false;

            _context.PremiumUsers.Remove(premiumUser);
            await _context.SaveChangesAsync();

            return true;
        }
    }
}