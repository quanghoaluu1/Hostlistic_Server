using AIService_Application.Services;
using AIService_Test.Helpers.TestDataBuilders;
using FluentAssertions;

namespace AIService_Test.Services.PromptTemplateEngineTests;

public class RenderTests
{
    private readonly PromptTemplateEngine _sut = new();

    [Fact]
    public async Task Render_WithSimplePlaceholders_ReplacesCorrectly()
    {
        // Arrange
        var template = "Hello {{name}}, welcome to {{event}}!";
        var parameters = new Dictionary<string, string>
        {
            ["name"] = "Alice",
            ["event"] = "TechnoConf"
        };

        // Act
        var result = _sut.Render(template, parameters);

        // Assert
        result.Should().Be("Hello Alice, welcome to TechnoConf!");
    }

    [Fact]
    public async Task Render_WithConditionalBlocks_HandlesTrueAndFalse()
    {
        // Arrange
        var template = "Promo: {{#has_discount}}Save {{discount}}!{{/has_discount}} Enjoy the show.";
        
        var paramsTrue = new Dictionary<string, string> { ["has_discount"] = "true", ["discount"] = "20%" };
        var paramsFalse = new Dictionary<string, string> { ["has_discount"] = "" };

        // Act
        var resultTrue = _sut.Render(template, paramsTrue);
        var resultFalse = _sut.Render(template, paramsFalse);

        // Assert
        resultTrue.Should().Contain("Save 20%!");
        resultFalse.Should().NotContain("Save");
        resultFalse.Should().Be("Promo:  Enjoy the show."); // Note: double space is expected behavior of current Regex
    }

    [Fact]
    public async Task BuildParametersFromEvent_ExtractsAllFields()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var eventDetail = EventBuilder.CreateEventDetail(eventId);
        eventDetail.Title = "AI Summit";
        eventDetail.Location = "Hanoi";
        eventDetail.EventMode = "Online";
        eventDetail.EventTypeName = "Workshop";
        eventDetail.StartDate = DateTime.UtcNow;
        eventDetail.EndDate = DateTime.UtcNow.AddHours(2);

        // Act
        var result = _sut.BuildParametersFromEvent(eventDetail);

        // Assert
        result["event_title"].Should().Be("AI Summit");
        result["event_location"].Should().Be("Hanoi");
    }
}

public class ParsingTests
{
    private readonly PromptTemplateEngine _sut = new();

    [Fact]
    public async Task ParseEmailResponse_ExtractsSubjectAndBody()
    {
        // Arrange
        var raw = "SUBJECT: Welcome to the event\nThis is the body content.";

        // Act
        var (subject, body) = _sut.ParseEmailResponse(raw);

        // Assert
        subject.Should().Be("Welcome to the event");
        body.Should().Contain("This is the body content.");
        body.Should().StartWith("<div>");
    }

    [Fact]
    public async Task ParseSocialPostResponse_SplitsContentAndHashtags()
    {
        // Arrange
        var raw = "Check out this amazing event!\n\n#AI #Tech #Summit";

        // Act
        var (content, hashtags) = _sut.ParseSocialPostResponse(raw, null);

        // Assert
        content.Should().Be("Check out this amazing event!");
        hashtags.Should().Be("#AI #Tech #Summit");
    }

    [Fact]
    public async Task SanitizeHtml_RemovesExcessTagsAndCodeFences()
    {
        // Arrange
        var raw = "```html\n<div><body><h2>Title</h2></body></div>\n```";

        // Act
        var result = _sut.SanitizeHtml(raw);

        // Assert
        result.Should().Be("<h2>Title</h2>");
        result.Should().NotContain("body");
        result.Should().NotContain("```");
    }
}
