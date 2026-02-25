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
            .Map(dest => dest.EventTypeName, src => src.EventType.Name);

        TypeAdapterConfig<Session, SessionResponseDto>
            .NewConfig()
            .Map(dest => dest.Talents, src => src.Lineups.Select(l => l.Talent));

        TypeAdapterConfig<Talent, TalentDetailDto>
            .NewConfig();
    }
}