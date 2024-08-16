using UserManagementService.Shared.Models;
using UserManagementService.Shared.Models.Request;

namespace UserManagementService.DL.Repositories.Interfaces;

public interface IUserRepository
{
    Task<User> CreateUserAsync(User user);
    
    Task<User> GetUserByUsernameAsync(string username);

    Task<bool> CheckIfEmailOrUsernameExistsAsync(string email, string username);
}