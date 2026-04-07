using Common;
using EventService_Application.DTOs;
using EventService_Application.Interfaces;
using EventService_Domain.Entities;
using EventService_Domain.Enums;
using EventService_Domain.Interfaces;
using Mapster;

namespace EventService_Application.Services
{
    public class LineupService : ILineupService
    {
        private readonly ILineupRepository _lineupRepository;
        private readonly IEventRepository _eventRepository;
        private readonly ITalentRepository _talentRepository;
        public LineupService(ILineupRepository lineupRepository, IEventRepository eventRepository, ITalentRepository talentRepository)
        {
            _lineupRepository = lineupRepository;
            _eventRepository = eventRepository;
            _talentRepository = talentRepository;
        }

        public async Task<ApiResponse<BatchLineupResultDto>> CreateLineupAsync(CreateLineupsRequest request)
        {
            var existingEvent = await _eventRepository.GetEventByIdAsync(request.EventId);
            if (existingEvent == null)
            {
                return ApiResponse<BatchLineupResultDto>.Fail(404, "Event not found");
            }

            // if (existingEvent.Sessions.Any())
            // {
            //     if (request.SessionId == null || existingEvent.Sessions.All(s => s.Id != request.SessionId))
            //     {
            //         return ApiResponse<BatchLineupResultDto>.Fail(
            //             400,
            //             "Invalid or missing SessionId for the specified Event"
            //         );
            //     }
            // }

            var uniqueTalentIds = request.TalentIds.Distinct().ToList();


            if (!uniqueTalentIds.Any())
            {
                return ApiResponse<BatchLineupResultDto>.Fail(400, "Talent list is empty");
            }

            var existingTalents =
                await _talentRepository.GetTalentByIdAsync(uniqueTalentIds);

            if (!existingTalents.Any())
            {
                return ApiResponse<BatchLineupResultDto>.Fail(404, "No talents found");
            }

            var validTalentIds = existingTalents.Select(t => t.Id).ToList();

            var existingLineups =
                await _lineupRepository.GetLineupsByEventAndTalentsAsync(
                    request.EventId,
                    request.SessionId,
                    validTalentIds
                );

            var existingTalentIdsInLineup =
                existingLineups.Select(l => l.TalentId).ToHashSet();

            var finalTalentIds = validTalentIds
                .Where(id => !existingTalentIdsInLineup.Contains(id))
                .ToList();

            var newLineups = finalTalentIds.Select(talentId => new Lineup
            {
                Id = Guid.NewGuid(),
                EventId = request.EventId,
                SessionId = request.SessionId,
                TalentId = talentId
            }).ToList();

            // Thay đổi nếu có quá nhiều data
            foreach (var lineup in newLineups)
            {
                await _lineupRepository.AddLineupAsync(lineup);
            }
            var talentLookup = existingTalents.ToDictionary(t => t.Id);
            var result = new BatchLineupResultDto
            {
                Created = newLineups.Select(l => new LineupDto
                {
                    Id = l.Id,
                    EventId = l.EventId,
                    SessionId = l.SessionId,
                    Talent = talentLookup[l.TalentId].Adapt<TalentDto>()
                }).ToList(),
                SkippedTalentIds = existingTalentIdsInLineup.ToList()
            };

            return ApiResponse<BatchLineupResultDto>.Success(
                200,
                "Lineups created successfully",
                result
            );
        }

        public async Task<ApiResponse<PagedResult<LineupDto>>> GetLineupsByEventIdAsync(Guid eventId, BaseQueryParams request)
        {
            var lineups = await _lineupRepository.GetLineupsByEventIdAsync(eventId, request.Page, request.PageSize, request.SortBy);
            var lineupDtos = lineups.Items.Select(l => new LineupDto
            {
                Id = l.Id,
                EventId = l.EventId,
                SessionId = l.SessionId,
                Talent = l.Talent.Adapt<TalentDto>()
            }).ToList();
            var result = new PagedResult<LineupDto>
            (
                lineupDtos,
                lineups.TotalItems,
                lineups.CurrentPage,
                lineups.PageSize
            );
            return ApiResponse<PagedResult<LineupDto>>.Success(
                200,
                "Lineups retrieved successfully",
                result
            );
        }

        public async Task<ApiResponse<LineupDto>> GetLineupById(Guid lineupId)
        {
            var lineup = await _lineupRepository.GetLineupByIdAsync(lineupId);
            if (lineup == null)
            {
                return ApiResponse<LineupDto>.Fail(404, "Lineup not found");
            }
            var lineupDto = new LineupDto
            {
                Id = lineup.Id,
                EventId = lineup.EventId,
                SessionId = lineup.SessionId,
                Talent = lineup.Talent.Adapt<TalentDto>()
            };
            return ApiResponse<LineupDto>.Success(200, "Lineup retrieved successfully", lineupDto);
        }

        public async Task<ApiResponse<PagedResult<LineupDto>>> GetAllLineups(BaseQueryParams request)
        {
            var lineups = await _lineupRepository.GetAllLineupsAsync(request.Page, request.PageSize, request.SortBy);
            if (lineups == null)
            {
                return ApiResponse<PagedResult<LineupDto>>.Fail(404, "No lineups found");
            }
            var lineupDtos = lineups.Items.Select(l => new LineupDto
            {
                Id = l.Id,
                EventId = l.EventId,
                SessionId = l.SessionId,
                Talent = l.Talent.Adapt<TalentDto>()
            }).ToList();
            var result = new PagedResult<LineupDto>
            (
                lineupDtos,
                lineups.TotalItems,
                lineups.CurrentPage,
                lineups.PageSize
            );

            return ApiResponse<PagedResult<LineupDto>>.Success(
                200,
                "Lineups retrieved successfully",
                result
            );
        }

        public async Task<ApiResponse<LineupDto>> UpdateLineupAsync(LineupDto request)
        {
            var existingLineup = await _lineupRepository.GetLineupByIdAsync(request.Id);
            if (existingLineup == null)
            {
                return ApiResponse<LineupDto>.Fail(404, "Lineup not found");
            }

            var existingEvent = await _eventRepository.GetEventByIdAsync(request.EventId);
            if (existingEvent == null)
            {
                return ApiResponse<LineupDto>.Fail(404, "Event not found");
            }

            // Không cho đổi Event (rule an toàn)
            if (existingLineup.EventId != request.EventId)
            {
                return ApiResponse<LineupDto>.Fail(400, "Changing Event is not allowed");
            }

            if (existingEvent.Sessions.Any())
            {
                if (request.SessionId == null ||
                    !existingEvent.Sessions.Any(s => s.Id == request.SessionId))
                {
                    return ApiResponse<LineupDto>.Fail(
                        400,
                        "Invalid or missing SessionId for the specified Event"
                    );
                }
            }

            if (existingLineup.TalentId != request.Talent.Id)
            {
                var existingTalent = await _talentRepository.GetTalentByIdAsync(request.Talent.Id);
                if (existingTalent == null)
                    return ApiResponse<LineupDto>.Fail(404, "Talent not found");
            }

            var duplicated = await _lineupRepository.LineupExistsAsync(
                request.EventId,
                request.SessionId,
                request.Talent.Id
            );

            if (duplicated)
            {
                return ApiResponse<LineupDto>.Fail(
                    409,
                    "Talent already exists in this Event/Session"
                );
            }

            existingLineup.SessionId = request.SessionId;
            existingLineup.TalentId = request.Talent.Id;

            await _lineupRepository.UpdateLineupAsync(existingLineup);

            var result = new LineupDto
            {
                Id = existingLineup.Id,
                EventId = existingLineup.EventId,
                SessionId = existingLineup.SessionId,
                Talent = existingLineup.Talent.Adapt<TalentDto>()
            };

            return ApiResponse<LineupDto>.Success(
                200,
                "Lineup updated successfully",
                result
            );
        }

        public async Task<ApiResponse<bool>> DeleteLineupAsync(Guid lineupId)
        {
            var existingLineup = await _lineupRepository.GetLineupByIdAsync(lineupId);
            if (existingLineup == null)
            {
                return ApiResponse<bool>.Fail(404, "Lineup not found");
            }
            if (existingLineup.Session != null && 
                existingLineup.Session.Status == SessionStatus.OnGoing)
            {
                return ApiResponse<bool>.Fail(400, "Cannot remove talent from an ongoing session");
            }
            var deleted = await _lineupRepository.DeleteLineupAsync(lineupId);
            if (!deleted)
            {
                return ApiResponse<bool>.Fail(500, "Failed to delete lineup");
            }
            return ApiResponse<bool>.Success(200, "Lineup deleted successfully", true);
        }
    }
}
