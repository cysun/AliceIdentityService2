using AliceIdentityService.Models;
using AutoMapper;

namespace AliceIdentityService.Services
{
    public class MapperProfile : Profile
    {
        public MapperProfile()
        {
            CreateMap<RegistrationInputModel, User>();
        }
    }
}
