using Common;
using IdentityService_Application.Interfaces;
using IdentityService_Application.Services;
using Microsoft.AspNetCore.Http;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace IdentityService_Test.Services
{
    public class UserTicketServiceTest
    {
        private readonly IBookingServiceClient _bookingClient;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly UserTicketService _service;

        public UserTicketServiceTest()
        {
            _bookingClient = Substitute.For<IBookingServiceClient>();
            _httpContextAccessor = Substitute.For<IHttpContextAccessor>();

            var context = new DefaultHttpContext();
            context.Request.Headers["Authorization"] = "Bearer test-token";

            _httpContextAccessor.HttpContext.Returns(context);

            _service = new UserTicketService(_bookingClient, _httpContextAccessor);
        }

        #region GetUserOrdersAsync Tests
        [Fact]
        public async Task GetUserOrdersAsync_ShouldReturnSuccess_WhenClientReturnsSuccess()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var expected = ApiResponse<object>.Success(200, "OK", new { data = "test" });

            _bookingClient
                .GetUserOrdersAsync(userId, "Bearer test-token")
                .Returns(expected);

            // Act
            var result = await _service.GetUserOrdersAsync(userId);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.IsSuccess);
            Assert.Equal(200, result.StatusCode);
            Assert.Equal(expected.Data, result.Data);

            await _bookingClient.Received(1)
                .GetUserOrdersAsync(userId, "Bearer test-token");
        }

        [Fact]
        public async Task GetUserOrdersAsync_ShouldReturn500_WhenExceptionThrown()
        {
            // Arrange
            var userId = Guid.NewGuid();

            _bookingClient
                .GetUserOrdersAsync(userId, Arg.Any<string>())
                .Throws(new Exception("boom"));

            // Act
            var result = await _service.GetUserOrdersAsync(userId);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.IsSuccess);
            Assert.Equal(500, result.StatusCode);
            Assert.Contains("Error retrieving user orders", result.Message);
        }
        #endregion

        #region GetUserTicketsAsync Tests
        [Fact]
        public async Task GetUserTicketsAsync_ShouldReturnFail_WhenOrdersFail()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var failResponse = ApiResponse<object>.Fail(400, "fail");

            _bookingClient
                .GetUserOrdersAsync(userId, Arg.Any<string>())
                .Returns(failResponse);

            // Act
            var result = await _service.GetUserTicketsAsync(userId);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(400, result.StatusCode);
            Assert.Equal("fail", result.Message);
        }

        [Fact]
        public async Task GetUserTicketsAsync_ShouldReturnSuccess_WhenOrdersSuccess()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var data = new { tickets = 123 };

            var ordersResponse = ApiResponse<object>.Success(200, "OK", data);

            _bookingClient
                .GetUserOrdersAsync(userId, Arg.Any<string>())
                .Returns(ordersResponse);

            // Act
            var result = await _service.GetUserTicketsAsync(userId);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(200, result.StatusCode);
            Assert.Equal("User tickets retrieved successfully", result.Message);
            Assert.Equal(data, result.Data);
        }
        #endregion

        #region GetUserTicketsWithEventDetailsAsync Tests
        [Fact]
        public async Task GetUserTicketsWithEventDetailsAsync_ShouldReturnSuccess_WhenOrdersSuccess()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var data = new { tickets = 456 };

            var ordersResponse = ApiResponse<object>.Success(200, "OK", data);

            _bookingClient
                .GetUserOrdersAsync(userId, Arg.Any<string>())
                .Returns(ordersResponse);

            // Act
            var result = await _service.GetUserTicketsWithEventDetailsAsync(userId);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(200, result.StatusCode);
            Assert.Equal("User tickets with event details retrieved successfully", result.Message);
            Assert.Equal(data, result.Data);
        }

        [Fact]
        public async Task GetUserOrdersAsync_ShouldPassNullHeader_WhenNoAuthHeader()
        {
            // Arrange
            var userId = Guid.NewGuid();

            var context = new DefaultHttpContext();
            context.Request.Headers.Clear();

            _httpContextAccessor.HttpContext.Returns(context);

            _bookingClient
                .GetUserOrdersAsync(userId, Arg.Any<string>())
                .Returns(ApiResponse<object>.Success(200, "OK", null));

            // Act
            var result = await _service.GetUserOrdersAsync(userId);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.IsSuccess);

            await _bookingClient.Received(1)
                .GetUserOrdersAsync(userId, Arg.Any<string>());
        }
        #endregion
    }
}
