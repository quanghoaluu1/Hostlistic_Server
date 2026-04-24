using AIService_Application.DTOs.Responses;
using AIService_Test.Helpers.TestDataBuilders;
using AIService_Domain.Enum;
using AIService_Domain.Entities;
using FluentAssertions;
using NSubstitute;
using Microsoft.Extensions.Logging;
using Common;

namespace AIService_Test.Services.AiContentServiceTests;

public class GenerationTests
{
    private readonly IAiProvider _aiProvider = Substitute.For<IAiProvider>();
    private readonly IAiRequestRepository _aiRequestRepository = Substitute.For<IAiRequestRepository>();
    private readonly IAiGeneratedContentRepository _aiGeneratedContentRepository = Substitute.For<IAiGeneratedContentRepository>();
    private readonly IPromptTemplateRepository _promptTemplateRepository = Substitute.For<IPromptTemplateRepository>();
    private readonly IPromptTemplateEngine _promptTemplateEngine = Substitute.For<IPromptTemplateEngine>();
    private readonly IEventServiceClient _eventServiceClient = Substitute.For<IEventServiceClient>();
    private readonly ILogger<AiContentService> _logger = Substitute.For<ILogger<AiContentService>>();
    private readonly IAiDataAggregationService _aiDataAggregationService = Substitute.For<IAiDataAggregationService>();

    private readonly AiContentService _sut;

    public GenerationTests()
    {
        _sut = new AiContentService(
            _aiProvider,
            _aiRequestRepository,
            _aiGeneratedContentRepository,
            _promptTemplateRepository,
            _promptTemplateEngine,
            _eventServiceClient,
            _aiDataAggregationService,
            _logger);
    }

    [Fact]
    public async Task GenerateDescriptionAsync_WithValidRequest_ReturnsSuccess()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var request = AiRequestBuilder.CreateDescriptionRequest();
        var eventDetail = EventBuilder.CreateEventDetail(request.EventId);
        var template = PromptTemplateBuilder.CreatePromptTemplate(PromptTemplateKey.EventDescription);
        
        _eventServiceClient.GetEventByIdAsync(request.EventId).Returns(eventDetail);
        _promptTemplateRepository.GetByKeyAsync(PromptTemplateKey.EventDescription).Returns(template);
        _promptTemplateEngine.BuildParametersFromEvent(eventDetail).Returns(new Dictionary<string, string>());
        _promptTemplateEngine.AddToneAndLanguage(Arg.Any<Dictionary<string, string>>(), request.Tone, request.Language)
            .Returns(new Dictionary<string, string>());
        _promptTemplateEngine.Render(template.UserPromptTemplate, Arg.Any<Dictionary<string, string>>())
            .Returns("Rendered Prompt");
        
        _aiProvider.GenerateContentAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<AiRequestOptions>())
            .Returns(new AiProviderResult("<p>Generated content</p>", 10, 20, "gemini-pro"));
        _promptTemplateEngine.SanitizeHtml(Arg.Any<string>()).Returns("<p>Generated content</p>");

        // Act
        var result = await _sut.GenerateDescriptionAsync(request, userId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(200);
        result.Data.HtmlContent.Should().Be("<p>Generated content</p>");
        
        await _aiRequestRepository.Received(2).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GenerateDescriptionAsync_WhenEventNotFound_ReturnsFail500()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var request = AiRequestBuilder.CreateDescriptionRequest();
        _eventServiceClient.GetEventByIdAsync(request.EventId).Returns((EventDetailDto?)null);

        // Act
        var result = await _sut.GenerateDescriptionAsync(request, userId);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(404);
        result.Message.Should().Be("Event not found");
    }

    [Fact]
    public async Task GenerateEmailAsync_WhenEventNotFound_ReturnsFail404()
    {
        // Arrange
        var request = AiRequestBuilder.CreateEmailRequest();
        _eventServiceClient.GetEventByIdAsync(request.EventId).Returns((EventDetailDto?)null);

        // Act
        var result = await _sut.GenerateEmailAsync(request, Guid.NewGuid());

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(404);
        result.Message.Should().Be("Event not found");
    }

    [Fact]
    public async Task GenerateSocialPostAsync_WhenEventNotFound_ReturnsFail404()
    {
        // Arrange
        var request = AiRequestBuilder.CreateSocialPostRequest();
        _eventServiceClient.GetEventByIdAsync(request.EventId).Returns((EventDetailDto?)null);

        // Act
        var result = await _sut.GenerateSocialPostAsync(request, Guid.NewGuid());

        // Assert
        result.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task GenerateEmailAsync_WhenUnknownEmailType_ThrowsBadHttpRequestException()
    {
        // Arrange
        var request = new GenerateEmailRequest
        {
            EventId = Guid.NewGuid(),
            EmailType = "unknown_type"
        };
        var eventDetail = EventBuilder.CreateEventDetail(request.EventId);
        _eventServiceClient.GetEventByIdAsync(request.EventId).Returns(eventDetail);

        // Act & Assert
        await _sut.Invoking(s => s.GenerateEmailAsync(request, Guid.NewGuid()))
            .Should().ThrowAsync<Microsoft.AspNetCore.Http.BadHttpRequestException>()
            .WithMessage("Unknown email type: unknown_type");
    }

    [Fact]
    public async Task GenerateSocialPostAsync_WhenContentExceedsPlatformLimit_SetsExceedsLimitTrue()
    {
        // Arrange
        var request = AiRequestBuilder.CreateSocialPostRequest(platform: "X");
        var eventDetail = EventBuilder.CreateEventDetail(request.EventId);
        var template = PromptTemplateBuilder.CreatePromptTemplate(PromptTemplateKey.SocialMediaPost);

        _eventServiceClient.GetEventByIdAsync(request.EventId).Returns(eventDetail);
        _promptTemplateRepository.GetByKeyAsync(PromptTemplateKey.SocialMediaPost).Returns(template);
        _promptTemplateEngine.BuildSocialPostParameters(eventDetail, request).Returns(new Dictionary<string, string>());
        _promptTemplateEngine.Render(Arg.Any<string>(), Arg.Any<Dictionary<string, string>>()).Returns("Social Prompt");

        _aiProvider.GenerateContentAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<AiRequestOptions>())
            .Returns(new AiProviderResult("Very long content that exceeds X limit...", 10, 20, "gemini-pro"));
        _promptTemplateEngine.ParseSocialPostResponse(Arg.Any<string>(), Arg.Any<string>()).Returns(("Very long content that exceeds X limit...", ""));
        _promptTemplateEngine.GetPlatformCharacterLimit("X").Returns(10); // Artificial low limit

        // Act
        var result = await _sut.GenerateSocialPostAsync(request, Guid.NewGuid());

        // Assert
        result.Data.ExceedsLimit.Should().BeTrue();
    }
}
