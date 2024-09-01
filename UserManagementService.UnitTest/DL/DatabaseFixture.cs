using Dapper;
using Microsoft.Extensions.Configuration;
using UserManagementService.DL.Helper;
using UserManagementService.Shared.Models;

namespace UserManagementService.UnitTest.DL;

public class DatabaseFixture : IDisposable
{
    public IConfiguration Configuration { get; }
    private readonly PostgresDatabaseHelper _dbHelper;

    public DatabaseFixture()
    {
        var builder = new ConfigurationBuilder()
            .SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), "..\\..\\..\\"))
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);; 
        Configuration = builder.Build();

        _dbHelper = new PostgresDatabaseHelper(Configuration.GetConnectionString("DefaultConnection"));
    }

    public async Task<User> CreateUser(string username, string email)
    {
        var newUser = new User
        {
            UserId = Guid.NewGuid(),
            Username = username,
            PasswordHash = "hashed_password",
            Email = email
        };

        return await _dbHelper.Transact(async connection =>
        {
            return await connection.QuerySingleAsync<User>(
                @"INSERT INTO Users (UserId, Username, PasswordHash, Email, CreatedAt, UpdatedAt)
                      VALUES (@UserId, @Username, @PasswordHash, @Email, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP)
                      RETURNING *;", newUser);
        });
    }

    public async Task DeleteUser(string username)
    {
        await _dbHelper.Execute(
            @"DELETE FROM Users WHERE Username = @Username", new { Username = username });
    }

    public void Dispose()
    {
        // Any additional cleanup if needed
    }
}