using Microsoft.AspNetCore.Authorization;
using TVOnline.Services;

namespace TVOnline.Areas.Premium
{
    public class PremiumAuthorizationHandler : AuthorizationHandler<PremiumRequirement>
    {
        private readonly IPremiumUserService _premiumUserService;

        public PremiumAuthorizationHandler(IPremiumUserService premiumUserService)
        {
            _premiumUserService = premiumUserService;
        }

        protected override async Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            PremiumRequirement requirement)
        {
            var userId = context.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            if (userId != null && await _premiumUserService.IsUserPremium(userId))
            {
                context.Succeed(requirement);
            }
        }
    }

    public class PremiumRequirement : IAuthorizationRequirement
    {
    }
}