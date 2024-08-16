using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UserManagementService.BL.Services.Interfaces;
using UserManagementService.Shared.Models.Request;

namespace UserManagementService.Controllers;

[ApiController]
[Microsoft.AspNetCore.Components.Route("api/v1/[controller]")]
public class UserController: Controller
{
    private readonly IUserService _userService;

    public UserController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpPost("register")]
    [Authorize] 
    public async Task<IActionResult> Register([FromBody] RegisterUserRequest user)
    {
        var registeredUser = await _userService.RegisterUserAsync(user);
        return CreatedAtAction(nameof(Register), new { id = registeredUser.UserId }, registeredUser);
    }
    
    [HttpPost("login")] // Not authorized
    public async Task<IActionResult> Login([FromBody] LoginRequest loginRequest)
    {
        try
        {
            var tokenResponse = await _userService.LoginAsync(loginRequest.Username, loginRequest.Password);
            return Ok(new
            {
                access_token = tokenResponse.AccessToken,
                token_type = tokenResponse.TokenType,
                expires_in = tokenResponse.ExpiresIn,
                refresh_token = tokenResponse.RefreshToken 
            });
        }
        catch (BadHttpRequestException ex)
        {
            return StatusCode(ex.StatusCode, new { error = ex.Message }); 
        }
    }
    
    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(); 
        return Ok(); 
    }
}