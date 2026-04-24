using AIService_Domain.Entities;
using FluentAssertions;
using NSubstitute;
using Microsoft.Extensions.Logging;
using Common;

namespace AIService_Test.Services.AiContentServiceTests;

public class UsageTests
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

    public UsageTests()
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
    public async Task GetAIToken_ReturnsChartData()
    {
        // Arrange
        var usage = new List<AiGeneratedContent>
        {
            new() { CreatedAt = DateTime.UtcNow.AddDays(-1), PromptTokens = 100, CompletionTokens = 50 },
            new() { CreatedAt = DateTime.UtcNow.AddDays(-2), PromptTokens = 80, CompletionTokens = 40 }
        };
        _aiGeneratedContentRepository.GetAiTokenChart().Returns(usage);

        // Act
        var result = await _sut.GetAIToken();

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data.Should().NotBeEmpty();
    }
}
