using System.ComponentModel.DataAnnotations;
namespace InventoryPro.AuthService.Models
    {
    /// <summary>
    /// Login request model for authentication
    /// </summary>
    public class LoginRequest
        {
        [Required]
        public string Username { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;

        public bool RememberMe { get; set; } = false;
        }

    /// <summary>
    /// Response model after successful login
    /// </summary>
    public class LoginResponse
        {
        public string Token { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
        public DateTime Expires { get; set; }
        public UserInfo User { get; set; } = new();
        }

    /// <summary>
    /// User information model for login response
    /// </summary>
    public class UserInfo
        {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? LastLoginAt { get; set; }
        public bool IsActive { get; set; } = true;
        }
    }