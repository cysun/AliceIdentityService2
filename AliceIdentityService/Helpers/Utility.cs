using System.Security.Cryptography;

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
    }
}
