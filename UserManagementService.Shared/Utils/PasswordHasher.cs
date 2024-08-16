namespace UserManagementService.Shared.Utils;

using System;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;


public static class PasswordHasher
{
    // Mimic Spring Security's default settings
    private const int Iterations = 10000; // Number of hashing iterations
    private const int SaltSize = 16; // Size of the salt in bytes
    private const int HashSize = 256 / 8; // Size of the hash in bytes (SHA-256)

    public static string HashPassword(string password)
    {
        // Generate a random salt
        byte[] salt = new byte[SaltSize];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(salt);
        }

        // Hash the password with the salt
        byte[] hash = KeyDerivation.Pbkdf2(
            password: password,
            salt: salt,
            prf: KeyDerivationPrf.HMACSHA256, // Use HMAC-SHA256
            iterationCount: Iterations,
            numBytesRequested: HashSize
        );

        // Combine the salt and hash into a single string
        var hashed = Convert.ToBase64String(salt) + ":" + Convert.ToBase64String(hash);
        return hashed;
    }

    public static bool VerifyPassword(string password, string hashedPassword)
    {
        // Extract the salt and hash from the stored hashed password
        var parts = hashedPassword.Split(':');
        var salt = Convert.FromBase64String(parts[0]);
        var storedHash = Convert.FromBase64String(parts[1]);   


        // Re-hash the provided password with the extracted salt
        var newHash = KeyDerivation.Pbkdf2(
            password: password,
            salt: salt,
            prf: KeyDerivationPrf.HMACSHA256,
            iterationCount: Iterations,
            numBytesRequested: HashSize   

        );

        // Compare the re-hashed password with the stored hash
        return CryptographicOperations.FixedTimeEquals(storedHash, newHash);
    }
}