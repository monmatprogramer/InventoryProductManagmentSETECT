using InventoryPro.Shared.DTOs;

namespace InventoryPro.WinForms.Services
    {
    /// <summary>
    /// Interface for authentication service
    /// Manages JWT tokens and user authentication state
    /// </summary>
    public interface IAuthService
        {
        /// <summary>
        /// Event raised when authentication state changes
        /// </summary>
        event EventHandler<bool>? AuthenticationStateChanged;

        /// <summary>
        /// Gets the current authentication token
        /// </summary>
        Task<string?> GetTokenAsync();

        /// <summary>
        /// Sets the authentication token
        /// </summary>
        Task SetTokenAsync(string token);

        /// <summary>
        /// Clears the authentication token
        /// </summary>
        Task ClearTokenAsync();

        /// <summary>
        /// Checks if user is authenticated
        /// </summary>
        Task<bool> IsAuthenticatedAsync();

        /// <summary>
        /// Gets the current user information
        /// </summary>
        Task<UserDto?> GetCurrentUserAsync();

        /// <summary>
        /// Sets the current user information
        /// </summary>
        Task SetCurrentUserAsync(UserDto user);

        /// <summary>
        /// Clears the current user information
        /// </summary>
        Task ClearCurrentUserAsync();
        }
    }