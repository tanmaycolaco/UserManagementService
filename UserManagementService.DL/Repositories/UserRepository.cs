using Dapper;
using Microsoft.Extensions.Configuration;
using UserManagementService.DL.Helper;
using UserManagementService.DL.Repositories.Interfaces;
using UserManagementService.Shared.Models;
using UserManagementService.Shared.Models.Request;

namespace UserManagementService.DL.Repositories;

public class UserRepository : IUserRepository
{
    private readonly PostgresDatabaseHelper _dbHelper;

    public UserRepository(IConfiguration configuration)
    {
        _dbHelper = new PostgresDatabaseHelper(configuration.GetConnectionString("DefaultConnection"));
    }
    const string RegisterNewUserSql = @"
            INSERT INTO Users (UserId, Username, PasswordHash, Email, CreatedAt, UpdatedAt)
            VALUES (@UserId, @Username, @PasswordHash, @Email, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP)
            RETURNING *; 
        ";
    
    const string GetUserByUsernameSql = @"
            SELECT * FROM Users 
            WHERE Username = @Username;
        ";

    public async Task<User> CreateUserAsync(User user)
    {
        return await _dbHelper.Transact(async connection =>
        {
            // 1. Insert the user into the Users table
            var createdUser = await connection.QuerySingleAsync<User>(RegisterNewUserSql, user);

            if (user.Roles != null && user.Roles.Any())
            {
                const string insertUserRolesSql = @"
                    INSERT INTO UserRoles (userId, roleId, createdAt, updatedAt)
                    SELECT @userId, roleId, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP
                    FROM Roles 
                    WHERE roleName = ANY(@roleNames); 
                ";

                // Extract roleNames from the User object
                var roleNames = user.Roles.Select(x => x).ToArray();

                await connection.ExecuteAsync(insertUserRolesSql, new { userId = createdUser.UserId, roleNames });
            }

            return createdUser;
        });
    }
    
    public async Task<User> GetUserByUsernameAsync(string username)
    {
        return await _dbHelper.QueryDatabaseWithResult<User>(
            async connection => await connection.QuerySingleOrDefaultAsync<User>(GetUserByUsernameSql, new { Username = username })
        );
    }
    
    public async Task<bool> CheckIfEmailOrUsernameExistsAsync(string email, string username)
    {
        const string sql = @"
            SELECT EXISTS (
                SELECT 1 FROM Users 
                WHERE Email = @Email OR Username = @Username
            );
        ";
        
        return await _dbHelper.QueryDatabaseWithResult<bool>(
            async connection => await connection.ExecuteScalarAsync<bool>(sql, new { Email = email, Username = username })
        );
    }
}