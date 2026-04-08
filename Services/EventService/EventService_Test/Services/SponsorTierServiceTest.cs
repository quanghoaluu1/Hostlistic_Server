using Mapster;

namespace EventService_Test;

public class SponsorTierServiceTest
{
    private readonly ISponsorTierRepository _repository;
    private readonly SponsorTierService _sut;

    public SponsorTierServiceTest()
    {
        _repository = Substitute.For<ISponsorTierRepository>();
        _sut = new SponsorTierService(_repository);

        TypeAdapterConfig<SponsorTier, SponsorTierDto>.NewConfig();
    }

    // ── CreateAsync ────────────────────────────────────────────────────────

    [Fact]
    public async Task CreateAsync_WithEmptyName_ReturnsFail400()
    {
        // Arrange
        var dto = new CreateSponsorTierDto { Name = "   ", Priority = 1 };

        // Act
        var result = await _sut.CreateAsync(dto);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(400);
    }

    [Fact]
    public async Task CreateAsync_WithValidName_ReturnsSuccess201()
    {
        // Arrange
        var dto = new CreateSponsorTierDto { Name = "Platinum", Priority = 1 };

        // Act
        var result = await _sut.CreateAsync(dto);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(201);
        result.Data!.Name.Should().Be("Platinum");
        await _repository.Received(1).AddAsync(Arg.Any<SponsorTier>());
        await _repository.Received(1).SaveChangesAsync();
    }

    // ── GetByIdAsync ───────────────────────────────────────────────────────

    [Fact]
    public async Task GetByIdAsync_WhenNotFound_ReturnsFail404()
    {
        // Arrange
        _repository.GetByIdAsync(Arg.Any<Guid>()).Returns((SponsorTier?)null);

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
        var tier = new SponsorTier { Id = id, Name = "Gold", Priority = 2 };
        _repository.GetByIdAsync(id).Returns(tier);

        // Act
        var result = await _sut.GetByIdAsync(id);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(200);
        result.Data!.Name.Should().Be("Gold");
    }

    // ── UpdateAsync ────────────────────────────────────────────────────────

    [Fact]
    public async Task UpdateAsync_WhenNotFound_ReturnsFail404()
    {
        // Arrange
        _repository.GetByIdAsync(Arg.Any<Guid>()).Returns((SponsorTier?)null);

        // Act
        var result = await _sut.UpdateAsync(Guid.NewGuid(), new UpdateSponsorTierDto { Name = "Silver" });

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task UpdateAsync_UpdatesNameAndPriority()
    {
        // Arrange
        var id = Guid.NewGuid();
        var tier = new SponsorTier { Id = id, Name = "Bronze", Priority = 3 };
        _repository.GetByIdAsync(id).Returns(tier);

        var dto = new UpdateSponsorTierDto { Name = "Silver", Priority = 2 };

        // Act
        var result = await _sut.UpdateAsync(id, dto);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(200);
        tier.Name.Should().Be("Silver");
        tier.Priority.Should().Be(2);
        await _repository.Received(1).UpdateAsync(Arg.Any<SponsorTier>());
    }

    [Fact]
    public async Task UpdateAsync_WithNullName_PreservesExistingName()
    {
        // Arrange
        var id = Guid.NewGuid();
        var tier = new SponsorTier { Id = id, Name = "Gold", Priority = 1 };
        _repository.GetByIdAsync(id).Returns(tier);

        var dto = new UpdateSponsorTierDto { Name = null, Priority = 5 };

        // Act
        var result = await _sut.UpdateAsync(id, dto);

        // Assert
        result.IsSuccess.Should().BeTrue();
        tier.Name.Should().Be("Gold"); // unchanged
        tier.Priority.Should().Be(5);  // updated
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
