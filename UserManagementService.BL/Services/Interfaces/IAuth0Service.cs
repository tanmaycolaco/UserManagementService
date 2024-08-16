
using Auth0.AuthenticationApi.Models;
using UserManagementService.Shared.Models.Request;

namespace UserManagementService.BL.Services.Interfaces;

public interface IAuth0Service
{
    Task RegisterUserAsync(RegisterUserRequest user);
    
    Task<AccessTokenResponse> GetTokenAsync(string username, string password);
    
    Task LogoutAsync(string refreshToken);
}