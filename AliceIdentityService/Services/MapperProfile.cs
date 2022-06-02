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
    }
}
