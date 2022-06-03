using System.Security.Cryptography;
using OpenIddict.Abstractions;

namespace AliceIdentityService.Helpers
{
    public class Utility
    {
        public static string GenerateClientSecret()
        {
            var bytes = new byte[32];
            using var rand = RandomNumberGenerator.Create();
            rand.GetBytes(bytes);
            return Convert.ToBase64String(bytes);
        }

        public static List<string> GetAllowedScopes(OpenIddictApplicationDescriptor descriptor)
        {
            var scopes = new List<string>();
            foreach (string permission in descriptor.Permissions)
                if (permission.StartsWith(OpenIddictConstants.Permissions.Prefixes.Scope))
                    scopes.Add(permission.Substring(OpenIddictConstants.Permissions.Prefixes.Scope.Length));
            return scopes;
        }
    }
}
