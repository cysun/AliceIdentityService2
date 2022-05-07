using System.Security.Claims;
using AliceIdentityService.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using OpenIddict.Abstractions;

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
            var identity = await base.GenerateClaimsAsync(user);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.GivenName, user.FirstName),
                new Claim(ClaimTypes.Surname, user.LastName),
                new Claim(OpenIddictConstants.Claims.Nickname, user.ScreenName),
            };
            identity.AddClaims(claims);

            return identity;
        }
    }
}
