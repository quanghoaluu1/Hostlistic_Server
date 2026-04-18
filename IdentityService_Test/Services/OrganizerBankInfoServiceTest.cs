using IdentityService_Application.DTOs;
using IdentityService_Application.Services;
using IdentityService_Domain.Entities;
using IdentityService_Domain.Interfaces;
using NSubstitute;

namespace IdentityService_Test.Services
{
    public class OrganizerBankInfoServiceTest
    {
        private readonly IOrganizerBankInfoRepository _organizerBankInfoRepository;
        private readonly OrganizerBankInfoService _service;

        public OrganizerBankInfoServiceTest()
        {
            _organizerBankInfoRepository = Substitute.For<IOrganizerBankInfoRepository>();
            _service = new OrganizerBankInfoService(_organizerBankInfoRepository);
        }

        #region CreateAsync Tests
        [Fact]
        public async Task CreateAsync_MissingBankInfo_ShouldFail()
        {
            // Arrange
            var dto = new CreateOrganizerBankInfoDto
            {
                BankName = "",
                AccountNumber = "123",
                AccountName = "Test",
                BankBin = "9704",
                UserId = Guid.NewGuid()
            };

            // Act
            var result = await _service.CreateAsync(dto);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(400, result.StatusCode);
            Assert.Equal("Thiếu thông tin ngân hàng", result.Message);

            await _organizerBankInfoRepository.DidNotReceive().AddAsync(Arg.Any<OrganizerBankInfo>());
        }

        [Fact]
        public async Task CreateAsync_NoUserAndOrganization_ShouldFail()
        {
            // Arrange
            var dto = new CreateOrganizerBankInfoDto
            {
                BankName = "VCB",
                AccountNumber = "123456",
                AccountName = "Test",
                BankBin = "9704",
                UserId = null,
                OrganizationId = null
            };

            // Act
            var result = await _service.CreateAsync(dto);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(400, result.StatusCode);
            Assert.Equal("Phải gắn với User hoặc Organization", result.Message);

            await _organizerBankInfoRepository.DidNotReceive().AddAsync(Arg.Any<OrganizerBankInfo>());
        }

        [Fact]
        public async Task CreateAsync_ValidUser_ShouldCreateSuccessfully()
        {
            // Arrange
            var dto = new CreateOrganizerBankInfoDto
            {
                BankName = "VCB",
                AccountNumber = "123456",
                AccountName = "Test User",
                BankBin = "9704",
                UserId = Guid.NewGuid()
            };

            // Act
            var result = await _service.CreateAsync(dto);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(201, result.StatusCode);
            Assert.NotNull(result.Data);
            Assert.Equal(dto.BankName, result.Data.BankName);

            await _organizerBankInfoRepository.Received(1).AddAsync(Arg.Any<OrganizerBankInfo>());
            await _organizerBankInfoRepository.Received(1).SaveChangesAsync();
        }

        [Fact]
        public async Task CreateAsync_ValidOrganization_ShouldCreateSuccessfully()
        {
            // Arrange
            var dto = new CreateOrganizerBankInfoDto
            {
                BankName = "ACB",
                AccountNumber = "999999",
                AccountName = "Org Name",
                BankBin = "9704",
                OrganizationId = Guid.NewGuid()
            };

            // Act
            var result = await _service.CreateAsync(dto);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(201, result.StatusCode);

            await _organizerBankInfoRepository.Received(1).AddAsync(Arg.Any<OrganizerBankInfo>());
        }
        #endregion

        #region GetByIdAsync Tests
        [Fact]
        public async Task GetByIdAsync_NotFound_ShouldReturn404()
        {
            // Arrange
            var id = Guid.NewGuid();

            _organizerBankInfoRepository.GetByIdAsync(id)
                .Returns((OrganizerBankInfo?)null);

            // Act
            var result = await _service.GetByIdAsync(id);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(404, result.StatusCode);
            Assert.Equal("Không tìm thấy", result.Message);
        }

        [Fact]
        public async Task GetByIdAsync_Found_ShouldReturnData()
        {
            // Arrange
            var id = Guid.NewGuid();

            var entity = new OrganizerBankInfo
            {
                Id = id,
                BankName = "VCB",
                AccountNumber = "123456",
                AccountName = "Test User",
                BankBin = "9704"
            };

            _organizerBankInfoRepository.GetByIdAsync(id)
                .Returns(entity);

            // Act
            var result = await _service.GetByIdAsync(id);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(200, result.StatusCode);
            Assert.NotNull(result.Data);
            Assert.Equal(entity.BankName, result.Data.BankName);
            Assert.Equal(entity.AccountNumber, result.Data.AccountNumber);
        }
        #endregion

        #region GetByUserIdAsync Tests
        [Fact]
        public async Task GetByUserIdAsync_HasData_ShouldReturnList()
        {
            // Arrange
            var userId = Guid.NewGuid();

            var entities = new List<OrganizerBankInfo>
    {
        new OrganizerBankInfo
        {
            Id = Guid.NewGuid(),
            BankName = "VCB",
            AccountNumber = "123",
            AccountName = "User A",
            BankBin = "9704"
        },
        new OrganizerBankInfo
        {
            Id = Guid.NewGuid(),
            BankName = "ACB",
            AccountNumber = "456",
            AccountName = "User B",
            BankBin = "9704"
        }
    };

            _organizerBankInfoRepository.GetByUserIdAsync(userId).Returns(entities);

            // Act
            var result = await _service.GetByUserIdAsync(userId);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(200, result.StatusCode);
            Assert.NotNull(result.Data);
            Assert.Equal(2, result.Data.Count());

            await _organizerBankInfoRepository.Received(1).GetByUserIdAsync(userId);
        }

        [Fact]
        public async Task GetByUserIdAsync_EmptyList_ShouldReturnEmpty()
        {
            // Arrange
            var userId = Guid.NewGuid();

            _organizerBankInfoRepository.GetByUserIdAsync(userId)
                .Returns(new List<OrganizerBankInfo>());

            // Act
            var result = await _service.GetByUserIdAsync(userId);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(200, result.StatusCode);
            Assert.NotNull(result.Data);
            Assert.Empty(result.Data);
        }
        #endregion

        #region GetByOrganizationIdAsync Tests
        [Fact]
        public async Task GetByOrganizationIdAsync_HasData_ShouldReturnList()
        {
            // Arrange
            var organizationId = Guid.NewGuid();

            var entities = new List<OrganizerBankInfo>
    {
        new OrganizerBankInfo
        {
            Id = Guid.NewGuid(),
            BankName = "VCB",
            AccountNumber = "123",
            AccountName = "Org A",
            BankBin = "9704"
        },
        new OrganizerBankInfo
        {
            Id = Guid.NewGuid(),
            BankName = "ACB",
            AccountNumber = "456",
            AccountName = "Org B",
            BankBin = "9704"
        }
    };

            _organizerBankInfoRepository.GetByOrganizationIdAsync(organizationId)
                .Returns(entities);

            // Act
            var result = await _service.GetByOrganizationIdAsync(organizationId);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(200, result.StatusCode);
            Assert.NotNull(result.Data);
            Assert.Equal(2, result.Data.Count());

            await _organizerBankInfoRepository.Received(1)
                .GetByOrganizationIdAsync(organizationId);
        }

        [Fact]
        public async Task GetByOrganizationIdAsync_EmptyList_ShouldReturnEmpty()
        {
            var organizationId = Guid.NewGuid();

            _organizerBankInfoRepository.GetByOrganizationIdAsync(organizationId)
                .Returns(new List<OrganizerBankInfo>());

            var result = await _service.GetByOrganizationIdAsync(organizationId);

            Assert.True(result.IsSuccess);
            Assert.Equal(200, result.StatusCode);
            Assert.NotNull(result.Data);
            Assert.Empty(result.Data);
        }
        #endregion

        #region UpdateAsync Tests
        [Fact]
        public async Task UpdateAsync_NotFound_ShouldReturn404()
        {
            var id = Guid.NewGuid();

            _organizerBankInfoRepository.GetByIdAsync(id)
                .Returns((OrganizerBankInfo?)null);

            var dto = new UpdateOrganizerBankInfoDto();

            var result = await _service.UpdateAsync(id, dto);

            Assert.False(result.IsSuccess);
            Assert.Equal(404, result.StatusCode);

            await _organizerBankInfoRepository.DidNotReceive().UpdateAsync(Arg.Any<OrganizerBankInfo>());
        }

        [Fact]
        public async Task UpdateAsync_FullUpdate_ShouldUpdateAllFields()
        {
            var id = Guid.NewGuid();

            var entity = new OrganizerBankInfo
            {
                Id = id,
                BankName = "Old",
                AccountNumber = "111",
                AccountName = "Old Name",
                BankBin = "0000"
            };

            _organizerBankInfoRepository.GetByIdAsync(id).Returns(entity);

            var dto = new UpdateOrganizerBankInfoDto
            {
                BankName = "New Bank",
                AccountNumber = "222",
                AccountName = "New Name",
                BankBin = "9704",
                UserId = Guid.NewGuid(),
                OrganizationId = Guid.NewGuid()
            };

            var result = await _service.UpdateAsync(id, dto);

            Assert.True(result.IsSuccess);
            Assert.Equal("New Bank", entity.BankName);
            Assert.Equal("222", entity.AccountNumber);
            Assert.Equal("New Name", entity.AccountName);
            Assert.Equal("9704", entity.BankBin);

            await _organizerBankInfoRepository.Received(1).UpdateAsync(entity);
            await _organizerBankInfoRepository.Received(1).SaveChangesAsync();
        }

        [Fact]
        public async Task UpdateAsync_PartialUpdate_ShouldOnlyUpdateProvidedFields()
        {
            var id = Guid.NewGuid();

            var entity = new OrganizerBankInfo
            {
                Id = id,
                BankName = "Old Bank",
                AccountNumber = "111",
                AccountName = "Old Name",
                BankBin = "0000"
            };

            _organizerBankInfoRepository.GetByIdAsync(id).Returns(entity);

            var dto = new UpdateOrganizerBankInfoDto
            {
                BankName = "New Bank",
                AccountNumber = null // không update
            };

            var result = await _service.UpdateAsync(id, dto);

            Assert.True(result.IsSuccess);
            Assert.Equal("New Bank", entity.BankName);
            Assert.Equal("111", entity.AccountNumber); // giữ nguyên
        }

        [Fact]
        public async Task UpdateAsync_EmptyString_ShouldNotOverwrite()
        {
            var id = Guid.NewGuid();

            var entity = new OrganizerBankInfo
            {
                Id = id,
                BankName = "Old Bank"
            };

            _organizerBankInfoRepository.GetByIdAsync(id).Returns(entity);

            var dto = new UpdateOrganizerBankInfoDto
            {
                BankName = "" // rỗng
            };

            var result = await _service.UpdateAsync(id, dto);

            Assert.True(result.IsSuccess);
            Assert.Equal("Old Bank", entity.BankName); // không bị overwrite
        }

        [Fact]
        public async Task UpdateAsync_UpdateUserAndOrganization_ShouldUpdate()
        {
            var id = Guid.NewGuid();

            var entity = new OrganizerBankInfo
            {
                Id = id
            };

            _organizerBankInfoRepository.GetByIdAsync(id).Returns(entity);

            var newUserId = Guid.NewGuid();
            var newOrgId = Guid.NewGuid();

            var dto = new UpdateOrganizerBankInfoDto
            {
                UserId = newUserId,
                OrganizationId = newOrgId
            };

            var result = await _service.UpdateAsync(id, dto);

            Assert.True(result.IsSuccess);
            Assert.Equal(newUserId, entity.UserId);
            Assert.Equal(newOrgId, entity.OrganizationId);
        }
        #endregion

        #region DeleteAsync Tests
        [Fact]
        public async Task DeleteAsync_NotFound_ShouldReturn404()
        {
            // Arrange
            var id = Guid.NewGuid();

            _organizerBankInfoRepository.DeleteAsync(id).Returns(false);

            // Act
            var result = await _service.DeleteAsync(id);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(404, result.StatusCode);
            Assert.Equal("Không tìm thấy", result.Message);

            await _organizerBankInfoRepository.DidNotReceive().SaveChangesAsync();
        }

        [Fact]
        public async Task DeleteAsync_Success_ShouldDeleteAndReturnTrue()
        {
            // Arrange
            var id = Guid.NewGuid();

            _organizerBankInfoRepository.DeleteAsync(id).Returns(true);

            // Act
            var result = await _service.DeleteAsync(id);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(200, result.StatusCode);
            Assert.True(result.Data);

            await _organizerBankInfoRepository.Received(1).DeleteAsync(id);
            await _organizerBankInfoRepository.Received(1).SaveChangesAsync();
        }
        #endregion
    }
}
