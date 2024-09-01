using Auth0.AuthenticationApi;
using Auth0.AuthenticationApi.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Moq;
using UserManagementService.BL.Services;
using Xunit;
using NSubstitute;

namespace UserManagementService.UnitTest.BL;

   public class Auth0TokenFetcherTests
    {
        private readonly IConfiguration _configuration;
        private readonly IMemoryCache _cache;
        private readonly IAuthenticationApiClient _authenticationApiClient;

        public Auth0TokenFetcherTests()
        {
            _configuration = Substitute.For<IConfiguration>();
            _cache = Substitute.For<IMemoryCache>();
            _authenticationApiClient = Substitute.For<IAuthenticationApiClient>();
        }

        [Fact]
        public async Task GetAccessTokenAsync_TokenInCache_ReturnsCachedToken()
        {
            // Arrange
            _configuration["Auth0:Domain"].Returns("testdomain");
            _configuration["Auth0:ClientId"].Returns("testclientid");
            _configuration["Auth0:ClientSecret"].Returns("testclientsecret");
            _cache.TryGetValue("Auth0AccessToken", out Arg.Any<string>()).Returns(x =>
            {
                x[1] = "cachedtoken";
                return true;
            });

            var auth0TokenFetcher = new Auth0TokenFetcher(_configuration, _cache, _authenticationApiClient);

            // Act
            var accessToken = await auth0TokenFetcher.GetAccessTokenAsync();

            // Assert
            Assert.Equal("cachedtoken", accessToken);
            await _authenticationApiClient.DidNotReceive().GetTokenAsync(Arg.Any<ClientCredentialsTokenRequest>());
        }

        [Fact]
        public async Task GetAccessTokenAsync_TokenNotInCache_FetchesNewToken()
        {
            // Arrange
            _configuration["Auth0:Domain"].Returns("testdomain");
            _configuration["Auth0:ClientId"].Returns("testclientid");
            _configuration["Auth0:ClientSecret"].Returns("testclientsecret");
            _cache.TryGetValue("Auth0AccessToken", out Arg.Any<string>()).Returns(false);
            _authenticationApiClient.GetTokenAsync(Arg.Any<ClientCredentialsTokenRequest>()).Returns(new AccessTokenResponse()
            {
                AccessToken = "newtoken"
            });

            var auth0TokenFetcher = new Auth0TokenFetcher(_configuration, _cache, _authenticationApiClient);

            // Act
            var accessToken = await auth0TokenFetcher.GetAccessTokenAsync();

            // Assert
            Assert.Equal("newtoken", accessToken);
            _cache.Received().Set("Auth0AccessToken", "newtoken");
        }

        [Fact]
        public async Task GetTokenAsync_TokenInCacheAndNotExpired_ReturnsCachedToken()
        {
            // Arrange
            var username = "testuser";
            var password = "testpassword";
            var cachedToken = new AccessTokenResponse { ExpiresIn = 3600 }; // 1 hour
            _configuration["Auth0:Domain"].Returns("testdomain");
            _configuration["Auth0:ClientId"].Returns("testclientid");
            _configuration["Auth0:ClientSecret"].Returns("testclientsecret");
            _cache.TryGetValue($"AuthToken_{username}", out Arg.Any<AccessTokenResponse>()).Returns(x =>
            {
                x[1] = cachedToken;
                return true;
            });

            var auth0TokenFetcher = new Auth0TokenFetcher(_configuration, _cache, _authenticationApiClient);

            // Act
            var tokenResponse = await auth0TokenFetcher.GetTokenAsync(username, password);

            // Assert
            Assert.Same(cachedToken, tokenResponse);
            await _authenticationApiClient.DidNotReceive().GetTokenAsync(Arg.Any<ResourceOwnerTokenRequest>());
        }

        [Fact]
        public async Task GetTokenAsync_TokenInCacheButExpired_FetchesNewToken()
        {
            // Arrange
            var username = "testuser";
            var password = "testpassword";
            var expiredToken = new AccessTokenResponse { ExpiresIn = 100 }; // Less than 5 minutes
            var newToken = new AccessTokenResponse { AccessToken = "newtoken", ExpiresIn = 3600 };
            _configuration["Auth0:Domain"].Returns("testdomain");
            _configuration["Auth0:ClientId"].Returns("testclientid");
            _configuration["Auth0:ClientSecret"].Returns("testclientsecret");
            _cache.TryGetValue($"AuthToken_{username}", out Arg.Any<AccessTokenResponse>()).Returns(x =>
            {
                x[1] = expiredToken;
                return true;
            });
            _authenticationApiClient.GetTokenAsync(Arg.Any<ResourceOwnerTokenRequest>()).Returns(newToken);

            var auth0TokenFetcher = new Auth0TokenFetcher(_configuration, _cache, _authenticationApiClient);

            // Act
            var tokenResponse = await auth0TokenFetcher.GetTokenAsync(username, password);

            // Assert
            Assert.Same(newToken, tokenResponse);
            _cache.Received().Set($"AuthToken_{username}", newToken);
        }

        [Fact]
        public async Task GetTokenAsync_TokenNotInCache_FetchesNewToken()
        {
            // Arrange
            var username = "testuser";
            var password = "testpassword";
            var newToken = new AccessTokenResponse { AccessToken = "newtoken", ExpiresIn = 3600 };
            _configuration["Auth0:Domain"].Returns("testdomain");
            _configuration["Auth0:ClientId"].Returns("testclientid");
            _configuration["Auth0:ClientSecret"].Returns("testclientsecret");
            _cache.TryGetValue($"AuthToken_{username}", out Arg.Any<AccessTokenResponse>()).Returns(false);
            _authenticationApiClient.GetTokenAsync(Arg.Any<ResourceOwnerTokenRequest>()).Returns(newToken);

            var auth0TokenFetcher = new Auth0TokenFetcher(_configuration, _cache, _authenticationApiClient);

            // Act
            var tokenResponse = await auth0TokenFetcher.GetTokenAsync(username, password);

            // Assert
            Assert.Same(newToken, tokenResponse);
            _cache.Received().Set($"AuthToken_{username}", newToken);
        }

        // ... additional test cases for error scenarios, configuration issues, etc.
    }