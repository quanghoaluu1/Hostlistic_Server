namespace EventService_Application.DTOs;

public class UserPlanLookupResult
{
    public bool IsSuccess { get; set; }
    public int StatusCode { get; set; }
    public string Message { get; set; } = string.Empty;
    public IEnumerable<UserPlanDto> Plans { get; set; } = [];
}
