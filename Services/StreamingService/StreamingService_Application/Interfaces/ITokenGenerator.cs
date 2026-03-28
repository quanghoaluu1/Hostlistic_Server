using StreamingService_Domain.Enums;

namespace StreamingService_Application.Interfaces;

public interface ITokenGenerator
{
    string GenerateLiveKitToken(string roomName, string identity, ParticipantRole role);
}
