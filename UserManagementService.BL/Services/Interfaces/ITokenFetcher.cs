using Auth0.AuthenticationApi.Models;

namespace UserManagementService.BL.Services.Interfaces;

public interface ITokenFetcher
{
    Task<string> GetAccessTokenAsync();
    
    Task<AccessTokenResponse> GetTokenAsync(string username, string password);
}