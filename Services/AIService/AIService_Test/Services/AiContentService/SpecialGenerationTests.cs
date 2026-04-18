using AIService_Application.DTOs.Responses;
using AIService_Test.Helpers.TestDataBuilders;
using FluentAssertions;
using NSubstitute;
using Microsoft.Extensions.Logging;
using Common;

namespace AIService_Test.Services.AiContentServiceTests;

public class SpecialGenerationTests
{
    private readonly IAiProvider _aiProvider = Substitute.For<IAiProvider>();
    private readonly IAiRequestRepository _aiRequestRepository = Substitute.For<IAiRequestRepository>();
    private readonly IAiGeneratedContentRepository _aiGeneratedContentRepository = Substitute.For<IAiGeneratedContentRepository>();
    private readonly IPromptTemplateRepository _promptTemplateRepository = Substitute.For<IPromptTemplateRepository>();
    private readonly IPromptTemplateEngine _promptTemplateEngine = Substitute.For<IPromptTemplateEngine>();
    private readonly IEventServiceClient _eventServiceClient = Substitute.For<IEventServiceClient>();
    private readonly ILogger<AiContentService> _logger = Substitute.For<ILogger<AiContentService>>();

    private readonly AiContentService _sut;

    public SpecialGenerationTests()
    {
        _sut = new AiContentService(
            _aiProvider,
            _aiRequestRepository,
            _aiGeneratedContentRepository,
            _promptTemplateRepository,
            _promptTemplateEngine,
            _eventServiceClient,
            _logger);
    }

    [Fact]
    public async Task GenerateSpeakerIntroAsync_WhenTalentNotFound_ReturnsFail404()
    {
        // Arrange
        var request = AiRequestBuilder.CreateSpeakerIntroRequest();
        var eventDetail = EventBuilder.CreateEventDetail(request.EventId);
        var emptyLineup = new LineupDetailDto { EventWideTalents = new List<LineupTalentDto>(), SessionTalents = new List<LineupSessionDto>() };

        _eventServiceClient.GetEventByIdAsync(request.EventId).Returns(eventDetail);
        _eventServiceClient.GetEventLineupAsync(request.EventId).Returns(emptyLineup);

        // Act
        var result = await _sut.GenerateSpeakerIntroAsync(request, Guid.NewGuid());

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(404);
        result.Message.Should().Contain("not found in event lineup");
    }

    [Fact]
    public async Task GenerateSessionAbstractAsync_WhenSessionNotFound_ReturnsFail404()
    {
        // Arrange
        var request = AiRequestBuilder.CreateSessionAbstractRequest();
        var eventDetail = EventBuilder.CreateEventDetail(request.EventId);
        eventDetail.Tracks = Array.Empty<TrackDetailDto>();

        _eventServiceClient.GetEventByIdAsync(request.EventId).Returns(eventDetail);

        // Act
        var result = await _sut.GenerateSessionAbstractAsync(request, Guid.NewGuid());

        // Assert
        result.StatusCode.Should().Be(404);
        result.Message.Should().Contain("not found in event");
    }

    [Fact]
    public async Task GenerateSpeakerIntroAsync_WhenSummarizeModeWithoutSourceText_ReturnsFail400()
    {
        // Arrange
        var request = new GenerateSpeakerIntroRequest
        {
            EventId = Guid.NewGuid(),
            TalentId = Guid.NewGuid(),
            Mode = "summarize",
            SourceText = null
        };

        // Act
        var result = await _sut.GenerateSpeakerIntroAsync(request, Guid.NewGuid());

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(400);
        result.Message.Should().Contain("SourceText is required");
    }

    [Fact]
    public async Task GenerateSpeakerIntroAsync_WithMinimalData_SetsDataQualityToMinimal()
    {
        // Arrange
        var request = AiRequestBuilder.CreateSpeakerIntroRequest();
        var eventDetail = EventBuilder.CreateEventDetail(request.EventId);
        var talent = new LineupTalentDto { TalentId = request.TalentId, Name = "John", Bio = null, Organization = null };
        var lineup = new LineupDetailDto { EventWideTalents = new List<LineupTalentDto> { talent }, SessionTalents = new List<LineupSessionDto>() };
        var template = PromptTemplateBuilder.CreatePromptTemplate(PromptTemplateKey.SpeakerIntroduction);

        _eventServiceClient.GetEventByIdAsync(request.EventId).Returns(eventDetail);
        _eventServiceClient.GetEventLineupAsync(request.EventId).Returns(lineup);
        _promptTemplateRepository.GetByKeyAsync(PromptTemplateKey.SpeakerIntroduction).Returns(template);
        _promptTemplateEngine.Render(Arg.Any<string>(), Arg.Any<Dictionary<string, string>>()).Returns("Prompt");
        _aiProvider.GenerateContentAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<AiRequestOptions>())
            .Returns(new AiProviderResult("Intro", 1, 1, "m"));

        // Act
        var result = await _sut.GenerateSpeakerIntroAsync(request, Guid.NewGuid());

        // Assert
        result.Data.Metadata.DataQuality.Should().Be("minimal");
        result.Data.Metadata.NeedsReview.Should().BeTrue();
    }

    [Fact]
    public async Task GenerateSessionAbstractAsync_WhenExpandModeWithoutSourceText_ReturnsFail400()
    {
        // Arrange
        var request = new GenerateSessionAbstractRequest
        {
            EventId = Guid.NewGuid(),
            SessionId = Guid.NewGuid(),
            Mode = "expand",
            SourceText = null
        };

        // Act
        var result = await _sut.GenerateSessionAbstractAsync(request, Guid.NewGuid());

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(400);
    }
}
