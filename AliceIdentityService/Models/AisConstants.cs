namespace AliceIdentityService.Models
{
    public static class AisConstants
    {
        public const string AdminClaim = "ais_admin";

        public static HashSet<string> StandardScopes = new HashSet<string>()
        {
            "email", "address", "profile", "phone"
        };

        public static class Policy
        {
            public const string IsAdmin = "IsAdmin";
        }
    }
}
