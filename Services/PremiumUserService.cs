using TVOnline.Data;
using TVOnline.Models;
using Microsoft.EntityFrameworkCore;

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

            return await _context.PremiumUsers.AnyAsync(p => p.UserId == userId);
        }
    }
} 