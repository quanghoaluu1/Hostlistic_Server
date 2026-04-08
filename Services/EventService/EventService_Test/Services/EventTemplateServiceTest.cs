using Mapster;

namespace EventService_Test;

public class EventTemplateServiceTest
{
    private readonly IEventTemplateRepository _repository;
    private readonly EventTemplateService _sut;

    public EventTemplateServiceTest()
    {
        _repository = Substitute.For<IEventTemplateRepository>();
        _sut = new EventTemplateService(_repository);

        TypeAdapterConfig<EventTemplate, EventTemplateDto>.NewConfig();
        TypeAdapterConfig<EventTemplateConfigDto, EventTemplateConfig>.NewConfig();
        TypeAdapterConfig<EventTemplateConfig, EventTemplateConfigDto>.NewConfig();
    }

    private static CreateEventTemplateDto ValidCreateDto(Guid? createdBy = null) => new CreateEventTemplateDto
    {
        CreatedBy = createdBy ?? Guid.NewGuid(),
        Name = "Tech Conference Template",
        Config = new EventTemplateConfigDto
        {
            ThemeColor = "#3B82F6",
            CoverImageUrl = null,
            DefaultTickets = new List<TemplateTicketDto>
            {
                new TemplateTicketDto { Name = "General", Price = 0 }
            }
        }
    };

    // ── CreateAsync ────────────────────────────────────────────────────────

    [Fact]
    public async Task CreateAsync_WithEmptyCreatedBy_ReturnsFail400()
    {
        // Arrange
        var dto = ValidCreateDto(createdBy: Guid.Empty);

        // Act
        var result = await _sut.CreateAsync(dto);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(400);
    }

    [Fact]
    public async Task CreateAsync_WithEmptyName_ReturnsFail400()
    {
        // Arrange
        var dto = ValidCreateDto();
        dto.Name = "   ";

        // Act
        var result = await _sut.CreateAsync(dto);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(400);
    }

    [Fact]
    public async Task CreateAsync_WithValidDto_ReturnsSuccess201()
    {
        // Arrange
        var dto = ValidCreateDto();

        // Act
        var result = await _sut.CreateAsync(dto);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(201);
        result.Data!.Name.Should().Be("Tech Conference Template");
        await _repository.Received(1).AddAsync(Arg.Any<EventTemplate>());
        await _repository.Received(1).SaveChangesAsync();
    }

    // ── GetByIdAsync ───────────────────────────────────────────────────────

    [Fact]
    public async Task GetByIdAsync_WhenNotFound_ReturnsFail404()
    {
        // Arrange
        _repository.GetByIdAsync(Arg.Any<Guid>()).Returns((EventTemplate?)null);

        // Act
        var result = await _sut.GetByIdAsync(Guid.NewGuid());

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task GetByIdAsync_WhenExists_ReturnsSuccess200()
    {
        // Arrange
        var id = Guid.NewGuid();
        var template = new EventTemplate
        {
            Id = id,
            CreatedBy = Guid.NewGuid(),
            Name = "My Template",
            Config = new EventTemplateConfig { ThemeColor = "#FF0000" }
        };
        _repository.GetByIdAsync(id).Returns(template);

        // Act
        var result = await _sut.GetByIdAsync(id);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(200);
        result.Data!.Name.Should().Be("My Template");
    }

    // ── GetByCreatorAsync ──────────────────────────────────────────────────

    [Fact]
    public async Task GetByCreatorAsync_ReturnsSuccess200WithTemplates()
    {
        // Arrange
        var creatorId = Guid.NewGuid();
        var templates = new List<EventTemplate>
        {
            new EventTemplate { Id = Guid.NewGuid(), CreatedBy = creatorId, Name = "Template A", Config = new EventTemplateConfig() },
            new EventTemplate { Id = Guid.NewGuid(), CreatedBy = creatorId, Name = "Template B", Config = new EventTemplateConfig() }
        };
        _repository.GetByCreatorAsync(creatorId).Returns(templates.AsReadOnly());

        // Act
        var result = await _sut.GetByCreatorAsync(creatorId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(200);
        result.Data.Should().HaveCount(2);
    }

    // ── UpdateAsync ────────────────────────────────────────────────────────

    [Fact]
    public async Task UpdateAsync_WhenNotFound_ReturnsFail404()
    {
        // Arrange
        _repository.GetByIdAsync(Arg.Any<Guid>()).Returns((EventTemplate?)null);

        // Act
        var result = await _sut.UpdateAsync(Guid.NewGuid(),
            new UpdateEventTemplateDto { Name = "New Name", Config = null });

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task UpdateAsync_UpdatesNameWhenProvided()
    {
        // Arrange
        var id = Guid.NewGuid();
        var template = new EventTemplate
        {
            Id = id,
            CreatedBy = Guid.NewGuid(),
            Name = "Old Name",
            Config = new EventTemplateConfig()
        };
        _repository.GetByIdAsync(id).Returns(template);

        // Act
        var result = await _sut.UpdateAsync(id, new UpdateEventTemplateDto { Name = "New Name", Config = null });

        // Assert
        result.IsSuccess.Should().BeTrue();
        template.Name.Should().Be("New Name");
        await _repository.Received(1).UpdateAsync(Arg.Any<EventTemplate>());
    }

    [Fact]
    public async Task UpdateAsync_WithNullName_PreservesExistingName()
    {
        // Arrange
        var id = Guid.NewGuid();
        var template = new EventTemplate
        {
            Id = id,
            CreatedBy = Guid.NewGuid(),
            Name = "Original Name",
            Config = new EventTemplateConfig()
        };
        _repository.GetByIdAsync(id).Returns(template);

        // Act
        var result = await _sut.UpdateAsync(id, new UpdateEventTemplateDto { Name = null, Config = null });

        // Assert
        result.IsSuccess.Should().BeTrue();
        template.Name.Should().Be("Original Name");
    }

    // ── DeleteAsync ────────────────────────────────────────────────────────

    [Fact]
    public async Task DeleteAsync_WhenNotFound_ReturnsFail404()
    {
        // Arrange
        _repository.DeleteAsync(Arg.Any<Guid>()).Returns(false);

        // Act
        var result = await _sut.DeleteAsync(Guid.NewGuid());

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task DeleteAsync_WhenExists_ReturnsSuccess200()
    {
        // Arrange
        var id = Guid.NewGuid();
        _repository.DeleteAsync(id).Returns(true);

        // Act
        var result = await _sut.DeleteAsync(id);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(200);
        await _repository.Received(1).SaveChangesAsync();
    }
}
