using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.ComponentModel.DataAnnotations;
using InventoryPro.AuthService.Models;
using InventoryPro.AuthService.Services;

namespace InventoryPro.AuthService.Controllers
    {
    /// <summary>
    /// Authentication API controller
    /// Handles login, registration, and user management endpoints
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
        {
        private readonly IAuthService _authService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IAuthService authService, ILogger<AuthController> logger)
            {
            _authService = authService;
            _logger = logger;
            }

        /// <summary>
        /// User login endpoint
        /// </summary>
        /// <param name="request">Login credentials</param>
        /// <returns>JWT token if successful</returns>
        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
            {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var response = await _authService.AuthenticateAsync(request);

            if (response == null)
                return Unauthorized(new { message = "Invalid username or password" });

            return Ok(response);
            }

        /// <summary>
        /// User registration endpoint
        /// </summary>
        /// <param name="request">User registration data</param>
        /// <returns>Created user if successful</returns>
        [HttpPost("register")]
        [Authorize(Roles = "Admin")] // Only admins can create new users
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
            {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = new User
                {
                Username = request.Username,
                Email = request.Email,
                FirstName = request.FirstName,
                LastName = request.LastName,
                Role = request.Role
                };

            var result = await _authService.RegisterAsync(user, request.Password);

            if (result == null)
                return BadRequest(new { message = "Username already exists" });

            return CreatedAtAction(nameof(GetUser), new { username = result.Username }, result);
            }

        /// <summary>
        /// Get user by username
        /// </summary>
        [HttpGet("{username}")]
        [Authorize]
        public async Task<IActionResult> GetUser(string username)
            {
            var user = await _authService.GetUserByUsernameAsync(username);

            if (user == null)
                return NotFound();

            // Don't return password hash
            return Ok(new
                {
                user.Id,
                user.Username,
                user.Email,
                user.FirstName,
                user.LastName,
                user.Role,
                user.IsActive,
                user.CreatedAt
                });
            }

        /// <summary>
        /// Change password endpoint
        /// </summary>
        [HttpPost("change-password")]
        [Authorize]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
            {
            // Get username from JWT token
            var username = User.Identity?.Name;

            if (string.IsNullOrEmpty(username))
                return Unauthorized();

            var result = await _authService.ChangePasswordAsync(
                username,
                request.OldPassword,
                request.NewPassword);

            if (!result)
                return BadRequest(new { message = "Invalid old password" });

            return Ok(new { message = "Password changed successfully" });
            }
        }

    // Request models
    public class RegisterRequest
        {
        [Required]
        public string Username { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [MinLength(6)]
        public string Password { get; set; } = string.Empty;

        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;

        [Required]
        public string Role { get; set; } = "User";
        }

    public class ChangePasswordRequest
        {
        [Required]
        public string OldPassword { get; set; } = string.Empty;

        [Required]
        [MinLength(6)]
        public string NewPassword { get; set; } = string.Empty;
        }
    }