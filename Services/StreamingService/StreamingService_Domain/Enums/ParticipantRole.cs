using Microsoft.Extensions.Hosting;

namespace StreamingService_Domain.Enums;

public enum ParticipantRole
{
    Host, //Organizer - full quyen
    Talent, 
    Moderator, // Mute, kick
    Viewer //chi dc xem
}