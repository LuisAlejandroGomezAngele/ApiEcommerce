using System;
using AutoMapper;
using ApiEcommerce.Models;
using ApiEcommerce.Models.Dtos;

namespace ApiEcommerce.Mapping;

public class UserProfile : Profile
{
    public UserProfile()
    {
        CreateMap<User, UserDto>().ReverseMap();
        CreateMap<User, UserRegisterDto>().ReverseMap();
        CreateMap<User, UserLoginDto>().ReverseMap();
        CreateMap<User, UserLoginResponseDto>().ReverseMap();
        CreateMap<ApplicationUser, UserDataDto>().ReverseMap();
        CreateMap<ApplicationUser, User>().ReverseMap();

        // Mapeo entre ApplicationUser (Identity) y UserDto usado por el controlador
        CreateMap<ApplicationUser, UserDto>()
            .ForMember(dest => dest.Username, opt => opt.MapFrom(src => src.UserName))
            // Evitar mapear valores sensibles o ausentes (Password se mantiene nulo)
            .ForMember(dest => dest.Password, opt => opt.Ignore())
            .ForMember(dest => dest.Role, opt => opt.Ignore())
            .ReverseMap()
            .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.Username));

    }
}