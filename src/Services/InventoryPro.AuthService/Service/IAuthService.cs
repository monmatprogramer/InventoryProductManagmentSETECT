using InventoryPro.AuthService.Models;
using System.Threading.Tasks;

namespace InventoryPro.AuthService.Services
{
    /// <summary>
    /// Interface for authentication service
    /// </summary>
    public interface IAuthService
    {
        Task<LoginResponse?> AuthenticateAsync(LoginRequest request);
        Task<User?> RegisterAsync(User user, string password);
        Task<User?> GetUserByUsernameAsync(string username);
        Task<bool> ChangePasswordAsync(string username, string oldPassword, string newPassword);
        // Add other method signatures as needed, for example:
        // Task<User?> GetUserByIdAsync(int userId);
    }
}