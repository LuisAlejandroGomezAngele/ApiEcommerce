using ApiEcommerce.Models;
using ApiEcommerce.Models.Dtos;
using Mapster;

namespace ApiEcommerce.Mapping;

public static class UserMapping
{
    public static void RegisterMappings(TypeAdapterConfig config)
    {
        config.NewConfig<User, UserDto>().TwoWays();
        config.NewConfig<User, UserRegisterDto>().TwoWays();
        config.NewConfig<User, UserLoginDto>().TwoWays();
        config.NewConfig<User, UserLoginResponseDto>().TwoWays();
        config.NewConfig<ApplicationUser, UserDataDto>().TwoWays();
        config.NewConfig<ApplicationUser, User>().TwoWays();

#pragma warning disable CS8603
        config.NewConfig<ApplicationUser, UserDto>()
            .Map(dest => dest.Username, src => src.UserName)
            .Ignore(dest => dest.Password)
            .Ignore(dest => dest.Role);
#pragma warning restore CS8603

        config.NewConfig<UserDto, ApplicationUser>()
            .Map(dest => dest.UserName, src => src.Username);
    }
}