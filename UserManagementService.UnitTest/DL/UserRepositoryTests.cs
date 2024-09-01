using System.Reflection;
using Microsoft.Extensions.Configuration;
using Moq;
using Npgsql;
using UserManagementService.DL.Helper;
using UserManagementService.DL.Repositories;
using UserManagementService.DL.Repositories.Interfaces;
using UserManagementService.Shared.Models;
using Xunit;

namespace UserManagementService.UnitTest.DL;

   public class UserRepositoryTests : IClassFixture<DatabaseFixture>
    {
        private readonly IUserRepository _userRepository;
        private readonly DatabaseFixture _fixture;

        public UserRepositoryTests(DatabaseFixture fixture)
        {
            _fixture = fixture;
            _userRepository = new UserRepository(fixture.Configuration);
        }

        [Fact]
        public async Task CreateUserAsync_CreatesUserWithNoRoles()
        {
            // Arrange
            var newUser = new User
            {
                UserId = Guid.NewGuid(),
                Username = "testuser1",
                PasswordHash = "hashed_password",
                Email = "testuser1@example.com"
            };
            
            await _fixture.DeleteUser(newUser.Username);

            // Act
            var createdUser = await _userRepository.CreateUserAsync(newUser);

            // Assert
            Assert.NotNull(createdUser);
            Assert.Equal(newUser.UserId, createdUser.UserId);
            Assert.Equal(newUser.Username, createdUser.Username);
            Assert.Equal(newUser.Email, createdUser.Email);
            Assert.Null(createdUser.Roles); 

            // Cleanup
            await _fixture.DeleteUser(newUser.Username);
        }

        [Fact]
        public async Task CreateUserAsync_CreatesUserWithRoles()
        {
            // Arrange (ensure 'Admin' and 'User' roles exist in your test DB)
            var newUser = new User
            {
                UserId = Guid.NewGuid(),
                Username = "testuser2",
                PasswordHash = "hashed_password",
                Email = "testuser2@example.com",
                Roles = new List<string>() { "Admin", "User" } 
            };
            
            await _fixture.DeleteUser(newUser.Username);

            // Act
            var createdUser = await _userRepository.CreateUserAsync(newUser);

            // Assert
            Assert.NotNull(createdUser);
            Assert.Equal(newUser.UserId, createdUser.UserId);

            // Cleanup
            await _fixture.DeleteUser(newUser.Username);
        }

        [Fact]
        public async Task GetUserByUsernameAsync_ExistingUser_ReturnsUser()
        {
            await _fixture.DeleteUser("testuser3");
            // Arrange
            var existingUser = await _fixture.CreateUser("testuser3", "testuser3@example.com");

            // Act
            var retrievedUser = await _userRepository.GetUserByUsernameAsync(existingUser.Username);

            // Assert
            Assert.NotNull(retrievedUser);
            Assert.Equal(existingUser.UserId, retrievedUser.UserId);
        }

        [Fact]
        public async Task GetUserByUsernameAsync_NonexistentUser_ReturnsNull()
        {
            // Act
            var retrievedUser = await _userRepository.GetUserByUsernameAsync("nonexistentuser");

            // Assert
            Assert.Null(retrievedUser);
        }

        [Fact]
        public async Task CheckIfEmailOrUsernameExistsAsync_ExistingEmail_ReturnsTrue()
        {
            await _fixture.DeleteUser("testuser4");
            // Arrange
            var existingUser = await _fixture.CreateUser("testuser4", "testuser4@example.com");

            // Act
            var exists = await _userRepository.CheckIfEmailOrUsernameExistsAsync(existingUser.Email, "someotherusername");

            // Assert
            Assert.True(exists);

            // Cleanup
            await _fixture.DeleteUser("testuser4");
        }

        [Fact]
        public async Task CheckIfEmailOrUsernameExistsAsync_ExistingUsername_ReturnsTrue()
        {
            await _fixture.DeleteUser("testuser5");
            // Arrange
            var existingUser = await _fixture.CreateUser("testuser5", "testuser5@example.com");

            // Act
            var exists = await _userRepository.CheckIfEmailOrUsernameExistsAsync("someotheremail@example.com", existingUser.Username);

            // Assert
            Assert.True(exists);

            // Cleanup
            await _fixture.DeleteUser("testuser5");
        }

        [Fact]
        public async Task CheckIfEmailOrUsernameExistsAsync_Nonexistent_ReturnsFalse()
        {
            // Act
            var exists = await _userRepository.CheckIfEmailOrUsernameExistsAsync("nonexistentemail@example.com", "nonexistentuser");

            // Assert
            Assert.False(exists);
        }
    }