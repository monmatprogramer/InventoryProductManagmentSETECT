using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.Extensions.Configuration;
using System.Security.Claims;
using System.Text;
using InventoryPro.AuthService.Data;
using InventoryPro.AuthService.Models;
using System.Threading.Tasks;

namespace InventoryPro.AuthService.Services
    {
    /// <summary>
    /// Implementation of authentication service
    /// Handles user authentication, JWT token generation, and password management
    /// </summary>
    public class AuthService : IAuthService
        {
        private readonly AuthDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthService> _logger;

        public AuthService(AuthDbContext context, IConfiguration configuration, ILogger<AuthService> logger)
            {
            _context = context;
            _configuration = configuration;
            _logger = logger;
            }

        /// <summary>
        /// Authenticates user and returns JWT token
        /// </summary>
        public async Task<LoginResponse?> AuthenticateAsync(LoginRequest request)
            {
            try
                {
                // Find user by username
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Username == request.Username && u.IsActive);

                if (user == null)
                    {
                    _logger.LogWarning("Login attempt for non-existent user: {Username}", request.Username);
                    return null;
                    }

                // Verify password using BCrypt
                if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
                    {
                    _logger.LogWarning("Invalid password for user: {Username}", request.Username);
                    return null;
                    }

                // Generate JWT token
                var token = GenerateJwtToken(user);

                _logger.LogInformation("User {Username} logged in successfully", user.Username);

                return new LoginResponse
                    {
                    Token = token,
                    Username = user.Username,
                    Role = user.Role,
                    ExpiresAt = DateTime.UtcNow.AddHours(
                        Convert.ToDouble(_configuration["JwtSettings:ExpirationInHours"]))
                    };
                }
            catch (Exception ex)
                {
                _logger.LogError(ex, "Error during authentication");
                return null;
                }
            }

        /// <summary>
        /// Registers a new user
        /// </summary>
        public async Task<User?> RegisterAsync(User user, string password)
            {
            try
                {
                // Check if username already exists
                if (await _context.Users.AnyAsync(u => u.Username == user.Username))
                    {
                    _logger.LogWarning("Registration attempt with existing username: {Username}", user.Username);
                    return null;
                    }

                // Hash password
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(password);
                user.CreatedAt = DateTime.UtcNow;

                // Add user to database
                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                _logger.LogInformation("New user registered: {Username}", user.Username);
                return user;
                }
            catch (Exception ex)
                {
                _logger.LogError(ex, "Error during user registration");
                return null;
                }
            }

        /// <summary>
        /// Gets user by username
        /// </summary>
        public async Task<User?> GetUserByUsernameAsync(string username)
            {
            return await _context.Users
                .FirstOrDefaultAsync(u => u.Username == username && u.IsActive);
            }

        /// <summary>
        /// Changes user password
        /// </summary>
        public async Task<bool> ChangePasswordAsync(string username, string oldPassword, string newPassword)
            {
            try
                {
                var user = await GetUserByUsernameAsync(username);
                if (user == null) return false;

                // Verify old password
                if (!BCrypt.Net.BCrypt.Verify(oldPassword, user.PasswordHash))
                    return false;

                // Update password
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
                user.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Password changed for user: {Username}", username);
                return true;
                }
            catch (Exception ex)
                {
                _logger.LogError(ex, "Error changing password");
                return false;
                }
            }

        /// <summary>
        /// Generates JWT token for authenticated user
        /// </summary>
        private string GenerateJwtToken(User user)
            {
            var jwtSettings = _configuration.GetSection("JwtSettings");
            var securityKey = jwtSettings["Secret"] ?? throw new InvalidOperationException("JWT secret is not configured.");
            var key = Encoding.ASCII.GetBytes(securityKey);

            var tokenDescriptor = new SecurityTokenDescriptor
                {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Name, user.Username),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim(ClaimTypes.Role, user.Role)
                }),
                Expires = DateTime.UtcNow.AddHours(Convert.ToDouble(jwtSettings["ExpirationInHours"])),
                Issuer = jwtSettings["Issuer"],
                Audience = jwtSettings["Audience"],
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature)
                };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
            }
        }
    }
