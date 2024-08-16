
using UserManagementService.Shared.Utils;

namespace UserManagementService.UnitTest.BL;

using System;
using System.Threading.Tasks;
using Auth0.AuthenticationApi.Models;
using Microsoft.AspNetCore.Http;
using Moq;
using Shared.Models;
using Xunit;
using UserManagementService.BL.Services;
using UserManagementService.BL.Services.Interfaces;
using UserManagementService.DL.Repositories.Interfaces;
using UserManagementService.Shared.Models.Request;

public class UserServiceTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IAuth0Service> _auth0ServiceMock;
    private readonly UserService _userService;

    public UserServiceTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _auth0ServiceMock = new Mock<IAuth0Service>();
        _userService = new UserService(_userRepositoryMock.Object, _auth0ServiceMock.Object);
    }

    [Fact]
    public async Task RegisterUserAsync_ValidRequest_CreatesUserAndRegistersWithAuth0()
    {
        // Arrange
        var request = new RegisterUserRequest
        {
            Username = "testuser",
            Password = "StrongPassword123!",
            Email = "testuser@example.com"
        };

        var createdUser = new User
        {
            UserId = Guid.NewGuid(),
            Username = request.Username,
            Email = request.Email,
            PasswordHash = "hashed_password" // Simulate hashed password
        };

        _userRepositoryMock.Setup(repo => repo.CreateUserAsync(It.IsAny<User>()))
            .ReturnsAsync(createdUser);
        _userRepositoryMock.Setup(repo => repo.CheckIfEmailOrUsernameExistsAsync(request.Email, request.Username))
            .ReturnsAsync(false);
        _auth0ServiceMock.Setup(service => service.RegisterUserAsync(It.IsAny<RegisterUserRequest>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _userService.RegisterUserAsync(request);

        // Assert
        Assert.Equal(createdUser, result);
        _userRepositoryMock.Verify(repo => repo.CreateUserAsync(It.IsAny<User>()), Times.Once);
        _auth0ServiceMock.Verify(service => service.RegisterUserAsync(It.IsAny<RegisterUserRequest>()), Times.Once);
    }

    [Fact]
    public async Task RegisterUserAsync_InvalidEmail_ThrowsBadRequest()
    {
        // Arrange
        var request = new RegisterUserRequest
        {
            Username = "testuser",
            Password = "StrongPassword123!",
            Email = "invalid-email" 
        };

        // Act & Assert
        await Assert.ThrowsAsync<BadHttpRequestException>(() => _userService.RegisterUserAsync(request));
    }

    [Fact]
    public async Task RegisterUserAsync_WeakPassword_ThrowsBadRequest()
    {
        // Arrange
        var request = new RegisterUserRequest
        {
            Username = "testuser",
            Password = "weak", // Weak password
            Email = "testuser@example.com"
        };

        // Act & Assert
        await Assert.ThrowsAsync<BadHttpRequestException>(() => _userService.RegisterUserAsync(request));
    }

    [Fact]
    public async Task RegisterUserAsync_DuplicateUsername_ThrowsBadRequest()
    {
        // Arrange
        var request = new RegisterUserRequest
        {
            Username = "existinguser", 
            Password = "StrongPassword123!",
            Email = "testuser@example.com"
        };

        _userRepositoryMock.Setup(repo => repo.CheckIfEmailOrUsernameExistsAsync(request.Email, request.Username))
            .ReturnsAsync(true); // Simulate an existing user

        // Act & Assert
        await Assert.ThrowsAsync<BadHttpRequestException>(() => _userService.RegisterUserAsync(request));
    }
    
    [Fact]
    public async Task RegisterUserAsync_DuplicateEmailOrUsername_ThrowsBadRequest()
    {
        // Arrange
        var request = new RegisterUserRequest
        {
            Username = "testuser",
            Password = "StrongPassword123!",
            Email = "existingemail@example.com" 
        };

        _userRepositoryMock.Setup(repo => repo.CheckIfEmailOrUsernameExistsAsync(request.Email, request.Username))
            .ReturnsAsync(true); // Simulate an existing user with this email or username

        // Act & Assert
        await Assert.ThrowsAsync<BadHttpRequestException>(() => _userService.RegisterUserAsync(request));
    }

    // Add more test cases for other validation scenarios and the LoginAsync method

    [Fact]
    public async Task LoginAsync_ValidCredentials_ReturnsToken()
    {
        // Arrange
        var username = "testuser";
        var password = "StrongPassword123!";
        var hashedPassword = PasswordHasher.HashPassword(password);

        var user = new User
        {
            UserId = Guid.NewGuid(),
            Username = username,
            Email = "testuser@example.com",
            PasswordHash = hashedPassword
        };

        _userRepositoryMock.Setup(repo => repo.GetUserByUsernameAsync(username))
            .ReturnsAsync(user);

        var tokenResponse = new AccessTokenResponse
        {
            AccessToken = "sample_access_token",
            TokenType = "Bearer",
            ExpiresIn = 3600 
        };

        _auth0ServiceMock.Setup(service => service.GetTokenAsync(user.Email, hashedPassword))
            .ReturnsAsync(tokenResponse);

        // Act
        var result = await _userService.LoginAsync(username, password);

        // Assert
        Assert.Equal(tokenResponse, result);
    }

    [Fact]
    public async Task LoginAsync_InvalidUsername_ThrowsUnauthorized()
    {
        // Arrange
        var username = "nonexistentuser";
        var password = "somepassword";

        _userRepositoryMock.Setup(repo => repo.GetUserByUsernameAsync(username))
            .ReturnsAsync((User)null); // Simulate user not found

        // Act & Assert
        await Assert.ThrowsAsync<BadHttpRequestException>(() => _userService.LoginAsync(username, password));
    }

    [Fact]
    public async Task LoginAsync_IncorrectPassword_ThrowsUnauthorized()
    {
        // Arrange
        var username = "testuser";
        var password = "wrongpassword";
        var hashedPassword = PasswordHasher.HashPassword("StrongPassword123!"); // Different password

        var user = new User
        {
            UserId = Guid.NewGuid(),
            Username = username,
            Email = "testuser@example.com",
            PasswordHash = hashedPassword
        };

        _userRepositoryMock.Setup(repo => repo.GetUserByUsernameAsync(username))
            .ReturnsAsync(user);

        // Act & Assert
        await Assert.ThrowsAsync<BadHttpRequestException>(() => _userService.LoginAsync(username, password));
    }
}