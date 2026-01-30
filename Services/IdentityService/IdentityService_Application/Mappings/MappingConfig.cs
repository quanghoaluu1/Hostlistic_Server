using Common.DTOs;
using IdentityService_Application.DTOs;
using IdentityService_Domain.Entities;
using Mapster;

namespace IdentityService_Application.Mappings;

public class MappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<RegisterRequest, User>()
            .Map(dest => dest.Email, src => src.Email)
            .Ignore(dest => dest.HashedPassword)
            .Map(dest => dest.Id, src => Guid.NewGuid())
            .Map(dest => dest.CreatedAt, src => DateTime.UtcNow);
        config.NewConfig<User, UserDto>()
            .Map(dest => dest.FullName, src => src.FullName);
    }
}