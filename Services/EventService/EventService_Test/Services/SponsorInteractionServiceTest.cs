using EventService_Test.Helpers.TestDataBuilders;
using Mapster;

namespace EventService_Test;

public class SponsorInteractionServiceTest
{
    private readonly ISponsorInteractionRepository _repository;
    private readonly ISponsorRepository _sponsorRepository;
    private readonly SponsorInteractionService _sut;

    public SponsorInteractionServiceTest()
    {
        _repository = Substitute.For<ISponsorInteractionRepository>();
        _sponsorRepository = Substitute.For<ISponsorRepository>();
        _sut = new SponsorInteractionService(_repository, _sponsorRepository);

        TypeAdapterConfig<SponsorInteraction, SponsorInteractionDto>.NewConfig();
    }

    // ── CreateAsync ────────────────────────────────────────────────────────

    [Fact]
    public async Task CreateAsync_WithEmptySponsorId_ReturnsFail400()
    {
        // Arrange
        var dto = new CreateSponsorInteractionDto
        {
            SponsorId = Guid.Empty,
            UserId = Guid.NewGuid(),
            InteractionType = InteractionType.Click
        };

        // Act
        var result = await _sut.CreateAsync(dto);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(400);
    }

    [Fact]
    public async Task CreateAsync_WithEmptyUserId_ReturnsFail400()
    {
        // Arrange
        var dto = new CreateSponsorInteractionDto
        {
            SponsorId = Guid.NewGuid(),
            UserId = Guid.Empty,
            InteractionType = InteractionType.Click
        };

        // Act
        var result = await _sut.CreateAsync(dto);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(400);
    }

    [Fact]
    public async Task CreateAsync_WhenSponsorNotFound_ReturnsFail400()
    {
        // Arrange
        var sponsorId = Guid.NewGuid();
        var dto = new CreateSponsorInteractionDto
        {
            SponsorId = sponsorId,
            UserId = Guid.NewGuid(),
            InteractionType = InteractionType.View
        };
        _sponsorRepository.GetByIdAsync(sponsorId).Returns((Sponsor?)null);

        // Act
        var result = await _sut.CreateAsync(dto);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(400);
        result.Message.Should().Contain("Sponsor không tồn tại");
    }

    [Fact]
    public async Task CreateAsync_WithValidDto_ReturnsSuccess201()
    {
        // Arrange
        var sponsorId = Guid.NewGuid();
        var sponsor = SponsorBuilder.CreateEntity(id: sponsorId);
        var dto = new CreateSponsorInteractionDto
        {
            SponsorId = sponsorId,
            UserId = Guid.NewGuid(),
            InteractionType = InteractionType.Click
        };
        _sponsorRepository.GetByIdAsync(sponsorId).Returns(sponsor);

        // Act
        var result = await _sut.CreateAsync(dto);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(201);
        await _repository.Received(1).AddAsync(Arg.Any<SponsorInteraction>());
        await _repository.Received(1).SaveChangesAsync();
    }

    // ── GetByIdAsync ───────────────────────────────────────────────────────

    [Fact]
    public async Task GetByIdAsync_WhenNotFound_ReturnsFail404()
    {
        // Arrange
        _repository.GetByIdAsync(Arg.Any<Guid>()).Returns((SponsorInteraction?)null);

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
        var interaction = new SponsorInteraction
        {
            Id = id,
            SponsorId = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            InteractionType = InteractionType.View,
            InteractionDate = DateTime.UtcNow
        };
        _repository.GetByIdAsync(id).Returns(interaction);

        // Act
        var result = await _sut.GetByIdAsync(id);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(200);
    }

    // ── TrackInteractionAsync ──────────────────────────────────────────────

    [Fact]
    public async Task TrackInteractionAsync_WhenSponsorNotFound_ThrowsKeyNotFoundException()
    {
        // Arrange
        var sponsorId = Guid.NewGuid();
        _sponsorRepository.ExistsAsync(sponsorId).Returns(false);

        // Act
        var act = async () => await _sut.TrackInteractionAsync(sponsorId, Guid.NewGuid(), InteractionType.Click);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task TrackInteractionAsync_WhenSponsorExists_AddsInteraction()
    {
        // Arrange
        var sponsorId = Guid.NewGuid();
        _sponsorRepository.ExistsAsync(sponsorId).Returns(true);

        // Act
        await _sut.TrackInteractionAsync(sponsorId, Guid.NewGuid(), InteractionType.LogoClick);

        // Assert
        await _repository.Received(1).AddAsync(Arg.Is<SponsorInteraction>(
            i => i.SponsorId == sponsorId && i.InteractionType == InteractionType.LogoClick));
        await _repository.Received(1).SaveChangesAsync();
    }

    // ── GetInteractionStatsAsync ───────────────────────────────────────────

    [Fact]
    public async Task GetInteractionStatsAsync_WhenSponsorNotFound_ThrowsKeyNotFoundException()
    {
        // Arrange
        var sponsorId = Guid.NewGuid();
        _sponsorRepository.GetByIdWithInteractionsAsync(sponsorId).Returns((Sponsor?)null);

        // Act
        var act = async () => await _sut.GetInteractionStatsAsync(sponsorId);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task GetInteractionStatsAsync_WithInteractions_ReturnsCorrectCounts()
    {
        // Arrange
        var sponsorId = Guid.NewGuid();
        var sponsor = SponsorBuilder.CreateEntity(id: sponsorId);
        sponsor.SponsorInteractions = new List<SponsorInteraction>
        {
            new() { SponsorId = sponsorId, UserId = Guid.NewGuid(), InteractionType = InteractionType.Click },
            new() { SponsorId = sponsorId, UserId = Guid.NewGuid(), InteractionType = InteractionType.Click },
            new() { SponsorId = sponsorId, UserId = Guid.NewGuid(), InteractionType = InteractionType.View }
        };
        _sponsorRepository.GetByIdWithInteractionsAsync(sponsorId).Returns(sponsor);

        // Act
        var stats = await _sut.GetInteractionStatsAsync(sponsorId);

        // Assert
        stats.TotalInteractions.Should().Be(3);
        stats.TotalClickInteractions.Should().Be(2);
    }
}
