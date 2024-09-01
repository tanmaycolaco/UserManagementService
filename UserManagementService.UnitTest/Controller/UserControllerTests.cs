using System.Security.Claims;
using Auth0.AuthenticationApi.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using UserManagementService.BL.Services.Interfaces;
using UserManagementService.Controllers;
using UserManagementService.Shared.Models;
using UserManagementService.Shared.Models.Request;
using Xunit;

namespace UserManagementService.UnitTest.Controller;

public class UserControllerTests
{
    private readonly Mock<IUserService> _userServiceMock;
    private readonly UserController _controller;

    public UserControllerTests()
    {
        _userServiceMock = new Mock<IUserService>();
        _controller = new UserController(_userServiceMock.Object);

        // Set up a mock HttpContext for authentication testing
        var httpContext = new DefaultHttpContext();
        var auth = new Mock<IAuthenticationService>();
        auth.Setup(a =>
                a.SignOutAsync(It.IsAny<HttpContext>(), It.IsAny<string>(), It.IsAny<AuthenticationProperties>()))
            .Returns(Task.CompletedTask);

        httpContext.RequestServices = new ServiceCollection()
            .AddSingleton<IAuthenticationService>(auth.Object)
            .BuildServiceProvider();
        _controller.ControllerContext = new ControllerContext()
        {
            HttpContext = httpContext
        };
    }

    [Fact]
    public async Task Register_ValidRequest_ReturnsCreatedAtAction()
    {
        // Arrange
        var registerRequest = new RegisterUserRequest
        {
            /* ... */
        };
        var registeredUser = new User { UserId = Guid.NewGuid() };
        _userServiceMock.Setup(service => service.RegisterUserAsync(registerRequest))
            .ReturnsAsync(registeredUser);

        // Add a mock user to the HttpContext for authorization
        _controller.ControllerContext.HttpContext.User =
            new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, "testuser") }));

        // Act
        var result = await _controller.Register(registerRequest);

        // Assert
        var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result);
        Assert.Equal(nameof(UserController.Register), createdAtActionResult.ActionName);
        Assert.Equal(registeredUser.UserId, createdAtActionResult.RouteValues["id"]);
        Assert.Equal(registeredUser, createdAtActionResult.Value);
    }

    [Fact]
    public async Task Login_ValidCredentials_ReturnsOkWithToken()
    {
        // Arrange
        var loginRequest = new LoginRequest
        {
            Username = "testuser", // A valid username you expect to exist in your system
            Password = "correctPassword123!" // The correct password associated with the testuser
        };
        
        var tokenResponse = new Auth0.AuthenticationApi.Models.AccessTokenResponse
        {
            AccessToken = "sample_access_token_12345", // A sample access token (you can generate a realistic one if needed)
            TokenType = "Bearer", // The typical token type for OAuth 2.0
            ExpiresIn = 3600, // Token expiration time in seconds (1 hour in this case)
            RefreshToken = "sample_refresh_token_abcde" // A sample refresh token
        };
        
        _userServiceMock.Setup(service => service.LoginAsync(loginRequest.Username, loginRequest.Password))
            .ReturnsAsync(tokenResponse);

        // Act
        var result = await _controller.Login(loginRequest);

        // Assert
        var okObjectResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(StatusCodes.Status200OK, okObjectResult.StatusCode);
    }

    [Fact]
    public async Task Login_InvalidCredentials_ReturnsBadRequest()
    {
        // Arrange
        var loginRequest = new LoginRequest
        {
            /* ... */
        };
        _userServiceMock.Setup(service => service.LoginAsync(loginRequest.Username, loginRequest.Password))
            .ThrowsAsync(new BadHttpRequestException("Invalid credentials", StatusCodes.Status400BadRequest));

        // Act
        var result = await _controller.Login(loginRequest);

        // Assert
        var badRequestObjectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(StatusCodes.Status400BadRequest, badRequestObjectResult.StatusCode);
    }
    
}