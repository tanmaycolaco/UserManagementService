using Auth0.AuthenticationApi.Models;
using UserManagementService.Shared.Models;
using UserManagementService.Shared.Models.Request;

namespace UserManagementService.BL.Services.Interfaces;


public interface IUserService
{
    Task<User> RegisterUserAsync(RegisterUserRequest user);

    Task<AccessTokenResponse> LoginAsync(string username, string password);
    
    Task LogoutAsync(string refreshToken);
}