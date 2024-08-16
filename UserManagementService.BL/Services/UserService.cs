using System.Text.RegularExpressions;
using Auth0.AuthenticationApi.Models;
using Microsoft.AspNetCore.Http;
using UserManagementService.DL.Repositories.Interfaces;
using UserManagementService.Shared.Utils;

namespace UserManagementService.BL.Services;

using UserManagementService.BL.Services.Interfaces;
using UserManagementService.Shared.Models;
using UserManagementService.Shared.Models.Request;
using BCrypt.Net;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IAuth0Service _auth0Service;

    public UserService(IUserRepository userRepository, IAuth0Service auth0Service)
    {
        _userRepository = userRepository;
        _auth0Service = auth0Service;
    }

    public async Task<User> RegisterUserAsync(RegisterUserRequest request)
    {
        await ValidateRegisterUserRequest(request);

        // 2. Create User object from the request
        var user = new User
        {
            UserId = Guid.NewGuid(),
            Username = request.Username,
            Email = request.Email,
            PasswordHash = PasswordHasher.HashPassword(request.Password),
            Roles = request.Roles
        };

        // 3. Create user in your database
        var createdUser = await _userRepository.CreateUserAsync(user);

        // 4. Register user with Auth0 (pass the plain-text password here)
        request.Password = user.PasswordHash; // Update the password in the request for Auth0
        await _auth0Service.RegisterUserAsync(request); 

        return createdUser;
    }
    
    public async Task<AccessTokenResponse> LoginAsync(string username, string password)
    {
        // 1. Validate username and password
        var user = await _userRepository.GetUserByUsernameAsync(username);
        if (user == null)
        {
            throw new BadHttpRequestException("Invalid username or password.", StatusCodes.Status401Unauthorized);
        }

        if (!PasswordHasher.VerifyPassword(password, user.PasswordHash))
        {
            throw new BadHttpRequestException("Invalid username or password.", StatusCodes.Status401Unauthorized);
        }

        // 2. Generate token using Auth0 service
        return await _auth0Service.GetTokenAsync(user.Email, user.PasswordHash);
    }

    public Task LogoutAsync(string refreshToken)
    {
        throw new NotImplementedException();
    }

    private async Task ValidateRegisterUserRequest(RegisterUserRequest request)
    {
        // 1. Check for mandatory fields
        if (string.IsNullOrWhiteSpace(request.Username))
        {
            throw new BadHttpRequestException("Username is required.", StatusCodes.Status422UnprocessableEntity);
        }

        if (string.IsNullOrWhiteSpace(request.Password))
        {
            throw new BadHttpRequestException("Password is required.", StatusCodes.Status422UnprocessableEntity);
        }

        if (string.IsNullOrWhiteSpace(request.Email))
        {
            throw new BadHttpRequestException("Email is required.", StatusCodes.Status422UnprocessableEntity);
        }
        
        // 2. Check if username is already taken
        if (await _userRepository.CheckIfEmailOrUsernameExistsAsync(request.Email, request.Username)) 
        {
            throw new BadHttpRequestException("Username is already taken.", StatusCodes.Status422UnprocessableEntity);
        }

        // 3. Check for a strong password (adjust rules as needed)
        if (!IsPasswordStrong(request.Password))
        {
            throw new BadHttpRequestException("Password is not strong enough. It should be at least 8 characters long and contain at least one uppercase letter, one lowercase letter, one digit, and one special character.", StatusCodes.Status422UnprocessableEntity);
        }

        // 4. Validate email format using regex
        if (!IsValidEmail(request.Email))
        {
            throw new BadHttpRequestException("Invalid email address.", StatusCodes.Status422UnprocessableEntity);
        }
    }

    private bool IsPasswordStrong(string password)
    {
        var regex = new Regex(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z]).{8,}$");
        return regex.IsMatch(password);
    }

    private bool IsValidEmail(string email)
    {
        var regex = new Regex(@"^[\w-\.]+@([\w-]+\.)+[\w-]{2,4}$");
        return regex.IsMatch(email);
    }
}