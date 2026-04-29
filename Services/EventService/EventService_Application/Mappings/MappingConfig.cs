using EventService_Application.DTOs;
using EventService_Domain.Entities;
using Mapster;

namespace EventService_Application.Mappings;

public class MappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        TypeAdapterConfig<EventRequestDto, Event>
            .NewConfig()
            .Map(dest => dest.LocationAddress, src => src.LocationAddress)
            .Map(dest => dest.Latitude, src => src.Latitude)
            .Map(dest => dest.Longitude, src => src.Longitude);

        TypeAdapterConfig<Event, EventResponseDto>
            .NewConfig()
            .Map(dest => dest.EventTypeName, src => src.EventType.Name)
            .Map(dest => dest.LocationAddress, src => src.LocationAddress)
            .Map(dest => dest.Latitude, src => src.Latitude)
            .Map(dest => dest.Longitude, src => src.Longitude);

        TypeAdapterConfig<Session, SessionResponseDto>
            .NewConfig()
            .Map(dest => dest.Talents, src => src.Lineups.Select(l => l.Talent));

        TypeAdapterConfig<Talent, TalentDetailDto>
            .NewConfig();
    }
}