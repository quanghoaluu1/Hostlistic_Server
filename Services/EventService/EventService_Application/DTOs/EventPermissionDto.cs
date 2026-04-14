namespace EventService_Application.DTOs;

public sealed record EventPermissionDto(
    bool IsOwner,
    string? Role,
    Dictionary<string, bool> Permissions);
