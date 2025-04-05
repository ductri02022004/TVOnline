using System;

namespace TVOnline.Models
{
    public class AccountStatusViewModel
    {
        public string UserId { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public bool EmailConfirmed { get; set; }
        public bool IsLockedOut { get; set; }
        public DateTimeOffset? LockoutEnd { get; set; }
        public string AccountStatus { get; set; } = string.Empty;
        public string StatusMessage { get; set; } = string.Empty;
        public bool IsPremium { get; set; }
        public string PremiumUserId { get; set; } = string.Empty;
    }
}