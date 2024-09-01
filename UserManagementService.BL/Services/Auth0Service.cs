using Auth0.AuthenticationApi;
using Auth0.AuthenticationApi.Models;
using Auth0.ManagementApi;
using Auth0.ManagementApi.Models;
using UserManagementService.BL.Services.Interfaces;
using UserManagementService.Shared.Models.Request;
using Microsoft.Extensions.Configuration;

namespace UserManagementService.BL.Services;

public class Auth0Service: IAuth0Service
{
    private readonly ITokenFetcher _tokenFetcher;
    private readonly IConfiguration _configuration;
    

    public Auth0Service(IConfiguration configuration, ITokenFetcher tokenFetcher)
    {
        _configuration = configuration;
        _tokenFetcher = tokenFetcher;
    }

    public async Task RegisterUserAsync(RegisterUserRequest user)
    {
        var accessToken = await _tokenFetcher.GetAccessTokenAsync();
        var managementApiClient = new ManagementApiClient(accessToken, _configuration["Auth0:Domain"]);

        var newUser = new UserCreateRequest
        {
            Connection = "Username-Password-Authentication", 
            Email = user.Email,
            Password = user.Password, 
        };

        await managementApiClient.Users.CreateAsync(newUser);
    }

    public async Task<AccessTokenResponse> GetTokenAsync(string username, string password)
    {
        return await _tokenFetcher.GetTokenAsync(username, password);
    }

    public async Task LogoutAsync(string refreshToken)
    {
        var domain = _configuration["Auth0:Domain"];
        var clientId = _configuration["Auth0:ClientId"];
        var clientSecret = _configuration["Auth0:ClientSecret"];

        var authenticationApiClient = new AuthenticationApiClient(new Uri($"https://{domain}"));
        
        // Revoke the refresh token
        await authenticationApiClient.RevokeRefreshTokenAsync(new RevokeRefreshTokenRequest
        {
            ClientId = clientId,
            ClientSecret = clientSecret,
            RefreshToken = refreshToken
        });

    }
}