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
        }

    /// <summary>
    /// Response model after successful login
    /// </summary>
    public class LoginResponse
        {
        public string Token { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
        }
    }