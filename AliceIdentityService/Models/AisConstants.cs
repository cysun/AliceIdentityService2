namespace AliceIdentityService.Models
{
    public static class AisConstants
    {
        public const string AdminClaim = "ais_admin";

        // Pre-create standard scopes and the associated claims. OpenIddict grants "openid" and "offline_access"
        // by default. OpenIddict also supports a "roles" scope which we will ignore.
        public static readonly Dictionary<string, string[]> StandardScopes = new Dictionary<string, string[]>()
        {
            {"email", new string[]{"email", "email_verified" } },
            {"address", new string[]{ "address" } },
            {"profile", new string[]{ "name", "family_name", "given_name", "middle_name", "nickname",
                "preferred_username", "profile", "picture", "website", "gender", "birthdate",
                "zoneinfo", "locale", "updated_at" } },
            {"phone", new string[]{"phone_number", "phone_number_verified"} }
        };

        public static class Policy
        {
            public const string IsAdmin = "IsAdmin";
        }
    }
}
