using UserManagementService.BL.Services.Interfaces;

namespace UserManagementService.BL.Services;

using Auth0.AuthenticationApi;
using Auth0.AuthenticationApi.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;

public class Auth0TokenFetcher : ITokenFetcher
{
    private readonly IMemoryCache _cache;
    private readonly IConfiguration _configuration;

    public Auth0TokenFetcher(IConfiguration configuration, IMemoryCache cache)
    {
        _configuration = configuration;
        _cache = cache;
    }

    public async Task<string> GetAccessTokenAsync()
    {
        // Check if the token is already in the cache and not expired
        if (_cache.TryGetValue("Auth0AccessToken", out string accessToken))
        {
            return accessToken; 
        }

        // If not in cache or expired, fetch a new token
        var domain = _configuration["Auth0:Domain"];
        var clientId = _configuration["Auth0:ClientId"];
        var clientSecret = _configuration["Auth0:ClientSecret"];

        var authenticationApiClient = new AuthenticationApiClient(new Uri($"https://{domain}"));
        var tokenRequest = new ClientCredentialsTokenRequest
        {
            ClientId = clientId,
            ClientSecret = clientSecret,
            Audience = $"https://{domain}/api/v2/" 
        };
        var tokenResponse = await authenticationApiClient.GetTokenAsync(tokenRequest);

        accessToken = tokenResponse.AccessToken;

        // Cache the new token
        var cacheEntryOptions = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(23) 
        };
        _cache.Set("Auth0AccessToken", accessToken, cacheEntryOptions);

        return accessToken;
    }

    public async Task<AccessTokenResponse> GetTokenAsync(string username, string password)
    {
        // Cache key includes the username
        var cacheKey = $"AuthToken_{username}"; 

        // Try to get the token from the cache first
        if (_cache.TryGetValue(cacheKey, out AccessTokenResponse cachedToken))
        {
            // Check if the token is about to expire (e.g., within 5 minutes)
            if (cachedToken.ExpiresIn > 300) // 300 seconds = 5 minutes
            {
                return cachedToken;
            }
        }

        // If not in cache or about to expire, fetch a new token
        var domain = _configuration["Auth0:Domain"];
        var clientId = _configuration["Auth0:ClientId"];
        var clientSecret = _configuration["Auth0:ClientSecret"];

        var authenticationApiClient = new AuthenticationApiClient(new Uri($"https://{domain}"));
        var request = new ResourceOwnerTokenRequest()
        {
            ClientId = clientId,
            ClientSecret = clientSecret,
            Username = username,
            Password = password,
            Realm = "Username-Password-Authentication" ,
            Audience = $"https://{domain}/api/v2/" ,
            Scope = "openid profile email"
        };

        var tokenResponse = await authenticationApiClient.GetTokenAsync(request);

        // Cache the new token with an expiration slightly before the actual expiration
        var cacheEntryOptions = new MemoryCacheEntryOptions
        {
            AbsoluteExpiration = DateTimeOffset.UtcNow.AddSeconds(tokenResponse.ExpiresIn - 300) // Cache for 5 minutes less than the actual expiration
        };
        _cache.Set(cacheKey, tokenResponse, cacheEntryOptions);

        return tokenResponse;
    }
}