using IdentityService_Application.DTOs;
using IdentityService_Application.Services;
using IdentityService_Domain.Entities;
using IdentityService_Domain.Interfaces;
using NSubstitute;

namespace IdentityService_Test.Services
{
    public class UserPlanServiceTest
    {
        private readonly UserPlanService _service;
        private readonly IUserPlanRepository _repository;
        private readonly ISubscriptionPlanRepository _subscriptionRepo;

        public UserPlanServiceTest()
        {
            _repository = Substitute.For<IUserPlanRepository>();
            _subscriptionRepo = Substitute.For<ISubscriptionPlanRepository>();
            _service = new UserPlanService(_repository, _subscriptionRepo);
        }

        #region CreateAsync Tests
        [Fact]
        public async Task CreateAsync_ShouldFail_WhenIdsAreEmpty()
        {
            // Arrange
            var dto = new CreateUserPlanDto
            {
                UserId = Guid.Empty,
                SubscriptionPlanId = Guid.Empty
            };

            // Act
            var result = await _service.CreateAsync(dto);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(400, result.StatusCode);


        }

        [Fact]
        public async Task CreateAsync_ShouldFail_WhenPlanNotFound()
        {
            // Arrange
            var dto = new CreateUserPlanDto
            {
                UserId = Guid.NewGuid(),
                SubscriptionPlanId = Guid.NewGuid()
            };

            _subscriptionRepo.GetByIdAsync(dto.SubscriptionPlanId)
                .Returns((SubscriptionPlan?)null);

            // Act
            var result = await _service.CreateAsync(dto);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(400, result.StatusCode);
        }

        [Fact]
        public async Task CreateAsync_ShouldFail_WhenPlanInactive()
        {
            // Arrange
            var dto = new CreateUserPlanDto
            {
                UserId = Guid.NewGuid(),
                SubscriptionPlanId = Guid.NewGuid()
            };

            var plan = new SubscriptionPlan
            {
                Id = dto.SubscriptionPlanId,
                IsActive = false,
                DurationInDays = 30
            };

            _subscriptionRepo.GetByIdAsync(dto.SubscriptionPlanId)
                .Returns(plan);

            // Act
            var result = await _service.CreateAsync(dto);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(400, result.StatusCode);
        }

        [Fact]
        public async Task CreateAsync_ShouldFail_WhenUserAlreadyHasActivePlan()
        {
            // Arrange
            var dto = new CreateUserPlanDto
            {
                UserId = Guid.NewGuid(),
                SubscriptionPlanId = Guid.NewGuid()
            };

            var plan = new SubscriptionPlan
            {
                Id = dto.SubscriptionPlanId,
                IsActive = true,
                DurationInDays = 30
            };

            var existingPlans = new List<UserPlan>
        {
            new UserPlan
            {
                SubscriptionPlanId = dto.SubscriptionPlanId,
                IsActive = true
            }
        };

            _subscriptionRepo.GetByIdAsync(dto.SubscriptionPlanId).Returns(plan);
            _repository.GetByUserIdAsync(dto.UserId, true).Returns(existingPlans);

            // Act
            var result = await _service.CreateAsync(dto);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(400, result.StatusCode);
        }

        [Fact]
        public async Task CreateAsync_ShouldSuccess_WhenValid()
        {
            // Arrange
            var dto = new CreateUserPlanDto
            {
                UserId = Guid.NewGuid(),
                SubscriptionPlanId = Guid.NewGuid()
            };

            var plan = new SubscriptionPlan
            {
                Id = dto.SubscriptionPlanId,
                IsActive = true,
                DurationInDays = 30
            };

            _subscriptionRepo.GetByIdAsync(dto.SubscriptionPlanId).Returns(plan);
            _repository.GetByUserIdAsync(dto.UserId, true).Returns(new List<UserPlan>());

            // Act
            var result = await _service.CreateAsync(dto);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(201, result.StatusCode);

            await _repository.Received(1).AddAsync(Arg.Any<UserPlan>());
            await _repository.Received(1).SaveChangesAsync();
        }
        #endregion

        #region GetByIdAsync Tests
        [Fact]
        public async Task GetByIdAsync_ShouldReturn404_WhenNotFound()
        {
            // Arrange
            var id = Guid.NewGuid();
            _repository.GetByIdAsync(id).Returns((UserPlan?)null);

            // Act
            var result = await _service.GetByIdAsync(id);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.IsSuccess);
            Assert.Equal(404, result.StatusCode);
            Assert.Equal("Không tìm thấy", result.Message);
            Assert.Null(result.Data);

            await _repository.Received(1).GetByIdAsync(id);
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnUserPlan_WhenFound()
        {
            // Arrange
            var id = Guid.NewGuid();

            var entity = new UserPlan
            {
                Id = id,
                UserId = Guid.NewGuid(),
                SubscriptionPlanId = Guid.NewGuid(),
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddDays(30),
                IsActive = true
            };

            _repository.GetByIdAsync(id).Returns(entity);

            // Act
            var result = await _service.GetByIdAsync(id);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.IsSuccess);
            Assert.Equal(200, result.StatusCode);
            Assert.Equal("OK", result.Message);

            Assert.NotNull(result.Data);
            Assert.Equal(entity.Id, result.Data.Id);
            Assert.Equal(entity.UserId, result.Data.UserId);
            Assert.Equal(entity.SubscriptionPlanId, result.Data.SubscriptionPlanId);

            await _repository.Received(1).GetByIdAsync(id);
        }
        #endregion

        #region GetByUserIdAsync Tests
        [Fact]
        public async Task GetByUserIdAsync_ShouldReturnList_WhenDataExists()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var onlyActive = true;

            var entities = new List<UserPlan>
    {
        new UserPlan
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            SubscriptionPlanId = Guid.NewGuid(),
            IsActive = true
        },
        new UserPlan
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            SubscriptionPlanId = Guid.NewGuid(),
            IsActive = true
        }
    };

            _repository.GetByUserIdAsync(userId, onlyActive)
                .Returns(entities);

            // Act
            var result = await _service.GetByUserIdAsync(userId, onlyActive);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.IsSuccess);
            Assert.Equal(200, result.StatusCode);
            Assert.Equal("OK", result.Message);

            Assert.NotNull(result.Data);
            Assert.Equal(2, result.Data.Count());

            foreach (var item in result.Data)
            {
                Assert.Equal(userId, item.UserId);
            }

            await _repository.Received(1)
                .GetByUserIdAsync(userId, onlyActive);
        }

        [Fact]
        public async Task GetByUserIdAsync_ShouldReturnEmptyList_WhenNoData()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var onlyActive = true;

            _repository.GetByUserIdAsync(userId, onlyActive)
                .Returns(new List<UserPlan>());

            // Act
            var result = await _service.GetByUserIdAsync(userId, onlyActive);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.IsSuccess);
            Assert.Equal(200, result.StatusCode);
            Assert.Equal("OK", result.Message);

            Assert.NotNull(result.Data);
            Assert.Empty(result.Data);

            await _repository.Received(1)
                .GetByUserIdAsync(userId, onlyActive);
        }
        #endregion

        #region UpdateAsync Tests
        [Fact]
        public async Task UpdateAsync_ShouldReturn404_WhenNotFound()
        {
            // Arrange
            var id = Guid.NewGuid();
            var dto = new UpdateUserPlanDto();

            _repository.GetByIdAsync(id).Returns((UserPlan?)null);

            // Act
            var result = await _service.UpdateAsync(id, dto);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.IsSuccess);
            Assert.Equal(404, result.StatusCode);
            Assert.Equal("Không tìm thấy", result.Message);
            Assert.Null(result.Data);

            await _repository.Received(1).GetByIdAsync(id);
            await _repository.DidNotReceive().UpdateAsync(Arg.Any<UserPlan>());
        }

        [Fact]
        public async Task UpdateAsync_ShouldUpdateEndDate_WhenProvided()
        {
            // Arrange
            var id = Guid.NewGuid();
            var oldEndDate = DateTime.UtcNow;
            var newEndDate = oldEndDate.AddDays(10);

            var entity = new UserPlan
            {
                Id = id,
                EndDate = oldEndDate,
                IsActive = true
            };

            var dto = new UpdateUserPlanDto
            {
                EndDate = newEndDate
            };

            _repository.GetByIdAsync(id).Returns(entity);

            // Act
            var result = await _service.UpdateAsync(id, dto);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(200, result.StatusCode);
            Assert.Equal("Cập nhật thành công", result.Message);

            Assert.NotNull(result.Data);
            Assert.Equal(newEndDate, result.Data.EndDate);

            await _repository.Received(1).UpdateAsync(Arg.Is<UserPlan>(x =>
                x.EndDate == newEndDate
            ));
            await _repository.Received(1).SaveChangesAsync();
        }

        [Fact]
        public async Task UpdateAsync_ShouldUpdateIsActive_WhenProvided()
        {
            // Arrange
            var id = Guid.NewGuid();

            var entity = new UserPlan
            {
                Id = id,
                IsActive = true
            };

            var dto = new UpdateUserPlanDto
            {
                IsActive = false
            };

            _repository.GetByIdAsync(id).Returns(entity);

            // Act
            var result = await _service.UpdateAsync(id, dto);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(200, result.StatusCode);

            Assert.NotNull(result.Data);
            Assert.False(result.Data.IsActive);

            await _repository.Received(1).UpdateAsync(Arg.Is<UserPlan>(x =>
                x.IsActive == false
            ));
        }

        [Fact]
        public async Task UpdateAsync_ShouldNotChangeData_WhenDtoIsEmpty()
        {
            // Arrange
            var id = Guid.NewGuid();

            var entity = new UserPlan
            {
                Id = id,
                EndDate = DateTime.UtcNow,
                IsActive = true
            };

            var dto = new UpdateUserPlanDto(); // không có field nào

            _repository.GetByIdAsync(id).Returns(entity);

            // Act
            var result = await _service.UpdateAsync(id, dto);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(200, result.StatusCode);

            Assert.NotNull(result.Data);
            Assert.Equal(entity.EndDate, result.Data.EndDate);
            Assert.True(result.Data.IsActive);

            await _repository.Received(1).UpdateAsync(entity);
        }
        #endregion

        #region CancelAsync Tests
        [Fact]
        public async Task CancelAsync_ShouldReturn404_WhenNotFound()
        {
            // Arrange
            var id = Guid.NewGuid();
            _repository.GetByIdAsync(id).Returns((UserPlan?)null);

            // Act
            var result = await _service.CancelAsync(id);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.IsSuccess);
            Assert.Equal(404, result.StatusCode);
            Assert.Equal("Không tìm thấy", result.Message);
            Assert.False(result.Data);

            await _repository.Received(1).GetByIdAsync(id);
            await _repository.DidNotReceive().UpdateAsync(Arg.Any<UserPlan>());
        }

        [Fact]
        public async Task CancelAsync_ShouldSetEndDate_WhenEndDateIsNull()
        {
            // Arrange
            var id = Guid.NewGuid();

            var entity = new UserPlan
            {
                Id = id,
                IsActive = true,
                EndDate = null
            };

            _repository.GetByIdAsync(id).Returns(entity);

            // Act
            var result = await _service.CancelAsync(id);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(200, result.StatusCode);
            Assert.Equal("Huỷ gói thành công", result.Message);
            Assert.True(result.Data);

            Assert.False(entity.IsActive);
            Assert.NotNull(entity.EndDate);

            await _repository.Received(1).UpdateAsync(Arg.Is<UserPlan>(x =>
                x.IsActive == false &&
                x.EndDate != null
            ));

            await _repository.Received(1).SaveChangesAsync();
        }

        [Fact]
        public async Task CancelAsync_ShouldKeepEndDate_WhenAlreadyExists()
        {
            // Arrange
            var id = Guid.NewGuid();
            var existingEndDate = DateTime.UtcNow.AddDays(5);

            var entity = new UserPlan
            {
                Id = id,
                IsActive = true,
                EndDate = existingEndDate
            };

            _repository.GetByIdAsync(id).Returns(entity);

            // Act
            var result = await _service.CancelAsync(id);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(200, result.StatusCode);

            Assert.False(entity.IsActive);
            Assert.Equal(existingEndDate, entity.EndDate);

            await _repository.Received(1).UpdateAsync(Arg.Is<UserPlan>(x =>
                x.EndDate == existingEndDate
            ));
        }
        #endregion
    }
}
