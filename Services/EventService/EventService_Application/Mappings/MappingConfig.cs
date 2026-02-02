using EventService_Application.DTOs;
using EventService_Domain.Entities;
using Mapster;

namespace EventService_Application.Mappings;

public class MappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        TypeAdapterConfig<Event, EventResponseDto>
            .NewConfig()
            .MaxDepth(3);
    }
}