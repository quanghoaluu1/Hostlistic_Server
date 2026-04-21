using AIService_Application.DTOs;
using AIService_Application.Services;
using FluentAssertions;
using NSubstitute;

namespace AIService_Test.Services.AiPlanEntitlementServiceTests;

public class EntitlementTests
{
    private readonly IUserPlanServiceClient _userPlanServiceClient = Substitute.For<IUserPlanServiceClient>();
    private readonly AiPlanEntitlementService _sut;

    public EntitlementTests()
    {
        _sut = new AiPlanEntitlementService(_userPlanServiceClient);
    }

    [Fact]
    public async Task EnsureCanUseAiAsync_WhenNoActivePlan_ReturnsFail403()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _userPlanServiceClient.GetByUserIdAsync(userId, true).Returns(new List<UserPlanDto>());

        // Act
        var result = await _sut.EnsureCanUseAiAsync(userId);

        // Assert
        result.Success.Should().BeFalse();
        result.StatusCode.Should().Be(403);
        result.Message.Should().Contain("No active subscription plan found");
    }

    [Fact]
    public async Task EnsureCanUseAiAsync_WhenPlanNoAiAccess_ReturnsFail403()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var plan = new UserPlanDto
        {
            SubscriptionPlan = new SubscriptionPlanDto { HasAiAccess = false },
            StartDate = DateTime.UtcNow.AddDays(-1)
        };
        _userPlanServiceClient.GetByUserIdAsync(userId, true).Returns(new List<UserPlanDto> { plan });

        // Act
        var result = await _sut.EnsureCanUseAiAsync(userId);

        // Assert
        result.Success.Should().BeFalse();
        result.StatusCode.Should().Be(403);
        result.Message.Should().Contain("does not include AI features");
    }

    [Fact]
    public async Task EnsureCanUseAiAsync_WithValidPlan_ReturnsSuccess200()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var plan = new UserPlanDto
        {
            SubscriptionPlan = new SubscriptionPlanDto { HasAiAccess = true },
            StartDate = DateTime.UtcNow.AddDays(-1)
        };
        _userPlanServiceClient.GetByUserIdAsync(userId, true).Returns(new List<UserPlanDto> { plan });

        // Act
        var result = await _sut.EnsureCanUseAiAsync(userId);

        // Assert
        result.Success.Should().BeTrue();
        result.StatusCode.Should().Be(200);
    }
}
