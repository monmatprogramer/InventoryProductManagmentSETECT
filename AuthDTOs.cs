using System.ComponentModel.DataAnnotations;

namespace InventoryPro.Shared.DTOs
    {
    /// <summary>
    /// Data Transfer Object for user login requests
    /// Contains username and password for authentication
    /// </summary>
    public class LoginRequestDto
        {
        [Required(ErrorMessage = "Username is required")]
        [StringLength(50, ErrorMessage = "Username cannot exceed 50 characters")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be between 6 and 100 characters")]
        public string Password { get; set; } = string.Empty;

        public bool RememberMe { get; set; } = false;
        }

    /// <summary>
    /// Data Transfer Object for authentication responses
    /// Contains JWT token and user information after successful login
    /// </summary>
    public class LoginResponseDto
        {
        public string Token { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
        public DateTime Expires { get; set; }
        public UserDto User { get; set; } = new();
        }

    /// <summary>
    /// Data Transfer Object for user registration
    /// Contains all necessary information to create a new user account
    /// </summary>
    public class RegisterRequestDto
        {
        [Required(ErrorMessage = "Username is required")]
        [StringLength(50, ErrorMessage = "Username cannot exceed 50 characters")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be between 6 and 100 characters")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password confirmation is required")]
        [Compare("Password", ErrorMessage = "Passwords do not match")]
        public string ConfirmPassword { get; set; } = string.Empty;

        public string Role { get; set; } = "User"; // Default role
        }

    /// <summary>
    /// Data Transfer Object for user information
    /// Used to transfer user data without sensitive information like passwords
    /// </summary>
    public class UserDto
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
