// using System.Reflection;
// using Microsoft.Extensions.Configuration;
// using Moq;
// using Npgsql;
// using UserManagementService.DL.Helper;
// using UserManagementService.DL.Repositories;
// using UserManagementService.Shared.Models;
// using Xunit;
//
// namespace UserManagementService.UnitTest.DL;
//
// public class UserRepositoryTests
// {
//     private readonly Mock<IConfiguration> _configurationMock;
//     private readonly Mock<PostgresDatabaseHelper> _dbHelperMock;
//     private readonly UserRepository _userRepository;
//     
//     public UserRepositoryTests()
//     {
//         _configurationMock = new Mock<IConfiguration>();
//         _configurationMock.Setup(config => config.GetConnectionString("DefaultConnection"))
//             .Returns("your_test_connection_string");
//
//         _dbHelperMock = new Mock<PostgresDatabaseHelper>("your_test_connection_string"); // Pass the connection string to the mock constructor
//         _userRepository = new UserRepository(_configurationMock.Object);
//
//         // Use reflection to override the _dbHelper field with the mock
//         var dbHelperField = typeof(UserRepository).GetField("_dbHelper", BindingFlags.NonPublic | BindingFlags.Instance);
//         dbHelperField?.SetValue(_userRepository, _dbHelperMock.Object);
//     }
//     
//     [Fact]
//     public async Task CreateUserAsync_ValidUser_ReturnsCreatedUser()
//     {
//         // Arrange
//         var user = new User
//         {
//             UserId = Guid.NewGuid(),
//             Username = "testuser",
//             PasswordHash = "hashed_password",
//             Email = "testuser@example.com",
//             Roles = new List<string> { "user" } // Assuming roles are strings for now
//         };
//
//         _dbHelperMock.Setup(db => db.Transact(It.IsAny<Func<NpgsqlConnection, Task<User>>>()))
//             .ReturnsAsync();
//         
//         _dbHelperMock.Setup(db => db.ExecuteAsync(It.IsAny<string>(), It.IsAny<object>()))
//             .ReturnsAsync(1); // Simulate successful execution of the UserRoles insert
//
//         // Act
//         var result = await _userRepository.CreateUserAsync(user);
//
//         // Assert
//         Assert.Equal(user, result);
//         _dbHelperMock.Verify(db => db.Transact(It.IsAny<Func<NpgsqlConnection, Task<User>>>()), Times.Once);
//         _dbHelperMock.Verify(db => db.ExecuteAsync(It.IsAny<string>(), It.IsAny<object>()), Times.Once); 
//     }
// }