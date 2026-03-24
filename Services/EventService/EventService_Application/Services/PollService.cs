using Common;
using EventService_Application.DTOs;
using EventService_Application.Interfaces;
using EventService_Domain.Entities;
using EventService_Domain.Enums;
using EventService_Domain.Interfaces;
using Mapster;

namespace EventService_Application.Services
{
    public class PollService : IPollService
    {
        private readonly IPollRepository _pollRepository;
        private readonly ISessionRepository _sessionRepository;
        public PollService(IPollRepository pollRepository, ISessionRepository sessionRepository)
        {
            _pollRepository = pollRepository;
            _sessionRepository = sessionRepository;
        }

        public async Task<ApiResponse<PollDto>> AddPollAsync(CreatePollRequest request)
        {
            //Check session tồn tại
            var existingSession = await _sessionRepository.GetSessionByIdAsync(request.SessionId);
            if (existingSession == null)
            {
                return ApiResponse<PollDto>.Fail(404, "Session not found.");
            }

            //Validate question
            if (string.IsNullOrWhiteSpace(request.Question))
            {
                return ApiResponse<PollDto>.Fail(400, "Question is required.");
            }

            //Validate options
            if (request.Options == null || request.Options.Count < 2)
            {
                return ApiResponse<PollDto>.Fail(400, "At least two options are required.");
            }

            //Validate duration
            if (request.DurationInSecond.HasValue && request.DurationInSecond <= 0)
            {
                return ApiResponse<PollDto>.Fail(400, "Duration must be greater than zero.");
            }

            //Validate CorrectAnswers theo PollType
            if (request.Type == PollType.Quiz)
            {
                if (request.CorrectAnswers == null || request.CorrectAnswers.Count == 0)
                {
                    return ApiResponse<PollDto>.Fail(400, "Correct answers are required for quiz poll.");
                }

                //Check index hợp lệ
                if (request.CorrectAnswers.Any(i => i < 0 || i >= request.Options.Count))
                {
                    return ApiResponse<PollDto>.Fail(400, "Correct answer index is invalid.");
                }

                //Check duplicate
                if (request.CorrectAnswers.Count != request.CorrectAnswers.Distinct().Count())
                {
                    return ApiResponse<PollDto>.Fail(400, "Duplicate correct answers are not allowed.");
                }
            }

            if (request.Type == PollType.Survey)
            {
                if (request.CorrectAnswers != null && request.CorrectAnswers.Count > 0)
                {
                    return ApiResponse<PollDto>.Fail(400, "Survey poll cannot have correct answers.");
                }
            }

            //Normalize options (Order / Id)
            var normalizedOptions = request.Options
                .Select((opt, index) => new PollOption
                {
                    Id = index,
                    Text = opt.Text,
                    Order = index,
                    ImageUrl = opt.ImageUrl
                })
                .ToList();

            //Create Poll
            var newPoll = new Poll
            {
                Id = Guid.NewGuid(),
                SessionId = request.SessionId,
                Question = request.Question,
                Options = normalizedOptions,
                CorrectAnswers = request.CorrectAnswers ?? [],
                Type = request.Type,
                IsPrivate = request.IsPrivate,
                DurationInSecond = request.DurationInSecond,

                //Create Poll luôn ở trạng thái Draft
                IsActive = false
            };

            await _pollRepository.AddPollAsync(newPoll);

            var pollDto = newPoll.Adapt<PollDto>();

            return ApiResponse<PollDto>.Success(200, "Poll created successfully.", pollDto);
        }

        public async Task<ApiResponse<PollDto>> GetPollByIdAsync(Guid pollId)
        {
            var poll = await _pollRepository.GetPollByIdAsync(pollId);
            if (poll == null)
            {
                return ApiResponse<PollDto>.Fail(404, "Poll not found.");
            }
            var pollDto = poll.Adapt<PollDto>();
            return ApiResponse<PollDto>.Success(200, "Poll retrieved successfully.", pollDto);
        }

        public async Task<ApiResponse<PagedResult<PollDto>>> GetPollsBySessionIdAsync(Guid sessionId, BaseQueryParams request)
        {
            var polls = await _pollRepository.GetPollsBySessionIdAsync(sessionId, request.Page, request.PageSize, request.SortBy);
            var pollDtos = polls.Adapt<List<PollDto>>();
            var pagedResult = new PagedResult<PollDto>
            (
                pollDtos,
                polls.TotalItems,
                polls.TotalPages,
                polls.PageSize
            );
            return ApiResponse<PagedResult<PollDto>>.Success(200, "Polls retrieved successfully.", pagedResult);
        }

        public async Task<ApiResponse<PollDto>> UpdatePollAsync(Guid pollId, UpdatePollRequest request)
        {
            var existingPoll = await _pollRepository.GetPollByIdAsync(pollId);
            if (existingPoll == null)
            {
                return ApiResponse<PollDto>.Fail(404, "Poll not found.");
            }

            //Chỉ cho phép update khi Draft
            if (existingPoll.IsActive)
            {
                return ApiResponse<PollDto>.Fail(400, "Only draft poll can be updated.");
            }

            //Update Question
            if (!string.IsNullOrWhiteSpace(request.Question))
            {
                existingPoll.Question = request.Question;
            }

            //Update Options (nếu được gửi)
            if (request.Options != null)
            {
                if (request.Options.Count < 2)
                {
                    return ApiResponse<PollDto>.Fail(400, "At least two options are required.");
                }

                existingPoll.Options = request.Options
                    .Select((opt, index) => new PollOption
                    {
                        Id = index,
                        Text = opt.Text,
                        Order = index,
                        ImageUrl = opt.ImageUrl
                    })
                    .ToList();

                // Update Options → Reset CorrectAnswers
                existingPoll.CorrectAnswers = [];
            }

            //CorrectAnswers (chỉ khi Quiz & được gửi)
            if (request.CorrectAnswers != null)
            {
                if (existingPoll.Type != PollType.Quiz)
                {
                    return ApiResponse<PollDto>.Fail(400, "Correct answers only apply to quiz poll.");
                }

                if (request.CorrectAnswers.Count == 0)
                {
                    return ApiResponse<PollDto>.Fail(400, "Quiz poll must have at least one correct answer.");
                }

                if (request.CorrectAnswers.Any(i => i < 0 || i >= existingPoll.Options.Count))
                {
                    return ApiResponse<PollDto>.Fail(400, "Correct answer index is invalid.");
                }

                if (request.CorrectAnswers.Count != request.CorrectAnswers.Distinct().Count())
                {
                    return ApiResponse<PollDto>.Fail(400, "Duplicate correct answers are not allowed.");
                }

                existingPoll.CorrectAnswers = request.CorrectAnswers;
            }

            //Validate Survey logic
            if (existingPoll.Type == PollType.Survey)
            {
                existingPoll.CorrectAnswers = [];
            }

            //Update Duration
            if (request.DurationInSecond.HasValue)
            {
                if (request.DurationInSecond <= 0)
                {
                    return ApiResponse<PollDto>.Fail(400, "Duration must be greater than zero.");
                }

                existingPoll.DurationInSecond = request.DurationInSecond;
            }

            //Update IsPrivate
            existingPoll.IsPrivate = request.IsPrivate;

            await _pollRepository.UpdatePollAsync(existingPoll);

            var pollDto = existingPoll.Adapt<PollDto>();

            return ApiResponse<PollDto>.Success(200, "Poll updated successfully.", pollDto);
        }

        public async Task<ApiResponse<bool>> DeletePollAsync(Guid pollId)
        {
            var existingPoll = await _pollRepository.GetPollByIdAsync(pollId);
            if (existingPoll == null)
            {
                return ApiResponse<bool>.Fail(404, "Poll not found.");
            }
            //Chỉ cho phép delete khi Poll đang ở trạng thái Draft (chưa active)
            if (existingPoll.IsActive)
            {
                return ApiResponse<bool>.Fail(400, "Only draft poll can be deleted.");
            }
            await _pollRepository.DeletePollAsync(existingPoll);
            return ApiResponse<bool>.Success(200, "Poll deleted successfully.", true);
        }
        public async Task<ApiResponse<PollDto>> ActivatePollAsync(Guid pollId)
        {
            var existingPoll = await _pollRepository.GetPollByIdAsync(pollId);
            if (existingPoll == null)
            {
                return ApiResponse<PollDto>.Fail(404, "Poll not found.");
            }
            //Chỉ cho phép activate khi Poll đang ở trạng thái Draft
            if (existingPoll.IsActive)
            {
                return ApiResponse<PollDto>.Fail(400, "Only draft poll can be activated.");
            }
            existingPoll.IsActive = true;
            await _pollRepository.UpdatePollAsync(existingPoll);
            var pollDto = existingPoll.Adapt<PollDto>();
            return ApiResponse<PollDto>.Success(200, "Poll activated successfully.", pollDto);
        }
    }
}
