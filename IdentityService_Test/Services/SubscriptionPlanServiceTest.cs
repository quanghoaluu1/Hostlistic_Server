using IdentityService_Application.DTOs;
using IdentityService_Application.Services;
using IdentityService_Domain.Entities;
using IdentityService_Domain.Interfaces;
using NSubstitute;

namespace IdentityService_Test.Services
{
    public class SubscriptionPlanServiceTest
    {
        private readonly ISubscriptionPlanRepository _repository;
        private readonly SubscriptionPlanService _service;

        public SubscriptionPlanServiceTest()
        {
            _repository = Substitute.For<ISubscriptionPlanRepository>();
            _service = new SubscriptionPlanService(_repository);
        }

        #region CreateAsync Tests
        [Fact]
        public async Task CreateAsync_EmptyName_ShouldFail()
        {
            var dto = new CreateSubscriptionPlanDto
            {
                Name = "",
                DurationInDays = 30,
                MaxEvents = 10,
                MaxAttendeesPerEvent = 100,
                CommissionRate = 0.1f
            };

            var result = await _service.CreateAsync(dto);

            Assert.False(result.IsSuccess);
            Assert.Equal(400, result.StatusCode);

            await _repository.DidNotReceive().AddAsync(Arg.Any<SubscriptionPlan>());
        }

        [Fact]
        public async Task CreateAsync_InvalidDuration_ShouldFail()
        {
            var dto = new CreateSubscriptionPlanDto
            {
                Name = "Basic",
                DurationInDays = 0,
                MaxEvents = 10,
                MaxAttendeesPerEvent = 100,
                CommissionRate = 0.1f
            };

            var result = await _service.CreateAsync(dto);

            Assert.False(result.IsSuccess);
        }

        [Fact]
        public async Task CreateAsync_NegativeMaxEvents_ShouldFail()
        {
            var dto = new CreateSubscriptionPlanDto
            {
                Name = "Basic",
                DurationInDays = 30,
                MaxEvents = -1,
                MaxAttendeesPerEvent = 100,
                CommissionRate = 0.1f
            };

            var result = await _service.CreateAsync(dto);

            Assert.False(result.IsSuccess);
        }

        [Fact]
        public async Task CreateAsync_InvalidMaxAttendees_ShouldFail()
        {
            var dto = new CreateSubscriptionPlanDto
            {
                Name = "Basic",
                DurationInDays = 30,
                MaxEvents = 10,
                MaxAttendeesPerEvent = 0,
                CommissionRate = 0.1f
            };

            var result = await _service.CreateAsync(dto);

            Assert.False(result.IsSuccess);
        }

        [Theory]
        [InlineData(-0.1f)]
        [InlineData(1.1f)]
        public async Task CreateAsync_InvalidCommissionRate_ShouldFail(float rate)
        {
            var dto = new CreateSubscriptionPlanDto
            {
                Name = "Basic",
                DurationInDays = 30,
                MaxEvents = 10,
                MaxAttendeesPerEvent = 100,
                CommissionRate = rate
            };

            var result = await _service.CreateAsync(dto);

            Assert.False(result.IsSuccess);
        }

        [Fact]
        public async Task CreateAsync_ValidData_ShouldCreateSuccessfully()
        {
            var dto = new CreateSubscriptionPlanDto
            {
                Name = "Pro",
                DurationInDays = 30,
                MaxEvents = 10,
                MaxAttendeesPerEvent = 100,
                CommissionRate = 0.2f
            };

            var result = await _service.CreateAsync(dto);

            Assert.True(result.IsSuccess);
            Assert.Equal(201, result.StatusCode);
            Assert.NotNull(result.Data);
            Assert.Equal("Pro", result.Data.Name);

            await _repository.Received(1).AddAsync(Arg.Any<SubscriptionPlan>());
            await _repository.Received(1).SaveChangesAsync();
        }
        #endregion

        #region GetAllAsync Tests
        [Fact]
        public async Task GetAllAsync_HasData_ShouldReturnList()
        {
            // Arrange
            var plans = new List<SubscriptionPlan>
            {
                new SubscriptionPlan
                {
                    Id = Guid.NewGuid(),
                    Name = "Basic",
                    IsActive = true
                },
                new SubscriptionPlan
                {
                    Id = Guid.NewGuid(),
                    Name = "Pro",
                    IsActive = false
                }
            };

            _repository.GetAllAsync(true).Returns(plans);

            // Act
            var result = await _service.GetAllAsync(true);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(200, result.StatusCode);
            Assert.NotNull(result.Data);
            Assert.Equal(2, result.Data.Count());

            await _repository.Received(1).GetAllAsync(true);
        }

        [Fact]
        public async Task GetAllAsync_ExcludeInactive_ShouldCallRepositoryCorrectly()
        {
            var plans = new List<SubscriptionPlan>
            {
                new SubscriptionPlan { Name = "Basic", IsActive = true }
            };

            _repository.GetAllAsync(false).Returns(plans);

            var result = await _service.GetAllAsync(false);

            Assert.True(result.IsSuccess);
            Assert.Single(result.Data);

            await _repository.Received(1).GetAllAsync(false);
        }

        [Fact]
        public async Task GetAllAsync_EmptyList_ShouldReturnEmpty()
        {
            _repository.GetAllAsync(true)
                .Returns(new List<SubscriptionPlan>());

            var result = await _service.GetAllAsync(true);

            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Data);
            Assert.Empty(result.Data);
        }
        #endregion

        #region GetByIdAsync Tests
        [Fact]
        public async Task GetByIdAsync_NotFound_ShouldReturn404()
        {
            // Arrange
            var id = Guid.NewGuid();

            _repository.GetByIdAsync(id)
                .Returns((SubscriptionPlan?)null);

            // Act
            var result = await _service.GetByIdAsync(id);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(404, result.StatusCode);
            Assert.Equal("Không tìm thấy", result.Message);

            await _repository.Received(1).GetByIdAsync(id);
        }

        [Fact]
        public async Task GetByIdAsync_Found_ShouldReturnData()
        {
            // Arrange
            var id = Guid.NewGuid();

            var entity = new SubscriptionPlan
            {
                Id = id,
                Name = "Pro",
                DurationInDays = 30,
                MaxEvents = 10,
                MaxAttendeesPerEvent = 100,
                CommissionRate = 0.2f,
                IsActive = true
            };

            _repository.GetByIdAsync(id)
                .Returns(entity);

            // Act
            var result = await _service.GetByIdAsync(id);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(200, result.StatusCode);
            Assert.NotNull(result.Data);
            Assert.Equal(entity.Name, result.Data.Name);

            await _repository.Received(1).GetByIdAsync(id);
        }
        #endregion

        #region UpdateAsync Tests
        [Fact]
        public async Task UpdateAsync_NotFound_ShouldReturn404()
        {
            var id = Guid.NewGuid();

            _repository.GetByIdAsync(id)
                .Returns((SubscriptionPlan?)null);

            var result = await _service.UpdateAsync(id, new UpdateSubscriptionPlanDto());

            Assert.False(result.IsSuccess);
            Assert.Equal(404, result.StatusCode);

            await _repository.DidNotReceive().UpdateAsync(Arg.Any<SubscriptionPlan>());
        }

        [Fact]
        public async Task UpdateAsync_FullUpdate_ShouldUpdateAllFields()
        {
            var id = Guid.NewGuid();

            var entity = new SubscriptionPlan
            {
                Id = id,
                Name = "Old",
                Price = 100,
                DurationInDays = 30,
                MaxEvents = 10,
                MaxAttendeesPerEvent = 100,
                CommissionRate = 0.1f,
                HasAiAccess = false,
                IsActive = true
            };

            _repository.GetByIdAsync(id).Returns(entity);

            var dto = new UpdateSubscriptionPlanDto
            {
                Name = "New",
                Price = 200,
                Description = "Updated",
                DurationInDays = 60,
                MaxEvents = 20,
                MaxAttendeesPerEvent = 200,
                CommissionRate = 0.5f,
                HasAiAccess = true,
                IsActive = false
            };

            var result = await _service.UpdateAsync(id, dto);

            Assert.True(result.IsSuccess);
            Assert.Equal("New", entity.Name);
            Assert.Equal(200, entity.Price);
            Assert.Equal("Updated", entity.Description);
            Assert.Equal(60, entity.DurationInDays);
            Assert.Equal(20, entity.MaxEvents);
            Assert.Equal(200, entity.MaxAttendeesPerEvent);
            Assert.Equal(0.5, entity.CommissionRate);
            Assert.True(entity.HasAiAccess);
            Assert.False(entity.IsActive);

            await _repository.Received(1).UpdateAsync(entity);
        }

        [Fact]
        public async Task UpdateAsync_PartialUpdate_ShouldOnlyUpdateProvidedFields()
        {
            var id = Guid.NewGuid();

            var entity = new SubscriptionPlan
            {
                Id = id,
                Name = "Old",
                Price = 100
            };

            _repository.GetByIdAsync(id).Returns(entity);

            var dto = new UpdateSubscriptionPlanDto
            {
                Name = "New"
                // các field khác null
            };

            var result = await _service.UpdateAsync(id, dto);

            Assert.True(result.IsSuccess);
            Assert.Equal("New", entity.Name);
            Assert.Equal(100, entity.Price); // không đổi
        }

        [Fact]
        public async Task UpdateAsync_InvalidDuration_ShouldNotUpdate()
        {
            var id = Guid.NewGuid();

            var entity = new SubscriptionPlan
            {
                Id = id,
                DurationInDays = 30
            };

            _repository.GetByIdAsync(id).Returns(entity);

            var dto = new UpdateSubscriptionPlanDto
            {
                DurationInDays = 0
            };

            var result = await _service.UpdateAsync(id, dto);

            Assert.True(result.IsSuccess);
            Assert.Equal(30, entity.DurationInDays); // giữ nguyên
        }

        [Theory]
        [InlineData(-0.1f)]
        [InlineData(1.5f)]
        public async Task UpdateAsync_InvalidCommissionRate_ShouldNotUpdate(float rate)
        {
            var id = Guid.NewGuid();

            var entity = new SubscriptionPlan
            {
                Id = id,
                CommissionRate = 0.2f
            };

            _repository.GetByIdAsync(id).Returns(entity);

            var dto = new UpdateSubscriptionPlanDto
            {
                CommissionRate = rate
            };

            var result = await _service.UpdateAsync(id, dto);

            Assert.True(result.IsSuccess);
            Assert.Equal(0.2f, entity.CommissionRate);
        }

        [Fact]
        public async Task UpdateAsync_EmptyName_ShouldNotOverwrite()
        {
            var id = Guid.NewGuid();

            var entity = new SubscriptionPlan
            {
                Id = id,
                Name = "Old"
            };

            _repository.GetByIdAsync(id).Returns(entity);

            var dto = new UpdateSubscriptionPlanDto
            {
                Name = ""
            };

            var result = await _service.UpdateAsync(id, dto);

            Assert.Equal("Old", entity.Name);
        }

        [Fact]
        public async Task UpdateAsync_UpdateBooleanFields_ShouldUpdate()
        {
            var id = Guid.NewGuid();

            var entity = new SubscriptionPlan
            {
                Id = id,
                HasAiAccess = false,
                IsActive = true
            };

            _repository.GetByIdAsync(id).Returns(entity);

            var dto = new UpdateSubscriptionPlanDto
            {
                HasAiAccess = true,
                IsActive = false
            };

            var result = await _service.UpdateAsync(id, dto);

            Assert.True(entity.HasAiAccess);
            Assert.False(entity.IsActive);
        }
        #endregion

        #region DeleteAsync Tests
        [Fact]
        public async Task DeleteAsync_NotFound_ShouldReturn404()
        {
            // Arrange
            var id = Guid.NewGuid();

            _repository.DeleteAsync(id).Returns(false);

            // Act
            var result = await _service.DeleteAsync(id);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(404, result.StatusCode);
            Assert.Equal("Không tìm thấy", result.Message);

            await _repository.DidNotReceive().SaveChangesAsync();
        }

        [Fact]
        public async Task DeleteAsync_Success_ShouldReturnTrue()
        {
            // Arrange
            var id = Guid.NewGuid();

            _repository.DeleteAsync(id).Returns(true);

            // Act
            var result = await _service.DeleteAsync(id);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(200, result.StatusCode);
            Assert.True(result.Data);

            await _repository.Received(1).DeleteAsync(id);
            await _repository.Received(1).SaveChangesAsync();
        }


        #endregion
    }
}
