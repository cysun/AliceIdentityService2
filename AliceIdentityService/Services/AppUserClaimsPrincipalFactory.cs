using System.Security.Claims;
using AliceIdentityService.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace AliceIdentityService.Services
{
    public class AppUserClaimsPrincipalFactory : UserClaimsPrincipalFactory<User>
    {
        public AppUserClaimsPrincipalFactory(UserManager<User> userManager,
            IOptions<IdentityOptions> optionsAccessor) : base(userManager, optionsAccessor)
        {
        }

        protected override async Task<ClaimsIdentity> GenerateClaimsAsync(User user)
        {
            // Claims in AspNetUserClaims are added here.
            var identity = await base.GenerateClaimsAsync(user);

            // Add the claims based on User properties. 
            var claims = new List<Claim>
            {
                new Claim(Claims.GivenName, user.FirstName),
                new Claim(Claims.FamilyName, user.LastName),
                new Claim(Claims.Nickname, user.ScreenName),
            };

            identity.AddClaims(claims);

            return identity;
        }
    }
}
