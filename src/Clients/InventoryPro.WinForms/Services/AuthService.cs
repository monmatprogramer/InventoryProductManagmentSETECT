using System.Text.Json;
using InventoryPro.Shared.DTOs;

namespace InventoryPro.WinForms.Services
    {
    /// <summary>
    /// Implementation of authentication service for Windows Forms client
    /// Manages JWT tokens and user session data using local file storage
    /// </summary>
    public class AuthService : IAuthService
        {
        private readonly string _tokenFilePath;
        private readonly string _userFilePath;
        private string? _currentToken;
        private UserDto? _currentUser;

        public event EventHandler<bool>? AuthenticationStateChanged;

        public AuthService()
            {
            // Create app data directory for storing tokens securely
            var appDataPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "InventoryPro");

            Directory.CreateDirectory(appDataPath);

            _tokenFilePath = Path.Combine(appDataPath, "token.dat");
            _userFilePath = Path.Combine(appDataPath, "user.dat");
            }

        /// <summary>
        /// Gets the stored authentication token
        /// </summary>
        public async Task<string?> GetTokenAsync()
            {
            try
                {
                if (!string.IsNullOrEmpty(_currentToken))
                    return _currentToken;

                if (File.Exists(_tokenFilePath))
                    {
                    var encryptedToken = await File.ReadAllBytesAsync(_tokenFilePath);
                    _currentToken = DecryptData(encryptedToken);
                    return _currentToken;
                    }

                return null;
                }
            catch (Exception)
                {
                // If there's any error reading the token, return null
                return null;
                }
            }

        /// <summary>
        /// Stores the authentication token securely
        /// </summary>
        public async Task SetTokenAsync(string token)
            {
            try
                {
                _currentToken = token;
                var encryptedToken = EncryptData(token);
                await File.WriteAllBytesAsync(_tokenFilePath, encryptedToken);

                // Notify about authentication state change
                AuthenticationStateChanged?.Invoke(this, true);
                }
            catch (Exception ex)
                {
                throw new InvalidOperationException("Failed to store authentication token", ex);
                }
            }

        /// <summary>
        /// Clears the stored authentication token
        /// </summary>
        public async Task ClearTokenAsync()
            {
            try
                {
                _currentToken = null;

                if (File.Exists(_tokenFilePath))
                    File.Delete(_tokenFilePath);

                await ClearCurrentUserAsync();

                // Notify about authentication state change
                AuthenticationStateChanged?.Invoke(this, false);
                }
            catch (Exception)
                {
                // Even if file deletion fails, we've cleared the memory token
                _currentToken = null;
                AuthenticationStateChanged?.Invoke(this, false);
                }
            }

        /// <summary>
        /// Checks if the user is currently authenticated
        /// </summary>
        public async Task<bool> IsAuthenticatedAsync()
            {
            var token = await GetTokenAsync();
            return !string.IsNullOrEmpty(token) && IsTokenValid(token);
            }

        /// <summary>
        /// Gets the current user information
        /// </summary>
        public async Task<UserDto?> GetCurrentUserAsync()
            {
            try
                {
                if (_currentUser != null)
                    return _currentUser;

                if (File.Exists(_userFilePath))
                    {
                    var encryptedUserData = await File.ReadAllBytesAsync(_userFilePath);
                    var userJson = DecryptData(encryptedUserData);
                    _currentUser = JsonSerializer.Deserialize<UserDto>(userJson);
                    return _currentUser;
                    }

                return null;
                }
            catch (Exception)
                {
                return null;
                }
            }

        /// <summary>
        /// Stores the current user information
        /// </summary>
        public async Task SetCurrentUserAsync(UserDto user)
            {
            try
                {
                _currentUser = user;
                var userJson = JsonSerializer.Serialize(user);
                var encryptedUserData = EncryptData(userJson);
                await File.WriteAllBytesAsync(_userFilePath, encryptedUserData);
                }
            catch (Exception ex)
                {
                throw new InvalidOperationException("Failed to store user information", ex);
                }
            }

        /// <summary>
        /// Clears the current user information
        /// </summary>
        public async Task ClearCurrentUserAsync()
            {
            try
                {
                _currentUser = null;

                if (File.Exists(_userFilePath))
                    File.Delete(_userFilePath);

                await Task.CompletedTask;
                }
            catch (Exception)
                {
                // Even if file deletion fails, we've cleared the memory user
                _currentUser = null;
                }
            }

        #region Helper Methods

        /// <summary>
        /// Simple encryption for token storage (in production, use stronger encryption)
        /// </summary>
        private byte[] EncryptData(string data)
            {
            // For demonstration purposes - in production, use proper encryption
            // like AES with machine-specific keys or Windows Data Protection API
            var bytes = System.Text.Encoding.UTF8.GetBytes(data);
            for (int i = 0; i < bytes.Length; i++)
                {
                bytes[i] = (byte)(bytes[i] ^ 0xAA); // Simple XOR encryption
                }
            return bytes;
            }

        /// <summary>
        /// Simple decryption for token storage
        /// </summary>
        private string DecryptData(byte[] encryptedData)
            {
            // Reverse the simple XOR encryption
            for (int i = 0; i < encryptedData.Length; i++)
                {
                encryptedData[i] = (byte)(encryptedData[i] ^ 0xAA);
                }
            return System.Text.Encoding.UTF8.GetString(encryptedData);
            }

        /// <summary>
        /// Validates if a JWT token is still valid (basic check)
        /// </summary>
        private bool IsTokenValid(string token)
            {
            try
                {
                // Basic JWT token validation
                var parts = token.Split('.');
                if (parts.Length != 3)
                    return false;

                // Decode the payload to check expiration
                var payload = parts[1];
                // Add padding if needed for base64 decoding
                while (payload.Length % 4 != 0)
                    payload += "=";

                var payloadBytes = Convert.FromBase64String(payload);
                var payloadJson = System.Text.Encoding.UTF8.GetString(payloadBytes);
                var payloadData = JsonSerializer.Deserialize<Dictionary<string, object>>(payloadJson);

                if (payloadData?.ContainsKey("exp") == true)
                    {
                    var exp = JsonSerializer.Deserialize<long>(payloadData["exp"].ToString()!);
                    var expirationTime = DateTimeOffset.FromUnixTimeSeconds(exp);
                    return expirationTime > DateTimeOffset.UtcNow;
                    }

                return true; // If no expiration, consider valid
                }
            catch (Exception)
                {
                return false; // If parsing fails, consider invalid
                }
            }

        #endregion
        }
    }