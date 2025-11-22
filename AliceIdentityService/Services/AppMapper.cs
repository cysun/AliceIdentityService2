using AliceIdentityService.Models;
using OpenIddict.Abstractions;
using OpenIddict.EntityFrameworkCore.Models;
using Riok.Mapperly.Abstractions;

namespace AliceIdentityService.Services;

[Mapper(EnabledConversions = MappingConversionType.All & ~MappingConversionType.ToStringMethod)]
public partial class AppMapper
{
    private string Trim(string src) => src?.Trim();

    // Client

    [MapperIgnoreSource(nameof(ApplicationInputModel.ClientSecret))]
    [MapperIgnoreSource(nameof(ApplicationInputModel.RedirectUris))]
    [MapperIgnoreSource(nameof(ApplicationInputModel.PostLogoutRedirectUris))]
    private partial void MapPartial(ApplicationInputModel src, OpenIddictApplicationDescriptor dest);

    [UserMapping(Default = true)]
    public void Map(ApplicationInputModel src, OpenIddictApplicationDescriptor dest)
    {
        MapPartial(src, dest);
        Map(src.RedirectUris, dest.RedirectUris);
        Map(src.PostLogoutRedirectUris, dest.PostLogoutRedirectUris);

        if (src.IsNewClientSecret)
        {
            dest.ClientSecret = src.ClientSecret;
            dest.ClientType = string.IsNullOrEmpty(src.ClientSecret)
                ? OpenIddictConstants.ClientTypes.Public
                : OpenIddictConstants.ClientTypes.Confidential;
        }
    }

    [MapperIgnoreTarget(nameof(ApplicationInputModel.ClientSecret))]
    [MapperIgnoreTarget(nameof(ApplicationInputModel.RedirectUris))]
    [MapperIgnoreTarget(nameof(ApplicationInputModel.PostLogoutRedirectUris))]
    private partial ApplicationInputModel MapPartial(OpenIddictApplicationDescriptor src);

    [UserMapping(Default = true)]
    public ApplicationInputModel Map(OpenIddictApplicationDescriptor src)
    {
        var dest = MapPartial(src);
        dest.RedirectUris = Map(src.RedirectUris);
        dest.PostLogoutRedirectUris = Map(src.PostLogoutRedirectUris);
        dest.ClientSecret = string.IsNullOrEmpty(src.ClientSecret) ? src.ClientSecret : "********";
        dest.IsPkce = src.Requirements.Contains(OpenIddictConstants.Requirements.Features.ProofKeyForCodeExchange);
        return dest;
    }

    private void Map(string src, HashSet<Uri> dest)
    {
        dest.Clear(); // dest is never null
        if (!string.IsNullOrEmpty(src))
        {
            var uris = src.Split([",", " ", "\n", "\r"], StringSplitOptions.RemoveEmptyEntries)
                .Select(s => new Uri(s));
            dest.UnionWith(uris);
        }
    }

    private string Map(HashSet<Uri> src) => string.Join('\n', src);

    // Scope

    public partial OpenIddictScopeDescriptor Map(ScopeInputModel src);
    public partial ScopeInputModel Map(OpenIddictEntityFrameworkCoreScope src);
    public partial void Map(ScopeInputModel src, OpenIddictScopeDescriptor dest);

    // User

    public partial User Map(RegistrationInputModel src);
    public partial EditUserInputModel Map(User src);
}
