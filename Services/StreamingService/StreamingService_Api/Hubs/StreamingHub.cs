using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using StreamingService_Application.Interfaces;
using System.Security.Claims;

namespace StreamingService_Api.Hubs;

[Authorize]
public class StreamingHub : Hub
{
    private readonly IEventServiceClient _eventServiceClient;

    public StreamingHub(IEventServiceClient eventServiceClient)
    {
        _eventServiceClient = eventServiceClient;
    }

    public Task JoinEventGroup(string eventId)
    {
        return Groups.AddToGroupAsync(Context.ConnectionId, BuildEventGroup(eventId));
    }

    public Task LeaveEventGroup(string eventId)
    {
        return Groups.RemoveFromGroupAsync(Context.ConnectionId, BuildEventGroup(eventId));
    }

    public async Task SendEventChatMessage(string eventId, string sessionId, string sender, string message)
    {
        if (string.IsNullOrWhiteSpace(eventId) || string.IsNullOrWhiteSpace(sessionId) || string.IsNullOrWhiteSpace(message))
        {
            return;
        }

        var userIdClaim = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier) ?? Context.User?.FindFirstValue("sub");
        if (!Guid.TryParse(userIdClaim, out var userId))
        {
            throw new HubException("You must be authenticated to send chat messages.");
        }

        if (!Guid.TryParse(eventId, out var parsedEventId) || !Guid.TryParse(sessionId, out var parsedSessionId))
        {
            throw new HubException("Invalid event or session id.");
        }

        var chatAccess = await _eventServiceClient.GetEventChatAccessAsync(parsedEventId, parsedSessionId, userId);
        if (!chatAccess.CanSendChat)
        {
            // Smooth UX: notify only the caller instead of throwing HubException.
            await Clients.Caller.SendAsync("EventChatBlocked", new
            {
                EventId = eventId,
                SessionId = sessionId,
                Message = chatAccess.ChatBlockedUntil.HasValue
                    ? $"Chat is blocked until {chatAccess.ChatBlockedUntil.Value:u}."
                    : "Chat is blocked for this session.",
                ChatBlockedUntil = chatAccess.ChatBlockedUntil
            });
            return;
        }

        var safeSender = string.IsNullOrWhiteSpace(sender) ? "Guest" : sender.Trim();
        var safeRole = string.IsNullOrWhiteSpace(chatAccess.Role) ? "Viewer" : chatAccess.Role.Trim();
        var safeMessage = message.Trim();

        await Clients.Group(BuildEventGroup(eventId)).SendAsync("EventChatMessageReceived", new
        {
            Id = Guid.NewGuid().ToString(),
            EventId = eventId,
            SessionId = sessionId,
            UserId = userId,
            Sender = safeSender,
            Role = safeRole,
            Message = safeMessage,
            SentAt = DateTime.UtcNow
        });
    }

    public async Task DeleteEventChatMessage(string eventId, string sessionId, string messageId)
    {
        if (string.IsNullOrWhiteSpace(eventId) || string.IsNullOrWhiteSpace(messageId))
        {
            return;
        }

        var userIdClaim = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier) ?? Context.User?.FindFirstValue("sub");
        if (!Guid.TryParse(userIdClaim, out var userId))
        {
            throw new HubException("You must be authenticated to delete chat messages.");
        }

        if (!Guid.TryParse(eventId, out var parsedEventId) || !Guid.TryParse(sessionId, out var parsedSessionId))
        {
            throw new HubException("Invalid event or session id.");
        }

        // Verify moderation role in the same session/track context.
        var chatAccess = await _eventServiceClient.GetEventChatAccessAsync(parsedEventId, parsedSessionId, userId);
        var role = chatAccess.Role?.ToLower() ?? "viewer";
        bool canModerate = role == "organizer" || role == "coorganizer" || role == "staff";

        if (!canModerate)
        {
            throw new HubException("You do not have permission to delete chat messages.");
        }

        await Clients.Group(BuildEventGroup(eventId)).SendAsync("EventChatMessageDeleted", new
        {
            EventId = eventId,
            MessageId = messageId,
            DeletedByUserId = userId
        });
    }

    public static string BuildEventGroup(string eventId) => $"event:{eventId}";
}
