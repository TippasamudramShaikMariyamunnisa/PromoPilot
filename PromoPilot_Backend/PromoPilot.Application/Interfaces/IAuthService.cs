using PromoPilot.Application.DTOs;
namespace PromoPilot.Application.Interfaces
{
    public interface IAuthService
    {
        Task<bool> RegisterAsync(RegisterRequest dto);
        Task<AuthResponse> LoginAsync(LoginRequest dto);
        Task<bool> LogoutAsync(string refreshToken);
        Task<AuthResponse> RefreshTokenAsync(string refreshToken);
    }
}