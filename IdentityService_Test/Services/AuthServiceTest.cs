using Common;
using IdentityService_Application.DTOs;
using IdentityService_Application.Interfaces;
using IdentityService_Application.Services;
using IdentityService_Domain.Entities;
using IdentityService_Domain.Interfaces;
using Microsoft.Extensions.Configuration;
using NotificationService_Application.Interfaces;
using NSubstitute;

namespace IdentityService_Test.Services
{
    public class AuthServiceTest
    {
        private readonly IUserRepository _userRepository;
        private readonly IRefreshTokenRepository _refreshTokenRepository;
        private readonly IConfiguration _configuration;
        private readonly IOtpService _otpService;
        private readonly INotificationServiceClient _notificationServiceClient;
        private readonly IBookingServiceClient _bookingServiceClient;
        private readonly IUserPlanRepository _userPlanRepository;

        private readonly AuthService _authService;

        public AuthServiceTest()
        {
            _userRepository = Substitute.For<IUserRepository>();
            _refreshTokenRepository = Substitute.For<IRefreshTokenRepository>();
            _configuration = Substitute.For<IConfiguration>();
            _otpService = Substitute.For<IOtpService>();
            _notificationServiceClient = Substitute.For<INotificationServiceClient>();
            _bookingServiceClient = Substitute.For<IBookingServiceClient>();
            _userPlanRepository = Substitute.For<IUserPlanRepository>();

            _configuration["Jwt:Key"].Returns("7PHM4w1cQx+KRuCFVhc3MF4cqgmgZtYPxslIcSf+06w=");
            _configuration["Jwt:Issuer"].Returns("hostlistic");
            _configuration["Jwt:Audience"].Returns("hostlistic");


            _authService = new AuthService(_userRepository, _refreshTokenRepository, _configuration, _otpService, _notificationServiceClient, _bookingServiceClient, _userPlanRepository);
        }

        #region RegisterAsync
        [Fact]
        public async Task RegisterAsync_EmailExists_ReturnFail()
        {
            // Arrange
            _userRepository
                .IsExistByEmailAsync(Arg.Any<string>())
                .Returns(true);

            var request = new RegisterRequest
            {
                Email = "test@gmail.com",
                Password = "123456",
                FullName = "Test User"
            };

            // Act
            var result = await _authService.RegisterAsync(request);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(400, result.StatusCode);

            // Verify không tạo user
            await _userRepository.DidNotReceive()
                .AddUserAsync(Arg.Any<User>());

            await _bookingServiceClient.DidNotReceive()
                .CreateWalletAsync(Arg.Any<Guid>());

            await _userPlanRepository.DidNotReceive()
                .AddAsync(Arg.Any<UserPlan>());
        }

        [Fact]
        public async Task RegisterAsync_ValidRequest_ReturnSuccess()
        {
            // Arrange
            _userRepository
                .IsExistByEmailAsync(Arg.Any<string>())
                .Returns(false);

            _userRepository
                .AddUserAsync(Arg.Any<User>())
                .Returns(Task.CompletedTask);

            _userRepository
                .SaveChangesAsync()
                .Returns(Task.CompletedTask);

            var walletResponse = ApiResponse<object>.Success(
                201,
                "Wallet created successfully",
                null
            );

            _bookingServiceClient
                .CreateWalletAsync(Arg.Any<Guid>())
                .Returns(Task.FromResult(walletResponse));

            _userPlanRepository
                .AddAsync(Arg.Any<UserPlan>())
                .Returns(Task.CompletedTask);

            _userPlanRepository
                .SaveChangesAsync()
                .Returns(Task.CompletedTask);

            var request = new RegisterRequest
            {
                Email = "test@gmail.com",
                Password = "123456",
                FullName = "Test User"
            };

            // Act
            var result = await _authService.RegisterAsync(request);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(201, result.StatusCode);

            // Verify tạo user + check hash password
            await _userRepository.Received(1)
                .AddUserAsync(Arg.Is<User>(u =>
                    u.Email == request.Email &&
                    u.FullName == request.FullName &&
                    BCrypt.Net.BCrypt.Verify(request.Password, u.HashedPassword)
                ));

            await _userRepository.Received(1).SaveChangesAsync();

            // Verify tạo wallet
            await _bookingServiceClient.Received(1)
                .CreateWalletAsync(Arg.Any<Guid>());

            // Verify tạo user plan
            await _userPlanRepository.Received(1)
                .AddAsync(Arg.Is<UserPlan>(p =>
                    p.SubscriptionPlanId != Guid.Empty &&
                    p.IsActive == true
                ));

            await _userPlanRepository.Received(1).SaveChangesAsync();
        }
        #endregion

        #region LoginAsync
        [Fact]
        public async Task LoginAsync_UserNotFound_ReturnUnauthorized()
        {
            // Arrange
            _userRepository
                .GetUserByEmailAsync(Arg.Any<string>())
                .Returns((User?)null);

            var request = new LoginRequest
            {
                Email = "test@gmail.com",
                Password = "123456"
            };

            // Act
            var result = await _authService.LoginAsync(request);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(401, result.StatusCode);

            await _refreshTokenRepository.DidNotReceive()
                .AddTokenAsync(Arg.Any<RefreshToken>());
        }

        [Fact]
        public async Task LoginAsync_WrongPassword_ReturnUnauthorized()
        {
            // Arrange
            var user = new User
            {
                Email = "test@gmail.com",
                HashedPassword = BCrypt.Net.BCrypt.HashPassword("correct_password"),
                IsActive = true
            };

            _userRepository
                .GetUserByEmailAsync(Arg.Any<string>())
                .Returns(user);

            var request = new LoginRequest
            {
                Email = "test@gmail.com",
                Password = "wrong_password"
            };

            // Act
            var result = await _authService.LoginAsync(request);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(401, result.StatusCode);

            await _refreshTokenRepository.DidNotReceive()
                .AddTokenAsync(Arg.Any<RefreshToken>());
        }

        [Fact]
        public async Task LoginAsync_UserDeactivated_ReturnUnauthorized()
        {
            // Arrange
            var user = new User
            {
                Email = "test@gmail.com",
                HashedPassword = BCrypt.Net.BCrypt.HashPassword("123456"),
                IsActive = false
            };

            _userRepository
                .GetUserByEmailAsync(Arg.Any<string>())
                .Returns(user);

            var request = new LoginRequest
            {
                Email = "test@gmail.com",
                Password = "123456"
            };

            // Act
            var result = await _authService.LoginAsync(request);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(401, result.StatusCode);

            await _refreshTokenRepository.DidNotReceive()
                .AddTokenAsync(Arg.Any<RefreshToken>());
        }

        [Fact]
        public async Task LoginAsync_ValidCredentials_ReturnSuccess()
        {
            // Arrange
            var user = new User
            {
                Id = Guid.NewGuid(),
                Email = "test@gmail.com",
                FullName = "Test User",
                HashedPassword = BCrypt.Net.BCrypt.HashPassword("123456"),
                IsActive = true
            };

            _userRepository
                .GetUserByEmailAsync(Arg.Any<string>())
                .Returns(user);

            _refreshTokenRepository
                .AddTokenAsync(Arg.Any<RefreshToken>())
                .Returns(Task.CompletedTask);

            _refreshTokenRepository
                .SaveChangesAsync()
                .Returns(Task.CompletedTask);

            var request = new LoginRequest
            {
                Email = "test@gmail.com",
                Password = "123456"
            };

            // Act
            var result = await _authService.LoginAsync(request);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(200, result.StatusCode);

            Assert.NotNull(result.Data);
            Assert.NotNull(result.Data.AccessToken);
            Assert.NotNull(result.Data.User);
            Assert.Equal(user.Email, result.Data.User.Email);

            // Verify refresh token được lưu
            await _refreshTokenRepository.Received(1)
                .AddTokenAsync(Arg.Any<RefreshToken>());

            await _refreshTokenRepository.Received(1)
                .SaveChangesAsync();
        }
        #endregion

        #region RequestPasswordResetAsync
        [Fact]
        public async Task RequestPasswordResetAsync_UserNotFound_ReturnFail()
        {
            // Arrange
            _userRepository
                .GetUserByEmailAsync(Arg.Any<string>())
                .Returns((User?)null);

            var email = "test@gmail.com";

            // Act
            var result = await _authService.RequestPasswordResetAsync(email);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(404, result.StatusCode);

            // Verify không gọi OTP và email
            await _otpService.DidNotReceive()
                .GenerateOtpAsync(Arg.Any<string>());

            await _notificationServiceClient.DidNotReceive()
                .SendOtpEmailAsync(Arg.Any<string>(), Arg.Any<string>());
        }

        [Fact]
        public async Task RequestPasswordResetAsync_ValidEmail_ReturnSuccess()
        {
            // Arrange
            var user = new User
            {
                Email = "test@gmail.com"
            };

            var otp = "123456";

            _userRepository
                .GetUserByEmailAsync(Arg.Any<string>())
                .Returns(user);

            _otpService
                .GenerateOtpAsync(Arg.Any<string>())
                .Returns(otp);

            _notificationServiceClient
                .SendOtpEmailAsync(Arg.Any<string>(), Arg.Any<string>())
                .Returns(Task.CompletedTask);

            var email = "test@gmail.com";

            // Act
            var result = await _authService.RequestPasswordResetAsync(email);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(200, result.StatusCode);

            // Verify generate OTP
            await _otpService.Received(1)
                .GenerateOtpAsync(email);

            // Verify gửi email đúng OTP
            await _notificationServiceClient.Received(1)
                .SendOtpEmailAsync(email, otp);
        }
        #endregion

        #region ResetPasswordAsync
        [Fact]
        public async Task ResetPasswordAsync_InvalidOtp_ReturnFail()
        {
            // Arrange
            _otpService
                .VerifyOtpAsync(Arg.Any<string>(), Arg.Any<string>())
                .Returns(false);

            var email = "test@gmail.com";
            var otp = "123456";
            var newPassword = "newpass";

            // Act
            var result = await _authService.ResetPasswordAsync(email, otp, newPassword);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(400, result.StatusCode);

            // Verify không update password
            await _userRepository.DidNotReceive()
                .SaveChangesAsync();
        }

        [Fact]
        public async Task ResetPasswordAsync_ValidOtp_ReturnSuccess()
        {
            // Arrange
            var user = new User
            {
                Email = "test@gmail.com",
                HashedPassword = BCrypt.Net.BCrypt.HashPassword("oldpass")
            };

            _otpService
                .VerifyOtpAsync(Arg.Any<string>(), Arg.Any<string>())
                .Returns(true);

            _userRepository
                .GetUserByEmailAsync(Arg.Any<string>())
                .Returns(user);

            _userRepository
                .SaveChangesAsync()
                .Returns(Task.CompletedTask);

            var email = "test@gmail.com";
            var otp = "123456";
            var newPassword = "newpass";

            // Act
            var result = await _authService.ResetPasswordAsync(email, otp, newPassword);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(200, result.StatusCode);

            // Verify password đã được hash lại
            Assert.True(BCrypt.Net.BCrypt.Verify(newPassword, user.HashedPassword));

            await _userRepository.Received(1)
                .SaveChangesAsync();
        }
        #endregion

        #region GoogleLoginAsync
        [Fact]
        public async Task GoogleLoginAsync_MissingClientId_ShouldThrowException()
        {
            // Arrange
            var request = new GoogleLoginRequest
            {
                IdToken = "any_token"
            };

            _configuration["Google:ClientId"].Returns((string?)null);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _authService.GoogleLoginAsync(request)
            );
        }

        #endregion

        #region RefreshTokenAsync
        [Fact]
        public async Task RefreshTokenAsync_ValidToken_ShouldReturnNewTokens()
        {
            // Arrange
            var oldToken = "old_token";

            var user = new User
            {
                Id = Guid.NewGuid(),
                Email = "test@gmail.com",
                IsActive = true
            };

            var existingToken = new RefreshToken
            {
                Token = oldToken,
                User = user,
                IsRevoked = false,
                ExpiryDate = DateTime.UtcNow.AddMinutes(10)
            };

            _refreshTokenRepository.GetTokenAsync(oldToken)
                .Returns(existingToken);

            // Act
            var result = await _authService.RefreshTokenAsync(oldToken);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Data);
            Assert.NotNull(result.Data.AccessToken);

            await _refreshTokenRepository.Received(1)
                .RevokeRefreshTokenAsync(existingToken);

            await _refreshTokenRepository.Received(1)
                .AddTokenAsync(Arg.Any<RefreshToken>());
        }

        [Fact]
        public async Task RefreshTokenAsync_TokenNotFound_ShouldFail()
        {
            var oldToken = "invalid";

            _refreshTokenRepository.GetTokenAsync(oldToken)
                .Returns((RefreshToken?)null);

            var result = await _authService.RefreshTokenAsync(oldToken);

            Assert.False(result.IsSuccess);
            Assert.Equal(401, result.StatusCode);
        }

        [Fact]
        public async Task RefreshTokenAsync_TokenRevoked_ShouldFail()
        {
            var token = new RefreshToken
            {
                Token = "TokenRevoked",
                IsRevoked = true,
                ExpiryDate = DateTime.UtcNow.AddMinutes(10)
            };

            _refreshTokenRepository.GetTokenAsync(Arg.Any<string>())
                .Returns(token);

            var result = await _authService.RefreshTokenAsync("token");

            Assert.False(result.IsSuccess);
        }

        [Fact]
        public async Task RefreshTokenAsync_TokenExpired_ShouldFail()
        {
            var token = new RefreshToken
            {
                Token = "TokenExpired",
                IsRevoked = false,
                ExpiryDate = DateTime.UtcNow.AddMinutes(-1)
            };

            _refreshTokenRepository.GetTokenAsync(Arg.Any<string>())
                .Returns(token);

            var result = await _authService.RefreshTokenAsync("token");

            Assert.False(result.IsSuccess);
        }

        [Fact]
        public async Task RefreshTokenAsync_UserNull_ShouldFail()
        {
            var token = new RefreshToken
            {
                User = null,
                Token = "TokenUserNull",
                IsRevoked = false,
                ExpiryDate = DateTime.UtcNow.AddMinutes(10)
            };

            _refreshTokenRepository.GetTokenAsync(Arg.Any<string>())
                .Returns(token);

            var result = await _authService.RefreshTokenAsync("token");

            Assert.False(result.IsSuccess);
        }

        [Fact]
        public async Task RefreshTokenAsync_UserDeactivated_ShouldFail()
        {
            var user = new User
            {
                IsActive = false
            };

            var token = new RefreshToken
            {
                User = user,
                Token = "TokenUserDeactivated",
                IsRevoked = false,
                ExpiryDate = DateTime.UtcNow.AddMinutes(10)
            };

            _refreshTokenRepository.GetTokenAsync(Arg.Any<string>())
                .Returns(token);

            var result = await _authService.RefreshTokenAsync("token");

            Assert.False(result.IsSuccess);
        }
        #endregion

        #region LogoutAsync
        [Fact]
        public async Task LogoutAsync_ValidToken_ShouldRevokeAndReturnSuccess()
        {
            // Arrange
            var refreshToken = "valid_token";

            var existingToken = new RefreshToken
            {
                Token = refreshToken,
                IsRevoked = false
            };

            _refreshTokenRepository.GetTokenAsync(refreshToken)
                .Returns(existingToken);

            // Act
            var result = await _authService.LogoutAsync(refreshToken);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.True(result.Data);
            Assert.Equal(200, result.StatusCode);

            await _refreshTokenRepository.Received(1)
                .RevokeRefreshTokenAsync(existingToken);

            await _refreshTokenRepository.Received(1)
                .SaveChangesAsync();
        }

        [Fact]
        public async Task LogoutAsync_TokenNotFound_ShouldFail()
        {
            // Arrange
            var refreshToken = "invalid_token";

            _refreshTokenRepository.GetTokenAsync(refreshToken)
                .Returns((RefreshToken?)null);

            // Act
            var result = await _authService.LogoutAsync(refreshToken);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(400, result.StatusCode);

            await _refreshTokenRepository.DidNotReceive()
                .RevokeRefreshTokenAsync(Arg.Any<RefreshToken>());
        }

        [Fact]
        public async Task LogoutAsync_TokenAlreadyRevoked_ShouldFail()
        {
            // Arrange
            var refreshToken = "revoked_token";

            var existingToken = new RefreshToken
            {
                Token = refreshToken,
                IsRevoked = true
            };

            _refreshTokenRepository.GetTokenAsync(refreshToken)
                .Returns(existingToken);

            // Act
            var result = await _authService.LogoutAsync(refreshToken);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(400, result.StatusCode);

            await _refreshTokenRepository.DidNotReceive()
                .RevokeRefreshTokenAsync(Arg.Any<RefreshToken>());
        }
        #endregion
    }


}
