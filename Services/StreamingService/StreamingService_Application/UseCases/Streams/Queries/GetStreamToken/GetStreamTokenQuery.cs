using MediatR;
using Microsoft.EntityFrameworkCore;
using StreamingService_Application.Interfaces;
using StreamingService_Domain.Entities;
using StreamingService_Domain.Enums;

namespace StreamingService_Application.UseCases.Streams.Queries.GetStreamToken;

public record GetStreamTokenQuery(Guid RoomId, Guid UserId, string Identity, ParticipantRole Role) : IRequest<string>;

public class GetStreamTokenQueryHandler : IRequestHandler<GetStreamTokenQuery, string>
{
    private readonly IStreamingServiceDbContext _dbContext;
    private readonly ITokenGenerator _tokenGenerator;
    private readonly IEventServiceClient _eventServiceClient;

    public GetStreamTokenQueryHandler(IStreamingServiceDbContext dbContext, ITokenGenerator tokenGenerator, IEventServiceClient eventServiceClient)
    {
        _dbContext = dbContext;
        _tokenGenerator = tokenGenerator;
        _eventServiceClient = eventServiceClient;
    }

    public async Task<string> Handle(GetStreamTokenQuery request, CancellationToken cancellationToken)
    {
        var room = await _dbContext.Set<StreamRoom>()
            .FirstOrDefaultAsync(r => r.Id == request.RoomId, cancellationToken);

        if (room == null)
            throw new Exception("Stream room not found");

        if (room.Status == StreamRoomStatus.Ended)
            throw new Exception("Stream room has already ended");

        // Request StreamAuth from EventService
        var authResult = await _eventServiceClient.VerifyStreamAccessAsync(room.EventId, request.UserId, cancellationToken);
        if (!authResult.IsAllowed)
        {
            throw new UnauthorizedAccessException(authResult.ErrorMessage ?? "Access denied to stream room");
        }

        // Determine exact role returned by EventService
        if (!Enum.TryParse<ParticipantRole>(authResult.Role, true, out var exactRole))
        {
            exactRole = ParticipantRole.Attendee; 
        }

        // Generate token using user identity and RoomName natively.
        var token = _tokenGenerator.GenerateLiveKitToken(room.LiveKitRoomName, request.Identity, exactRole);

        // Add or update participant in DB
        var participant = await _dbContext.Set<StreamParticipant>()
            .FirstOrDefaultAsync(p => p.StreamRoomId == request.RoomId && p.UserId == request.UserId, cancellationToken);
            
        if (participant == null)
        {
            participant = new StreamParticipant
            {
                Id = Guid.NewGuid(),
                StreamRoomId = request.RoomId,
                UserId = request.UserId,
                LiveKitIdentity = request.Identity,
                Role = exactRole,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            _dbContext.Set<StreamParticipant>().Add(participant);
        }
        else
        {
            participant.LiveKitIdentity = request.Identity;
            participant.Role = exactRole;
            participant.UpdatedAt = DateTime.UtcNow;
            _dbContext.Set<StreamParticipant>().Update(participant);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        return token;
    }
}
