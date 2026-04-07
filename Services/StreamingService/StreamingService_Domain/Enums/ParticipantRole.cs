using Microsoft.Extensions.Hosting;

namespace StreamingService_Domain.Enums;

public enum ParticipantRole
{
    Organizer, 
    CoOrganizer,
    Staff,
    Attendee
}