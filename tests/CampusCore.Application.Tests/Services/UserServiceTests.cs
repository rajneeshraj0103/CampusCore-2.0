
using CampusCore.Application.DTOs.Users;
using CampusCore.Application.Exceptions;
using CampusCore.Application.Interfaces;
using CampusCore.Application.Services;
using CampusCore.Domain.Entities;
using Moq;

namespace CampusCore.Application.Tests.Services
{
    public class UserServiceTests
    {
        private readonly Mock<IUserRepository> _userRepositoryMock;
        private readonly Mock<IRoleRepository> _roleRepositoryMock;

        private readonly UserService _userService;

        public UserServiceTests()
        {
            _userRepositoryMock = new Mock<IUserRepository>();
            _roleRepositoryMock = new Mock<IRoleRepository>();

            _userService = new UserService(
                _userRepositoryMock.Object,
                _roleRepositoryMock.Object);
        }

        [Fact]
        public async Task GetUserByIdAsync_WhenUserExists_ReturnUser()
        {
            //Arrange
            var user = new User
            {
                Id = 1,
                Name = "Rajneesh",
                Email = "rajneesh@test.com",
                RoleId = 1,
                CreatedAt = DateTime.UtcNow
            };

            _userRepositoryMock
                .Setup(x => x.GetByIdAsync(1))
                .ReturnsAsync(user);

            //Act
            var result = await _userService.GetUserByIdAsync(1);

            //Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.Id);
            Assert.Equal("Rajneesh", result.Name);
            Assert.Equal("rajneesh@test.com", result.Email);
            Assert.Equal(1, result.RoleId);

            _userRepositoryMock
                .Verify(x => x.GetByIdAsync(1), Times.Once);
        }

        [Fact]
        public async Task GetUserByIdAsync_WhenUserNotExist_ThrowsNotFoundException()
        {
            // Arrange
            _userRepositoryMock
                .Setup(x => x.GetByIdAsync(999))
                .ReturnsAsync((User?)null);

            //Act
            await Assert.ThrowsAsync<NotFoundException>(
                () => _userService.GetUserByIdAsync(999));

            //Assert
            _userRepositoryMock.Verify(
                x => x.GetByIdAsync(999), 
                Times.Once);
        }

        [Fact]
        public async Task GetUsersAsync_WhenUsersExist_ReturnUsers()
        {
            // Arrange
            List<User> users = new List<User>
            {
                new User
                {   Id = 1,
                    Name = "Rajneesh",
                    Email = "rajneesh@test.com",
                    RoleId = 1,
                    CreatedAt = DateTime.UtcNow
                },
                new User
                {
                    Id = 2,
                    Name = "John",
                    Email = "john@test.com",
                    RoleId = 2,
                    CreatedAt = DateTime.UtcNow
                }
            };

                _userRepositoryMock
                    .Setup(x => x.GetAllAsync())
                    .ReturnsAsync(users);

            // Act
            var results = await _userService.GetUsersAsync();

            // Assert
            Assert.NotNull(results);
            Assert.Equal(2, results.Count);
            Assert.Equal("Rajneesh", results[0].Name);
            Assert.Equal("John", results[1].Name);

            _userRepositoryMock
                .Verify(x => x.GetAllAsync(), Times.Once);
        }

        [Fact]
        public async Task GetUsersAsync_WhenUsersDoNotExist_ReturnEmptyList()
        {
            // Arrange
            

            _userRepositoryMock
                .Setup(x => x.GetAllAsync())
                .ReturnsAsync(new List<User>());

            // Act
            var results = await _userService.GetUsersAsync();

            // Assert
            Assert.NotNull(results);
            Assert.Empty(results);

            _userRepositoryMock
                .Verify(x => x.GetAllAsync(), Times.Once);
        }

        [Fact]
        public async Task CreateUserAsync_WhenRoleIsValid_CreateUserAndReturnUser()
        {
            // Arrange
            var dto = new CreateUserDto
            {
                Name = "Rajneesh",
                Email = "rajneesh@test.com",
                Password = "TestPassword123",
                RoleId = 1
            };

            var role = new Role
            {
                Id = 1
            };

            _roleRepositoryMock
                .Setup(x => x.GetByIdAsync(dto.RoleId))
                .ReturnsAsync(role);

            // Act
            var result = await _userService.CreateUserAsync(dto);

            //Assert
            Assert.NotNull(result);
            Assert.Equal(dto.Name, result.Name);
            Assert.Equal(dto.Email, result.Email);
            Assert.Equal(dto.RoleId, result.RoleId);

            _roleRepositoryMock
                .Verify(x => x.GetByIdAsync(dto.RoleId), Times.Once);

            _userRepositoryMock
                .Verify(
                    x => x.AddAsync(It.IsAny<User>()),
                Times.Once);
        }

        [Fact]
        public async Task CreateUserAsync_WhenRoleIsInvalid_ThrowsBadRequestException()
        {
            // Arrange
            var dto = new CreateUserDto
            {
                Name = "Rajneesh",
                Email = "rajneesh@test.com",
                Password = "TestPassword123",
                RoleId = 999
            };

            _roleRepositoryMock
                .Setup(x => x.GetByIdAsync(dto.RoleId))
                .ReturnsAsync((Role?)null);

            // Act & Assert
            await Assert.ThrowsAsync<BadRequestException>(
                () => _userService.CreateUserAsync(dto));

            _roleRepositoryMock
                .Verify(
                    x => x.GetByIdAsync(dto.RoleId),
                    Times.Once);

            _userRepositoryMock
                .Verify(
                    x => x.AddAsync(It.IsAny<User>()),
                    Times.Never);
        }

        [Fact]
        public async Task UpdateUserAsync_WhenUserAndRoleExist_UpdateUserAndReturnUser()
        {
            // Arrange
            var user = new User
            {
                Id = 1,
                Name = "Old Name",
                Email = "old@test.com",
                RoleId = 1,
                CreatedAt = DateTime.UtcNow
            };

            var dto = new UpdateUserDto
            {
                Name = "Rajneesh",
                Email = "rajneesh@test.com",
                RoleId = 2
            };

            var role = new Role
            {
                Id = 2
            };

            _userRepositoryMock
                .Setup(x => x.GetByIdAsync(1))
                .ReturnsAsync(user);

            _roleRepositoryMock
                .Setup(x => x.GetByIdAsync(dto.RoleId))
                .ReturnsAsync(role);

            // Act
            var result = await _userService.UpdateUserAsync(1, dto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(dto.Name, result.Name);
            Assert.Equal(dto.Email, result.Email);
            Assert.Equal(dto.RoleId, result.RoleId);

            _userRepositoryMock
                .Verify(x => x.GetByIdAsync(1), Times.Once);

            _roleRepositoryMock
                .Verify(x => x.GetByIdAsync(dto.RoleId), Times.Once);

            _userRepositoryMock.Verify(
                x => x.UpdateAsync(It.Is<User>(u =>
                u.Id == 1 &&
                u.Name == dto.Name &&
                u.Email == dto.Email &&
                u.RoleId == dto.RoleId)),
                Times.Once);
        }

        [Fact]
        public async Task UpdateUserAsync_WhenUserDoesNotExist_ThrowsNotFoundException()
        {
            // Arrange
            var dto = new UpdateUserDto
            {
                Name = "Rajneesh",
                Email = "rajneesh@test.com",
                RoleId = 2
            };

            // Setup repository to return null
            _userRepositoryMock
                .Setup(x => x.GetByIdAsync(1))
                .ReturnsAsync((User?)null);

            // Act & Assert
            await Assert.ThrowsAnyAsync<NotFoundException>(
                () => _userService.UpdateUserAsync(1, dto));

            // Verify user repository was called
            _userRepositoryMock
                .Verify(x => x.GetByIdAsync(1), 
                Times.Once);

            // Verify role repository was never called
            _roleRepositoryMock
                .Verify(x => x.GetByIdAsync(dto.RoleId), 
                Times.Never);

            // Verify update was never called
            _userRepositoryMock.Verify(
                 x => x.UpdateAsync(It.IsAny<User>()),
                 Times.Never);

        }

        [Fact]
        public async Task UpdateUserAsync_WhenRoleDoesNotExist_ThrowsBadRequestException()
        {
            // Arrange
            var user = new User
            {
                Id = 1,
                Name = "Rajneesh",
                Email = "rajneesh@test.com",
                RoleId = 1,
                CreatedAt = DateTime.UtcNow
            };

            var dto = new UpdateUserDto
            {
                Name = "Updated Name",
                Email = "updated@test.com",
                RoleId = 999
            };

            _userRepositoryMock
                .Setup(x => x.GetByIdAsync(1))
                .ReturnsAsync(user);

            _roleRepositoryMock
                .Setup(x => x.GetByIdAsync(999))
                .ReturnsAsync((Role?)null);

            // Act & Assert
            await Assert.ThrowsAsync<BadRequestException>(
                () => _userService.UpdateUserAsync(1, dto));

            // Verify
            _userRepositoryMock
                .Verify(x => x.GetByIdAsync(1), Times.Once);

            _roleRepositoryMock
                .Verify(x => x.GetByIdAsync(999), Times.Once);

            _userRepositoryMock
                .Verify(
                    x => x.UpdateAsync(It.IsAny<User>()),
                    Times.Never);
        }

        [Fact]
        public async Task PatchUserAsync_WhenUserAndRoleExist_UpdateUserAndReturnUser()
        {
            // Arrange
            var user = new User
            {
                Id = 1,
                Name = "Old Name",
                Email = "old@test.com",
                RoleId = 1,
                CreatedAt = DateTime.UtcNow
            };

            var dto = new PatchUserDto
            {
                Name = "Rajneesh",
                Email = "rajneesh@test.com",
                RoleId = 2
            };

            var role = new Role
            {
                Id = 2
            };

            _userRepositoryMock
                .Setup(x => x.GetByIdAsync(1))
                .ReturnsAsync(user);

            _roleRepositoryMock
                .Setup(x => x.GetByIdAsync(2))
                .ReturnsAsync(role);

            // Act
            var result = await _userService.PatchUserAsync(1, dto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(dto.Name, result.Name);
            Assert.Equal(dto.Email, result.Email);
            Assert.Equal(dto.RoleId, result.RoleId);

            _userRepositoryMock
                .Verify(x => x.GetByIdAsync(1), Times.Once);

            _roleRepositoryMock
                .Verify(x => x.GetByIdAsync(2), Times.Once);

            _userRepositoryMock.Verify(
                x => x.UpdateAsync(It.Is<User>(u =>
                    u.Id == 1 &&
                    u.Name == dto.Name &&
                    u.Email == dto.Email &&
                    u.RoleId == dto.RoleId)),
                Times.Once);
        }

        [Fact]
        public async Task PatchUserAsync_WhenUserDoesNotExist_ThrowsNotFoundException()
        {
            // Arrange
            var dto = new PatchUserDto
            {
                Name = "Rajneesh",
                Email = "rajneesh@test.com",
                RoleId = 2
            };

            _userRepositoryMock
                .Setup(x => x.GetByIdAsync(1))
                .ReturnsAsync((User?)null);

            // Act & Assert
            await Assert.ThrowsAsync<NotFoundException>(
                () => _userService.PatchUserAsync(1, dto));

            // Verify
            _userRepositoryMock
                .Verify(x => x.GetByIdAsync(1), Times.Once);

            _roleRepositoryMock
                .Verify(
                    x => x.GetByIdAsync(It.IsAny<int>()),
                    Times.Never);

            _userRepositoryMock
                .Verify(
                    x => x.UpdateAsync(It.IsAny<User>()),
                    Times.Never);
        }

        [Fact]
        public async Task PatchUserAsync_WhenRoleDoesNotExist_ThrowsBadRequestException()
        {
            // Arrange
            var user = new User
            {
                Id = 1,
                Name = "Rajneesh",
                Email = "rajneesh@test.com",
                RoleId = 1,
                CreatedAt = DateTime.UtcNow
            };

            var dto = new PatchUserDto
            {
                RoleId = 999
            };

            _userRepositoryMock
                .Setup(x => x.GetByIdAsync(1))
                .ReturnsAsync(user);

            _roleRepositoryMock
                .Setup(x => x.GetByIdAsync(999))
                .ReturnsAsync((Role?)null);

            // Act & Assert
            await Assert.ThrowsAsync<BadRequestException>(
                () => _userService.PatchUserAsync(1, dto));

            // Verify
            _userRepositoryMock
                .Verify(x => x.GetByIdAsync(1), Times.Once);

            _roleRepositoryMock
                .Verify(x => x.GetByIdAsync(999), Times.Once);

            _userRepositoryMock
                .Verify(
                    x => x.UpdateAsync(It.IsAny<User>()),
                    Times.Never);
        }

        [Fact]
        public async Task DeleteUserAsync_WhenUserExists_DeleteUser()
        {
            // Arrange
            var user = new User
            {
                Id = 1,
                Name = "Rajneesh",
                Email = "rajneesh@test.com",
                RoleId = 1,
                CreatedAt = DateTime.UtcNow
            };

            _userRepositoryMock
                .Setup(x => x.GetByIdAsync(1))
                .ReturnsAsync(user);

            // Act
            await _userService.DeleteUserAsync(1);

            // Assert
            _userRepositoryMock
                .Verify(x => x.GetByIdAsync(1), Times.Once);

            _userRepositoryMock
                .Verify(
                    x => x.DeleteAsync(user),
                    Times.Once);
        }

        [Fact]
        public async Task DeleteUserAsync_WhenUserDoesNotExist_ThrowsNotFoundException()
        {
            // Arrange
            _userRepositoryMock
                .Setup(x => x.GetByIdAsync(999))
                .ReturnsAsync((User?)null);

            // Act & Assert
            await Assert.ThrowsAsync<NotFoundException>(
                () => _userService.DeleteUserAsync(999));

            // Verify
            _userRepositoryMock
                .Verify(x => x.GetByIdAsync(999), Times.Once);

            _userRepositoryMock
                .Verify(
                    x => x.DeleteAsync(It.IsAny<User>()),
                    Times.Never);
        }
    }
}
