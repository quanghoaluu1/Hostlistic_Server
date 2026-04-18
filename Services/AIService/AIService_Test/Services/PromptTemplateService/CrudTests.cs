using AIService_Application.DTOs.Requests;
using AIService_Application.Services;
using AIService_Domain.Entities;
using AIService_Domain.Enum;
using AIService_Domain.Interfaces;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace AIService_Test.Services.PromptTemplateServiceTests;

public class CrudTests
{
    private readonly IPromptTemplateRepository _repository = Substitute.For<IPromptTemplateRepository>();
    private readonly ILogger<PromptTemplateService> _logger = Substitute.For<ILogger<PromptTemplateService>>();
    private readonly PromptTemplateService _sut;

    public CrudTests()
    {
        _sut = new PromptTemplateService(_repository, _logger);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsAllTemplates()
    {
        // Arrange
        var templates = new List<PromptTemplate> { new() { Id = Guid.NewGuid(), DisplayName = "T1" } };
        _repository.GetAllAsync(Arg.Any<CancellationToken>()).Returns(templates);

        // Act
        var result = await _sut.GetAllAsync();

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().HaveCount(1);
    }

    [Fact]
    public async Task CreateAsync_WhenKeyExists_ReturnsConflict409()
    {
        // Arrange
        var request = new CreatePromptTemplateRequest { TemplateKey = PromptTemplateKey.EventDescription };
        _repository.GetByKeyAsync(PromptTemplateKey.EventDescription).Returns(new PromptTemplate());

        // Act
        var result = await _sut.CreateAsync(request);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(409);
    }
}
