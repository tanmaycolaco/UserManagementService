using System.Net;
using Auth0.AuthenticationApi.Models;
using Auth0.Core.Exceptions;
using Auth0.ManagementApi;
using Auth0.ManagementApi.Models;
using Microsoft.Extensions.Configuration;
using Moq;
using UserManagementService.BL.Services;
using UserManagementService.BL.Services.Interfaces;
using UserManagementService.Shared.Models.Request;
using Xunit;

namespace UserManagementService.UnitTest.BL;

public class Auth0ServiceTests
{
    private readonly Mock<ITokenFetcher> _tokenFetcherMock;
    private readonly Mock<IConfiguration> _configurationMock;
    private readonly Auth0Service _auth0Service;

    public Auth0ServiceTests()
    {
        _tokenFetcherMock = new Mock<ITokenFetcher>();
        _configurationMock = new Mock<IConfiguration>();
        _auth0Service = new Auth0Service(_configurationMock.Object, _tokenFetcherMock.Object);
    }

    [Fact]
    public async Task GetTokenAsync_Success_ReturnsTokenFromFetcher()
    {
        // Arrange
        var username = "testuser";
        var password = "StrongPassword123!";
        var expectedTokenResponse = new AccessTokenResponse 
        {
            AccessToken = "sample_access_token",
            TokenType = "Bearer",
            ExpiresIn = 3600
        };

        _tokenFetcherMock.Setup(fetcher => fetcher.GetTokenAsync(username, password))
            .ReturnsAsync(expectedTokenResponse);

        // Act
        var result = await _auth0Service.GetTokenAsync(username, password);

        // Assert
        Assert.Equal(expectedTokenResponse, result);
        _tokenFetcherMock.Verify(fetcher => fetcher.GetTokenAsync(username, password), Times.Once);
    }

    [Fact]
    public async Task RegisterUserAsync_InvalidToken_ThrowsApiException()
    {
        // Arrange
        var request = new RegisterUserRequest { /* ... */ };

        _configurationMock.SetupGet(config => config["Auth0:Domain"]).Returns("your-auth0-domain.auth0.com");
        
        _tokenFetcherMock.Setup(fetcher => fetcher.GetAccessTokenAsync())
            .ThrowsAsync(new ErrorApiException(HttpStatusCode.Unauthorized));

        // Act & Assert
        await Assert.ThrowsAsync<ErrorApiException>(() => _auth0Service.RegisterUserAsync(request));
    }
    

    [Fact]
    public async Task GetTokenAsync_ApiException_ThrowsApiException()
    {
        // Arrange
        var username = "testuser";
        var password = "StrongPassword123!";

        //var mockResponse = new HttpResponseMessage((HttpStatusCode)401);
        _tokenFetcherMock.Setup(fetcher => fetcher.GetTokenAsync(username, password))
            .ThrowsAsync(new ErrorApiException(HttpStatusCode.Unauthorized));

        // Act & Assert
        await Assert.ThrowsAsync<ErrorApiException>(async () => await _auth0Service.GetTokenAsync(username, password));
    }
}