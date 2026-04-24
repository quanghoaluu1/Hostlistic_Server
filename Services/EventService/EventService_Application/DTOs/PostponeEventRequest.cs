namespace EventService_Application.DTOs;

public record PostponeEventRequest(DateTime? NewStartTime, DateTime? NewEndTime, string Reason);
