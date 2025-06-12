using BCrypt.Net;

namespace InventoryPro.AuthService.Helpers
    {
    /// <summary>
    /// Helper class to generate BCrypt password hashes
    /// Use this to generate correct password hashes for seeding
    /// </summary>
    public static class PasswordHashGenerator
        {
        /// <summary>
        /// Generates BCrypt hash for passwords
        /// </summary>
        /// <param name="password">Plain text password</param>
        /// <returns>BCrypt hashed password</returns>
        public static string GenerateHash(string password)
            {
            //return BCrypt.HashPassword(password, BCrypt.GenerateSalt(12));
            return BCrypt.Net.BCrypt.HashPassword(password, BCrypt.Net.BCrypt.GenerateSalt(12));
            }

        /// <summary>
        /// Method to generate hashes for common passwords
        /// Run this to get the correct hashes for your seed data
        /// </summary>
        public static void GenerateCommonHashes()
            {
            Console.WriteLine("=== PASSWORD HASHES FOR SEEDING ===");
            Console.WriteLine($"admin123: {GenerateHash("admin123")}");
            Console.WriteLine($"user123: {GenerateHash("user123")}");
            Console.WriteLine($"manager123: {GenerateHash("manager123")}");
            Console.WriteLine("====================================");
            }

        /// <summary>
        /// Verify a password against a hash
        /// </summary>
        /// <param name="password">Plain text password</param>
        /// <param name="hash">BCrypt hash</param>
        /// <returns>True if password matches hash</returns>
        public static bool VerifyPassword(string password, string hash)
            {
            //return BCrypt.Verify(password, hash);
            return BCrypt.Net.BCrypt.Verify(password, hash);
            }
        }
    }

// Example usage in Program.cs or during development:
// PasswordHashGenerator.GenerateCommonHashes();