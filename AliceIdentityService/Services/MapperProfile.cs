using AliceIdentityService.Models;
using AutoMapper;
using OpenIddict.Abstractions;
using OpenIddict.EntityFrameworkCore.Models;

namespace AliceIdentityService.Services;

public class MapperProfile : Profile
{
    public MapperProfile()
    {
        CreateMap<RegistrationInputModel, User>();
        CreateMap<User, EditUserInputModel>();
        CreateMap<ScopeInputModel, OpenIddictScopeDescriptor>();
        CreateMap<OpenIddictEntityFrameworkCoreScope, ScopeInputModel>();

        CreateMap<string, HashSet<Uri>>().ConvertUsing(new StringToUrisConverter());
        CreateMap<HashSet<Uri>, string>().ConvertUsing(new UrisToStringConverter());

        CreateMap<ApplicationInputModel, OpenIddictApplicationDescriptor>()
            .ForMember(dest => dest.ClientSecret, opt => opt.Condition(src => src.IsNewClientSecret))
            .ForMember(dest => dest.ClientType, opt => opt.MapFrom((src, dest) =>
            {
                if (!src.IsNewClientSecret) return dest.ClientType;
                return string.IsNullOrEmpty(src.ClientSecret) ?
                    OpenIddictConstants.ClientTypes.Public : OpenIddictConstants.ClientTypes.Confidential;
            }));

        CreateMap<OpenIddictApplicationDescriptor, ApplicationInputModel>()
            .ForMember(dest => dest.ClientSecret, opt => opt.MapFrom(src =>
                string.IsNullOrEmpty(src.ClientSecret) ? src.ClientSecret : "********"))
            .ForMember(dest => dest.IsPkce, opt => opt.MapFrom(src =>
                src.Requirements.Contains(OpenIddictConstants.Requirements.Features.ProofKeyForCodeExchange)));
    }

    public class StringToUrisConverter : ITypeConverter<string, HashSet<Uri>>
    {
        public HashSet<Uri> Convert(string source, HashSet<Uri> destination, ResolutionContext context)
        {
            destination.Clear();
            if (!string.IsNullOrEmpty(source))
            {
                var uris = source.Split(new string[] { ",", " ", "\n", "\r" }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(s => new Uri(s));
                destination.UnionWith(uris);
            }
            return destination;
        }
    }

    public class UrisToStringConverter : ITypeConverter<HashSet<Uri>, string>
    {
        public string Convert(HashSet<Uri> source, string destination, ResolutionContext context)
        {
            return string.Join('\n', source);
        }
    }
}
