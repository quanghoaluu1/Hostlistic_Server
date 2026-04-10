using EventService_Test.Helpers.TestDataBuilders;
using Mapster;

namespace EventService_Test;

public class TalentServiceTest
{
    private readonly ITalentRepository _talentRepository;
    private readonly TalentService _sut;

    public TalentServiceTest()
    {
        _talentRepository = Substitute.For<ITalentRepository>();
        _sut = new TalentService(_talentRepository);

        TypeAdapterConfig<Talent, TalentDto>.NewConfig();
    }

    // ── GetTalentByIdAsync ─────────────────────────────────────────────────

    [Fact]
    public async Task GetTalentByIdAsync_WhenTalentNotFound_ReturnsFail404()
    {
        // Arrange
        _talentRepository.GetTalentByIdAsync(Arg.Any<Guid>()).Returns((Talent)null!);

        // Act
        var result = await _sut.GetTalentByIdAsync(Guid.NewGuid());

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(404);
        result.Message.Should().Contain("Talent not found");
    }

    [Fact]
    public async Task GetTalentByIdAsync_WhenTalentExists_ReturnsSuccess200()
    {
        // Arrange
        var talentId = Guid.NewGuid();
        var talent = TalentBuilder.CreateEntity(id: talentId, name: "Speaker One");
        _talentRepository.GetTalentByIdAsync(talentId).Returns(talent);

        // Act
        var result = await _sut.GetTalentByIdAsync(talentId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(200);
        result.Data!.Name.Should().Be("Speaker One");
    }

    // ── CreateTalentAsync ──────────────────────────────────────────────────

    [Fact]
    public async Task CreateTalentAsync_WithEmptyName_ReturnsFail400()
    {
        // Arrange
        var request = TalentBuilder.CreateRequest(name: "   ");

        // Act
        var result = await _sut.CreateTalentAsync(request);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(400);
        result.Message.Should().Contain("Talent name is required");
    }

    [Fact]
    public async Task CreateTalentAsync_WithValidRequest_ReturnsSuccess201()
    {
        // Arrange
        var request = TalentBuilder.CreateRequest(name: "New Speaker");

        // Act
        var result = await _sut.CreateTalentAsync(request);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(201);
        result.Message.Should().Contain("Talent created successfully");
        await _talentRepository.Received(1).AddTalentAsync(Arg.Any<Talent>());
        await _talentRepository.Received(1).SaveChangesAsync();
    }

    [Fact]
    public async Task CreateTalentAsync_WithNoAvatarUrl_AssignsDefaultAvatar()
    {
        // Arrange
        var request = new CreateTalentDto { Name = "Speaker", AvatarUrl = null, Email = "a@b.com" };

        // Act
        var result = await _sut.CreateTalentAsync(request);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data!.AvatarUrl.Should().NotBeNullOrEmpty();
    }

    // ── UpdateTalentAsync ──────────────────────────────────────────────────

    [Fact]
    public async Task UpdateTalentAsync_WhenTalentNotFound_ReturnsFail404()
    {
        // Arrange
        _talentRepository.GetTalentByIdAsync(Arg.Any<Guid>()).Returns((Talent)null!);

        // Act
        var result = await _sut.UpdateTalentAsync(Guid.NewGuid(), TalentBuilder.UpdateRequest());

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task UpdateTalentAsync_WithValidRequest_ReturnsSuccess200()
    {
        // Arrange
        var talentId = Guid.NewGuid();
        var existing = TalentBuilder.CreateEntity(id: talentId, name: "Old Name");
        _talentRepository.GetTalentByIdAsync(talentId).Returns(existing);

        var request = TalentBuilder.UpdateRequest(name: "New Name");

        // Act
        var result = await _sut.UpdateTalentAsync(talentId, request);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(200);
        existing.Name.Should().Be("New Name");
        await _talentRepository.Received(1).UpdateTalentAsync(Arg.Any<Talent>());
    }

    // ── DeleteTalentAsync ──────────────────────────────────────────────────

    [Fact]
    public async Task DeleteTalentAsync_WhenTalentNotFound_ReturnsFail404()
    {
        // Arrange
        _talentRepository.GetTalentByIdAsync(Arg.Any<Guid>()).Returns((Talent)null!);

        // Act
        var result = await _sut.DeleteTalentAsync(Guid.NewGuid());

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task DeleteTalentAsync_WhenTalentHasLineups_ReturnsFail400()
    {
        // Arrange
        var talentId = Guid.NewGuid();
        var talent = TalentBuilder.CreateEntity(id: talentId, hasLineups: true);
        _talentRepository.GetTalentByIdAsync(talentId).Returns(talent);

        // Act
        var result = await _sut.DeleteTalentAsync(talentId);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(400);
        result.Message.Should().Contain("Talent has Lineups");
    }

    [Fact]
    public async Task DeleteTalentAsync_WhenDeleteFails_ReturnsFail500()
    {
        // Arrange
        var talentId = Guid.NewGuid();
        var talent = TalentBuilder.CreateEntity(id: talentId);
        _talentRepository.GetTalentByIdAsync(talentId).Returns(talent);
        _talentRepository.DeleteTalentAsync(talentId).Returns(false);

        // Act
        var result = await _sut.DeleteTalentAsync(talentId);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(500);
    }

    [Fact]
    public async Task DeleteTalentAsync_WithNoLineups_ReturnsSuccess200()
    {
        // Arrange
        var talentId = Guid.NewGuid();
        var talent = TalentBuilder.CreateEntity(id: talentId, hasLineups: false);
        _talentRepository.GetTalentByIdAsync(talentId).Returns(talent);
        _talentRepository.DeleteTalentAsync(talentId).Returns(true);

        // Act
        var result = await _sut.DeleteTalentAsync(talentId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(200);
    }
}
