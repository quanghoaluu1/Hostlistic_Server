using CloudinaryDotNet.Actions;
using Common;
using IdentityService_Application.DTOs;
using IdentityService_Application.Interfaces;
using IdentityService_Application.Services;
using IdentityService_Domain.Entities;
using IdentityService_Domain.Interfaces;
using Microsoft.AspNetCore.Http;
using NSubstitute;

namespace IdentityService_Test.Services
{
    public class UserServiceTest
    {
        private readonly IUserRepository _userRepository;
        private readonly IPhotoService _photoService;

        private readonly UserService _userService;

        public UserServiceTest()
        {
            _userRepository = Substitute.For<IUserRepository>();
            _photoService = Substitute.For<IPhotoService>();
            _userService = new UserService(_userRepository, _photoService, new HttpClient(), new HttpContextAccessor());
        }


        #region GetUserProfileAsync Tests
        [Fact]
        public async Task GetUserProfileAsync_ReturnsUserProfile_WhenUserExists()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var user = new User
            {
                Id = userId,
                FullName = "John Doe",
                Email = "unit@test.com",
                Role = IdentityService_Domain.Enums.Role.Member,
            };

            _userRepository
                .GetUserByIdAsync(userId)
                .Returns(user);

            // Act
            var result = await _userService.GetUserProfileAsync(userId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(userId, result.Data.Id);
            Assert.Equal("John Doe", result.Data.FullName);
            Assert.Equal("unit@test.com", result.Data.Email);
        }
        [Fact]
        public async Task GetUserProfileAsync_ThrowsException_WhenUserNotFound()
        {
            // Arrange
            var userId = Guid.NewGuid();

            _userRepository
                .GetUserByIdAsync(userId)
                .Returns((User)null);

            // Act
            var result = await _userService.GetUserProfileAsync(userId);

            // Act & Assert
            Assert.NotNull(result);
            Assert.False(result.IsSuccess);
            Assert.Equal(404, result.StatusCode);
            Assert.Equal("User not found", result.Message);
            Assert.Null(result.Data); ;
        }
        #endregion

        #region UpdateUserProfileAsync Tests
        [Fact]
        public async Task UpdateUserProfileAsync_ReturnsFail_WhenUserNotFound()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var request = new UpdateUserProfileRequest
            {
                FullName = "John Doe"
            };

            _userRepository
                .GetUserByIdAsync(userId)
                .Returns((User)null);

            // Act
            var result = await _userService.UpdateUserProfileAsync(userId, request);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.IsSuccess);
            Assert.Equal(404, result.StatusCode);
            Assert.Equal("User not found", result.Message);
            Assert.Null(result.Data);

            await _userRepository.Received(1).GetUserByIdAsync(userId);
        }

        [Fact]
        public async Task UpdateUserProfileAsync_ReturnsFail_WhenFullNameIsEmpty()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var user = new User { Id = userId };

            var request = new UpdateUserProfileRequest
            {
                FullName = "   " // invalid
            };

            _userRepository
                .GetUserByIdAsync(userId)
                .Returns(user);

            // Act
            var result = await _userService.UpdateUserProfileAsync(userId, request);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(400, result.StatusCode);
            Assert.Equal("Full name is required", result.Message);

            // Quan trọng: không được update DB
            await _userRepository.DidNotReceive().UpdateUserAsync(Arg.Any<User>());
            await _userRepository.DidNotReceive().SaveChangesAsync();
        }

        [Fact]
        public async Task UpdateUserProfileAsync_UpdatesUserSuccessfully()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var user = new User
            {
                Id = userId,
                FullName = "Old Name",
                PhoneNumber = "123",
            };

            var request = new UpdateUserProfileRequest
            {
                FullName = "John Doe",
                PhoneNumber = "999",
            };

            _userRepository
                .GetUserByIdAsync(userId)
                .Returns(user);

            // Act
            var result = await _userService.UpdateUserProfileAsync(userId, request);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(200, result.StatusCode);
            Assert.Equal("User profile updated successfully", result.Message);

            Assert.NotNull(result.Data);
            Assert.Equal("John Doe", result.Data.FullName);
            Assert.Equal("999", result.Data.PhoneNumber);

            await _userRepository.Received(1).UpdateUserAsync(user);
            await _userRepository.Received(1).SaveChangesAsync();

            Assert.Equal("John Doe", user.FullName);
            Assert.Equal("999", user.PhoneNumber);
        }
        #endregion

        #region UpdateUserProfileWithAvatarAsync Tests
        [Fact]
        public async Task UpdateUserProfileWithAvatarAsync_ReturnsFail_WhenUserNotFound()
        {
            var userId = Guid.NewGuid();

            _userRepository
                .GetUserByIdAsync(userId)
                .Returns((User)null);

            var result = await _userService.UpdateUserProfileWithAvatarAsync(userId, new UpdateUserProfileRequest(), null);

            Assert.False(result.IsSuccess);
            Assert.Equal(404, result.StatusCode);
        }

        [Fact]
        public async Task UpdateUserProfileWithAvatarAsync_ReturnsFail_WhenFullNameInvalid()
        {
            var userId = Guid.NewGuid();
            var user = new User { Id = userId };

            _userRepository.GetUserByIdAsync(userId).Returns(user);

            var request = new UpdateUserProfileRequest
            {
                FullName = "   "
            };

            var result = await _userService.UpdateUserProfileWithAvatarAsync(userId, request, null);

            Assert.False(result.IsSuccess);
            Assert.Equal(400, result.StatusCode);

            await _photoService.DidNotReceive().UploadPhotoAsync(Arg.Any<IFormFile>());
        }

        [Fact]
        public async Task UpdateUserProfileWithAvatarAsync_UpdatesAvatar_AndDeletesOldOne()
        {
            var userId = Guid.NewGuid();
            var user = new User
            {
                Id = userId,
                AvatarUrl = "https://res.cloudinary.com/demo/image/upload/v1/old-avatar.jpg"
            };

            var file = Substitute.For<IFormFile>();
            file.Length.Returns(100);

            var uploadResult = new ImageUploadResult
            {
                SecureUrl = new Uri("https://cdn.com/new-avatar.jpg")
            };

            _userRepository.GetUserByIdAsync(userId).Returns(user);
            _photoService.UploadPhotoAsync(file).Returns(uploadResult);

            var request = new UpdateUserProfileRequest
            {
                FullName = "John Doe"
            };

            var result = await _userService.UpdateUserProfileWithAvatarAsync(userId, request, file);

            Assert.True(result.IsSuccess);
            Assert.Equal("https://cdn.com/new-avatar.jpg", user.AvatarUrl);

            await _photoService.Received(1).UploadPhotoAsync(file);
            await _photoService.Received(1).DeletePhotoAsync(Arg.Any<string>());
        }

        [Fact]
        public async Task UpdateUserProfileWithAvatarAsync_ReturnsFail_WhenUploadFails()
        {
            var userId = Guid.NewGuid();
            var user = new User { Id = userId };

            var file = Substitute.For<IFormFile>();
            file.Length.Returns(100);

            var uploadResult = new ImageUploadResult
            {
                Error = new Error { Message = "Upload failed" }
            };

            _userRepository.GetUserByIdAsync(userId).Returns(user);
            _photoService.UploadPhotoAsync(file).Returns(uploadResult);

            var request = new UpdateUserProfileRequest
            {
                FullName = "John Doe"
            };

            var result = await _userService.UpdateUserProfileWithAvatarAsync(userId, request, file);

            Assert.False(result.IsSuccess);
            Assert.Contains("Avatar upload failed", result.Message);

            await _userRepository.DidNotReceive().UpdateUserAsync(Arg.Any<User>());
        }

        [Fact]
        public async Task UpdateUserProfileWithAvatarAsync_UsesAvatarUrl_WhenNoFile()
        {
            var userId = Guid.NewGuid();
            var user = new User { Id = userId };

            _userRepository.GetUserByIdAsync(userId).Returns(user);

            var request = new UpdateUserProfileRequest
            {
                FullName = "John Doe",
                AvatarUrl = "https://cdn.com/manual.jpg"
            };

            var result = await _userService.UpdateUserProfileWithAvatarAsync(userId, request, null);

            Assert.True(result.IsSuccess);
            Assert.Equal("https://cdn.com/manual.jpg", user.AvatarUrl);

            await _photoService.DidNotReceive().UploadPhotoAsync(Arg.Any<IFormFile>());
        }
        [Fact]
        public async Task UpdateUserProfileWithAvatarAsync_KeepsOldAvatar_WhenNoNewAvatar()
        {
            var userId = Guid.NewGuid();
            var user = new User
            {
                Id = userId,
                AvatarUrl = "old.jpg"
            };

            _userRepository.GetUserByIdAsync(userId).Returns(user);

            var request = new UpdateUserProfileRequest
            {
                FullName = "John Doe"
            };

            await _userService.UpdateUserProfileWithAvatarAsync(userId, request, null);

            Assert.Equal("old.jpg", user.AvatarUrl);
        }
        #endregion

        #region SearchByEmailAsync
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("  ")]
        [InlineData("ab")]
        public async Task SearchByEmailAsync_ReturnsFail_WhenEmailInvalid(string email)
        {
            // Act
            var result = await _userService.SearchByEmailAsync(email);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(400, result.StatusCode);
            Assert.Equal("Email query must be at least 3 characters.", result.Message);
            Assert.Null(result.Data);

            await _userRepository.DidNotReceive().SearchByEmailAsync(Arg.Any<string>());
        }

        [Fact]
        public async Task SearchByEmailAsync_ReturnsEmptyList_WhenNoUsersFound()
        {
            var email = "test";

            _userRepository
                .SearchByEmailAsync(email)
                .Returns(new List<User>());

            // Act
            var result = await _userService.SearchByEmailAsync(email);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(200, result.StatusCode);
            Assert.NotNull(result.Data);
            Assert.Empty(result.Data);
        }

        [Fact]
        public async Task SearchByEmailAsync_ReturnsMappedResults_WhenUsersFound()
        {
            var email = "test";

            var users = new List<User>
    {
        new User
        {
            Id = Guid.NewGuid(),
            FullName = "John Doe",
            Email = "test1@mail.com",
            AvatarUrl = "avatar1.jpg"
        },
        new User
        {
            Id = Guid.NewGuid(),
            FullName = "Jane Doe",
            Email = "test2@mail.com",
            AvatarUrl = "avatar2.jpg"
        }
    };

            _userRepository
                .SearchByEmailAsync(email)
                .Returns(users);

            // Act
            var result = await _userService.SearchByEmailAsync(email);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(2, result.Data.Count);

            Assert.Equal(users[0].Id, result.Data[0].Id);
            Assert.Equal(users[0].Email, result.Data[0].Email);

            Assert.Equal(users[1].FullName, result.Data[1].FullName);
        }

        [Fact]
        public async Task SearchByEmailAsync_TrimAndLowercaseEmail_BeforeCallingRepository()
        {
            var input = "  TeSt@Mail  ";
            var normalized = "test@mail";

            _userRepository
                .SearchByEmailAsync(normalized)
                .Returns(new List<User>());

            // Act
            await _userService.SearchByEmailAsync(input);

            // Assert
            await _userRepository.Received(1).SearchByEmailAsync(normalized);
        }
        #endregion

        #region GetUserList
        [Fact]
        public async Task GetUserList_ReturnsPagedUsersSuccessfully()
        {
            // Arrange
            var request = new BaseQueryParams
            {
                Page = 1,
                PageSize = 2
            };

            var users = new List<User>
    {
        new User
        {
            Id = Guid.NewGuid(),
            FullName = "John Doe",
            Email = "john@mail.com"
        },
        new User
        {
            Id = Guid.NewGuid(),
            FullName = "Jane Doe",
            Email = "jane@mail.com"
        }
    };

            var pagedUsers = new PagedResult<User>
            (
                users,
                totalItems: 10,
                currentPage: 1,
                pageSize: 2
            );

            _userRepository
                .GetUsersAsync(request)
                .Returns(pagedUsers);

            // Act
            var result = await _userService.GetUserList(request);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.IsSuccess);
            Assert.Equal(200, result.StatusCode);

            var data = result.Data;
            Assert.NotNull(data);

            Assert.Equal(10, data.TotalItems);
            Assert.Equal(1, data.CurrentPage);
            Assert.Equal(2, data.PageSize);

            Assert.Equal(2, data.Items.Count);
            Assert.Equal(users[0].Id, data.Items[0].Id);
            Assert.Equal(users[1].Email, data.Items[1].Email);

            await _userRepository.Received(1).GetUsersAsync(request);
        }

        [Fact]
        public async Task GetUserList_ReturnsEmptyList_WhenNoUsers()
        {
            // Arrange
            var request = new BaseQueryParams();

            var pagedUsers = new PagedResult<User>
            (
                new List<User>(),
                totalItems: 0,
                currentPage: 1,
                pageSize: 10
            );

            _userRepository
                .GetUsersAsync(request)
                .Returns(pagedUsers);

            // Act
            var result = await _userService.GetUserList(request);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Data);
            Assert.Empty(result.Data.Items);
            Assert.Equal(0, result.Data.TotalItems);
        }

        [Fact]
        public async Task GetUserList_MapsUserToUserProfileDto_Correctly()
        {
            // Arrange
            var request = new BaseQueryParams();

            var user = new User
            {
                Id = Guid.NewGuid(),
                FullName = "John Doe",
                Email = "john@mail.com",
                AvatarUrl = "avatar.jpg"
            };

            var pagedUsers = new PagedResult<User>
            (
                new List<User> { user },
                1, 1, 10
            );

            _userRepository.GetUsersAsync(request).Returns(pagedUsers);

            // Act
            var result = await _userService.GetUserList(request);

            // Assert
            var dto = result.Data.Items.First();

            Assert.Equal(user.Id, dto.Id);
            Assert.Equal(user.FullName, dto.FullName);
            Assert.Equal(user.Email, dto.Email);
            Assert.Equal(user.AvatarUrl, dto.AvatarUrl);
        }
        #endregion

        #region UpdateUserStatus
        [Fact]
        public async Task UpdateUserStatus_ReturnsFail_WhenUserNotFound()
        {
            // Arrange
            var userId = Guid.NewGuid();

            _userRepository
                .GetUserByIdAsync(userId)
                .Returns((User)null);

            // Act
            var result = await _userService.UpdateUserStatus(userId);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(404, result.StatusCode);
            Assert.Equal("User not found", result.Message);
            Assert.False(result.Data);

            await _userRepository.Received(1).GetUserByIdAsync(userId);
            await _userRepository.DidNotReceive().UpdateUserStatus(Arg.Any<User>());
        }

        [Fact]
        public async Task UpdateUserStatus_UpdatesSuccessfully()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var user = new User
            {
                Id = userId,
                IsActive = true
            };

            _userRepository
                .GetUserByIdAsync(userId)
                .Returns(user);

            // Act
            var result = await _userService.UpdateUserStatus(userId);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(200, result.StatusCode);
            Assert.Equal("User status updated successfully", result.Message);
            Assert.True(result.Data);

            await _userRepository.Received(1).UpdateUserStatus(user);
            await _userRepository.Received(1).SaveChangesAsync();
        }
        #endregion

        #region GetUserDashboardAsync
        [Fact]
        public async Task GetUserDashboardAsync_ReturnsCorrectDashboard()
        {
            // Arrange
            var today = DateTime.UtcNow.Date;

            int diff = (7 + (today.DayOfWeek - DayOfWeek.Monday)) % 7;
            var startOfWeek = today.AddDays(-diff);

            var startOf7WeeksAgo = startOfWeek.AddDays(-42);

            var userDates = new List<DateTime>
            {
                startOf7WeeksAgo,
                startOf7WeeksAgo.AddDays(1),
                startOf7WeeksAgo.AddDays(7),
            };

            _userRepository
                .GetUserDashboardRawAsync(Arg.Any<DateTime>())
                .Returns((3, userDates));

            // Act
            var result = await _userService.GetUserDashboardAsync();

            // Assert
            Assert.True(result.IsSuccess);

            var data = result.Data;
            Assert.Equal(3, data.TotalUsers);
            Assert.Equal(7, data.UserTrend.Count);

            Assert.Contains(data.UserTrend, w => w.Users == 2);
            Assert.Contains(data.UserTrend, w => w.Users == 1);
        }

        [Fact]
        public async Task GetUserDashboardAsync_ReturnsZeroData_WhenNoUsers()
        {
            // Arrange
            var today = DateTime.UtcNow.Date;

            int diff = (7 + (today.DayOfWeek - DayOfWeek.Monday)) % 7;
            var startOfWeek = today.AddDays(-diff);

            var startOf7WeeksAgo = startOfWeek.AddDays(-42);

            _userRepository
                .GetUserDashboardRawAsync(Arg.Any<DateTime>())
                .Returns((0, new List<DateTime>()));

            // Act
            var result = await _userService.GetUserDashboardAsync();

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(0, result.Data.TotalUsers);

            Assert.Equal(7, result.Data.UserTrend.Count);

            Assert.All(result.Data.UserTrend, w => Assert.Equal(0, w.Users));
        }

        [Fact]
        public async Task GetUserDashboardAsync_AlwaysReturns7Weeks()
        {
            // Arrange
            _userRepository
                .GetUserDashboardRawAsync(Arg.Any<DateTime>())
                .Returns((0, new List<DateTime>()));

            // Act
            var result = await _userService.GetUserDashboardAsync();

            // Assert
            Assert.NotNull(result);
            Assert.True(result.IsSuccess);

            Assert.NotNull(result.Data);
            Assert.Equal(7, result.Data.UserTrend.Count);
        }

        [Fact]
        public async Task GetUserDashboardAsync_GroupsUsersByWeekCorrectly()
        {
            // Arrange
            var today = DateTime.UtcNow.Date;

            int diff = (7 + (today.DayOfWeek - DayOfWeek.Monday)) % 7;
            var startOfWeek = today.AddDays(-diff);

            var startOf7WeeksAgo = startOfWeek.AddDays(-42);

            var sameWeek = startOf7WeeksAgo.AddDays(2);

            var userDates = new List<DateTime>
            {
                startOf7WeeksAgo,
                sameWeek // cùng tuần
            };

            _userRepository
                .GetUserDashboardRawAsync(Arg.Any<DateTime>())
                .Returns((2, userDates));

            // Act
            var result = await _userService.GetUserDashboardAsync();

            // Assert
            Assert.True(result.IsSuccess);

            Assert.Contains(result.Data.UserTrend, w => w.Users == 2);
        }
        #endregion
    }
}
