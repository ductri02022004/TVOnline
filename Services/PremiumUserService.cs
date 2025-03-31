using TVOnline.Data;
using TVOnline.Models;
using Microsoft.EntityFrameworkCore;

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

            return await _context.PremiumUsers.AnyAsync(p => p.UserId == userId);
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